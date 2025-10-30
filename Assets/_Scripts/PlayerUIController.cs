using UnityEngine;
using MoreMountains.Tools; // Ми використовуємо простір імен MoreMountains
using TMPro; // Для TextMeshPro
using UnityEngine.UI; // Для доступу до Image, якщо знадобиться

public class PlayerUIController : MonoBehaviour
{
    [Header("Player Data Source")]
    [Tooltip("Перетягни сюди об'єкт гравця, на якому висить PlayerResourceController")]
    [SerializeField] private PlayerResourceController playerResources;

    [Header("UI Bars")]
    [Tooltip("Перетягни сюди MMProgressBar для палива")]
    // Вказуємо повний шлях, щоб уникнути плутанини з іншим скриптом
    [SerializeField] private MoreMountains.Tools.MMProgressBar fuelBar;

    [Tooltip("Перетягни сюди MMProgressBar для форсажу")]
    [SerializeField] private MoreMountains.Tools.MMProgressBar boostBar;

    [Header("UI Text (Optional)")]
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private TextMeshProUGUI boostText;

    private bool wasBoostingLastFrame = false;

    // Ми збережемо ці компоненти для прямого доступу
    private Image _boostForegroundImage;
    private Image _fuelForegroundImage;

    void Start()
    {
        if (playerResources == null)
        {
            Debug.LogError("PlayerResourceController не призначено в PlayerUIController! Скрипт вимкнено.", this);
            enabled = false;
            return;
        }

        // --- Отримуємо компоненти для прямого доступу ---
        if (boostBar != null && boostBar.ForegroundBar != null)
        {
            _boostForegroundImage = boostBar.ForegroundBar.GetComponent<Image>();
        }
        if (fuelBar != null && fuelBar.ForegroundBar != null)
        {
            _fuelForegroundImage = fuelBar.ForegroundBar.GetComponent<Image>();
        }

        // --- Підписуємось на події ---
        playerResources.OnBoostAdded.AddListener(HandleBoostAdded);
        playerResources.OnFuelAdded.AddListener(HandleFuelAdded); // <--- НОВЕ: Підписка на подію палива

        // --- Ініціалізуємо бари ---
        InitializeBars();
    }

    void OnDestroy()
    {
        if (playerResources != null)
        {
            playerResources.OnBoostAdded.RemoveListener(HandleBoostAdded);
            playerResources.OnFuelAdded.RemoveListener(HandleFuelAdded); // <--- НОВЕ: Відписка
        }
    }

    /// <summary>
    /// Встановлює початковий стан UI (викликається ОДИН РАЗ в Start)
    /// </summary>
    private void InitializeBars()
    {
        // Використовуємо SetBar, щоб миттєво встановити ВСІ смужки
        if (fuelBar != null)
        {
            fuelBar.SetBar(playerResources.CurrentFuel, 0f, playerResources.MaxFuel);
        }
        if (boostBar != null)
        {
            boostBar.SetBar(playerResources.CurrentBoost, 0f, playerResources.MaxBoost);
        }

        UpdateFuelText();
        UpdateBoostText();
    }

    void Update()
    {
        UpdateFuelBar();
        UpdateBoostBarLogic();
    }

    /// <summary>
    /// ВИКЛИКАЄТЬСЯ ПОДІЄЮ: Коли підібрали нітро
    /// </summary>
    public void HandleBoostAdded()
    {
        if (boostBar != null)
        {
            // Використовуємо UpdateBar, щоб запустити "бамп" та анімацію "наздоганяння"
            boostBar.UpdateBar(playerResources.CurrentBoost, 0f, playerResources.MaxBoost);
        }
    }

    /// <summary>
    /// НОВИЙ МЕТОД - ВИКЛИКАЄТЬСЯ ПОДІЄЮ: Коли підібрали паливо
    /// </summary>
    public void HandleFuelAdded()
    {
        if (fuelBar != null)
        {
            // Використовуємо UpdateBar, щоб запустити "бамп"
            fuelBar.UpdateBar(playerResources.CurrentFuel, 0f, playerResources.MaxFuel);
        }
    }

    /// <summary>
    /// ВИКЛИКАЄТЬСЯ В UPDATE: Вся логіка "спалювання" та "наздоганяння" нітро
    /// </summary>
    private void UpdateBoostBarLogic()
    {
        if (boostBar == null) return;

        bool isBoostingNow = playerResources.IsBoosting;
        float normalizedBoost = 0f;

        if (playerResources.MaxBoost > 0)
        {
            normalizedBoost = playerResources.CurrentBoost / playerResources.MaxBoost;
        }

        if (isBoostingNow)
        {
            // 1. СЦЕНАРІЙ: "СПАЛЮВАННЯ" (Shift затиснутий)
            // Ми вручну рухаємо ТІЛЬКИ передню смужку, ігноруючи `SetBar`
            ManuallySetBarFill(boostBar, _boostForegroundImage, normalizedBoost);
        }
        else if (wasBoostingLastFrame && !isBoostingNow)
        {
            // 2. СЦЕНАРІЙ: "НАЗДОГАНЯННЯ" (Щойно відпустили Shift)
            // Викликаємо "гучний" UpdateBar, щоб анімувати червону смужку
            boostBar.UpdateBar(playerResources.CurrentBoost, 0f, playerResources.MaxBoost);
        }

        wasBoostingLastFrame = isBoostingNow;
        UpdateBoostText();
    }

    /// <summary>
    /// ВИКЛИКАЄТЬСЯ В UPDATE: Оновлення палива
    /// </summary>
    private void UpdateFuelBar()
    {
        // ОНОВЛЕНО: Ми більше не використовуємо ручний метод ManuallySetBarFill
        // за твоїм проханням ("апдейтити всі бари одночано").
        // Ми викликаємо SetBar, який миттєво оновить ВСІ смужки (білу, червону)
        // до нового значення без "бампу". Це ідеально для спалювання.
        if (fuelBar != null)
        {
            fuelBar.SetBar(playerResources.CurrentFuel, 0f, playerResources.MaxFuel);
            UpdateFuelText();
        }
    }

    /// <summary>
    /// Наш власний "тихий" метод, який рухає ТІЛЬКИ передню смужку (ДЛЯ НІТРО)
    /// </summary>
    private void ManuallySetBarFill(MoreMountains.Tools.MMProgressBar bar, Image foregroundImage, float normalizedValue)
    {
        if (bar.ForegroundBar == null) return;

        float remappedValue = bar.MinimumBarFillValue + normalizedValue * (bar.MaximumBarFillValue - bar.MinimumBarFillValue);

        switch (bar.FillMode)
        {
            case MoreMountains.Tools.MMProgressBar.FillModes.LocalScale:
                Vector3 newScale = bar.ForegroundBar.localScale;
                switch (bar.BarDirection)
                {
                    case MoreMountains.Tools.MMProgressBar.BarDirections.DownToUp:
                        newScale.y = remappedValue;
                        break;
                    case MoreMountains.Tools.MMProgressBar.BarDirections.UpToDown:
                        newScale.y = 1f - remappedValue;
                        break;
                    case MoreMountains.Tools.MMProgressBar.BarDirections.LeftToRight:
                        newScale.x = remappedValue;
                        break;
                    case MoreMountains.Tools.MMProgressBar.BarDirections.RightToLeft:
                        newScale.x = 1f - remappedValue;
                        break;
                }
                bar.ForegroundBar.localScale = newScale;
                break;

            case MoreMountains.Tools.MMProgressBar.FillModes.FillAmount:
                if (foregroundImage != null)
                {
                    foregroundImage.fillAmount = remappedValue;
                }
                break;
        }
    }

    // --- Допоміжні методи для оновлення тексту ---

    private void UpdateFuelText()
    {
        if (fuelText != null && playerResources.MaxFuel > 0)
        {
            float normalizedFuel = playerResources.CurrentFuel / playerResources.MaxFuel;
            fuelText.text = $"{Mathf.CeilToInt(normalizedFuel * 100)}%";
        }
    }

    private void UpdateBoostText()
    {
        if (boostText != null && playerResources.MaxBoost > 0)
        {
            float normalizedBoost = playerResources.CurrentBoost / playerResources.MaxBoost;
            boostText.text = $"{Mathf.CeilToInt(normalizedBoost * 100)}%";
        }
    }
}

