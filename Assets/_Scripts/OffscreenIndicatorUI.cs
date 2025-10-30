using UnityEngine;
using UnityEngine.UI; // Для роботи з Image

public class OffscreenIndicatorUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Перетягни сюди RectTransform UI-елемента, який буде індикатором")]
    [SerializeField] private RectTransform indicatorRectTransform;
    [Tooltip("Перетягни сюди об'єкт гравця (машину)")]
    [SerializeField] private Transform playerTransform;

    [Header("Settings")]
    [Tooltip("Відступ від краю екрана в пікселях")]
    [SerializeField] private float screenBorderMargin = 50f;

    private Camera mainCamera;
    private Transform target; // Поточна ціль (найближча монета)

    void Start()
    {
        mainCamera = Camera.main;
        if (indicatorRectTransform == null || playerTransform == null)
        {
            Debug.LogError("Не всі посилання вказані в OffscreenIndicatorUI! Скрипт вимкнено.", this);
            enabled = false;
            return;
        }
        // Ховаємо індикатор на старті
        indicatorRectTransform.gameObject.SetActive(false);
    }

    void Update()
    {
        // Знаходимо найближчу монету в кожному кадрі
        if (CoinManager.Instance != null)
        {
            target = CoinManager.Instance.GetClosestCoin(playerTransform.position);
        }

        // Якщо монет немає, ховаємо індикатор і виходимо
        if (target == null)
        {
            indicatorRectTransform.gameObject.SetActive(false);
            return;
        }

        // --- Основна логіка ---

        // 1. Конвертуємо світову позицію монети в позицію на екрані
        Vector3 targetScreenPosition = mainCamera.WorldToScreenPoint(target.position);

        // 2. Перевіряємо, чи ціль видима на екрані
        bool isOffScreen = targetScreenPosition.z <= 0 ||
                           targetScreenPosition.x <= screenBorderMargin ||
                           targetScreenPosition.x >= Screen.width - screenBorderMargin ||
                           targetScreenPosition.y <= screenBorderMargin ||
                           targetScreenPosition.y >= Screen.height - screenBorderMargin;

        if (isOffScreen)
        {
            indicatorRectTransform.gameObject.SetActive(true);

            // 3. Обмежуємо позицію індикатора в межах екрана (квадратний радіус)
            Vector3 cappedTargetScreenPosition = targetScreenPosition;
            if (cappedTargetScreenPosition.z < 0)
            {
                // Якщо ціль за камерою, інвертуємо позицію, щоб індикатор показував назад
                cappedTargetScreenPosition *= -1;
            }

            // Знаходимо центр екрана
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2f;

            // Напрямок від центру екрана до цілі
            cappedTargetScreenPosition -= screenCenter;

            // Визначаємо, чи ми ближче до вертикального чи горизонтального краю
            float slope = cappedTargetScreenPosition.y / cappedTargetScreenPosition.x;
            float screenSlope = (float)Screen.height / Screen.width;

            if (Mathf.Abs(slope) > screenSlope) // Ближче до верхнього/нижнього краю
            {
                // Обмежуємо по Y
                float y = Mathf.Sign(cappedTargetScreenPosition.y) * (Screen.height / 2f - screenBorderMargin);
                cappedTargetScreenPosition = new Vector3(y / slope, y, 0);
            }
            else // Ближче до бічних країв
            {
                // Обмежуємо по X
                float x = Mathf.Sign(cappedTargetScreenPosition.x) * (Screen.width / 2f - screenBorderMargin);
                cappedTargetScreenPosition = new Vector3(x, x * slope, 0);
            }

            // Повертаємо координати відносно лівого нижнього кута
            indicatorRectTransform.position = screenCenter + cappedTargetScreenPosition;

            // 4. Повертаємо індикатор в напрямку цілі
            float angle = Mathf.Atan2(cappedTargetScreenPosition.y, cappedTargetScreenPosition.x) * Mathf.Rad2Deg;
            indicatorRectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            indicatorRectTransform.gameObject.SetActive(false);
        }
    }
}