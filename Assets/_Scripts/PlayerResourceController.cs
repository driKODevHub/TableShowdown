using UnityEngine;
using UnityEngine.Events; // <--- ДОДАНО
using ArcadeVP; // Додаємо простір імен асету, щоб мати доступ до контролера

public class PlayerResourceController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private ArcadeVehicleController arcadeVehicleController;

    [Header("Fuel Settings")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float fuelConsumptionRate = 0.5f; // Палива за секунду
    private float currentFuel;

    [Header("Boost Settings")]
    [SerializeField] private float maxBoost = 100f;
    [SerializeField] private float boostConsumptionRate = 10f; // Форсажу за секунду
    [SerializeField] private float boostMultiplier = 1.75f; // Наскільки сильніший буст
    private float currentBoost;

    [Header("UI Events")]
    public UnityEvent OnBoostAdded;
    public UnityEvent OnFuelAdded; // <--- НОВЕ: Подія для UI при підборі палива

    // Зберігаємо оригінальні значення з контролера
    private float originalMaxSpeed;
    private float originalAcceleration;
    private bool isBoosting = false;

    // --- Публічні властивості (для UI або інших систем) ---
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public float CurrentBoost => currentBoost;
    public float MaxBoost => maxBoost;
    public bool IsBoosting => isBoosting;

    // --- Ініціалізація ---
    // Awake() викликається раніше за Start()
    void Awake()
    {
        // Автоматично знаходимо контролер, якщо він не вказаний в інспекторі
        if (arcadeVehicleController == null)
            arcadeVehicleController = GetComponent<ArcadeVehicleController>();

        // Зберігаємо початкові налаштування машини
        if (arcadeVehicleController != null)
        {
            originalMaxSpeed = arcadeVehicleController.MaxSpeed;
            originalAcceleration = arcadeVehicleController.accelaration;
        }
        else
        {
            Debug.LogError("ArcadeVehicleController не знайдено!", this);
        }

        // Встановлюємо початкові значення
        currentFuel = maxFuel;
        currentBoost = 25f; // Почнемо з невеликою кількістю бусту
    }

    void Update()
    {
        // Порядок важливий: спочатку обробляємо паливо, потім буст
        HandleFuel();
        HandleBoost();
    }

    private void HandleFuel()
    {
        if (currentFuel > 0)
        {
            // Перевіряємо, чи машина рухається (використовуємо carVelocity з контролера)
            // і чи натиснута педаль газу (Vertical > 0)
            bool isMoving = arcadeVehicleController.carVelocity.magnitude > 0.5f;
            bool isAccelerating = Input.GetAxis("Vertical") > 0.1f;

            if (isMoving && isAccelerating)
            {
                currentFuel -= fuelConsumptionRate * Time.deltaTime;
            }

            // Дозволяємо машині прискорюватись
            arcadeVehicleController.accelaration = originalAcceleration;
        }
        else
        {
            // Паливо скінчилося
            currentFuel = 0;

            // Забороняємо машині прискорюватись, встановлюючи прискорення в 0
            arcadeVehicleController.accelaration = 0;

            // Якщо ми бустили, коли скінчилось паливо, зупиняємо буст
            if (isBoosting)
            {
                StopBoosting();
            }
        }
    }

    private void HandleBoost()
    {
        // Перевіряємо натискання Shift, наявність бусту та наявність палива
        if (Input.GetKey(KeyCode.LeftShift) && currentBoost > 0 && currentFuel > 0)
        {
            isBoosting = true;
            currentBoost -= boostConsumptionRate * Time.deltaTime;

            // Збільшуємо максимальну швидкість машини
            arcadeVehicleController.MaxSpeed = originalMaxSpeed * boostMultiplier;

            // Тут можна додати ефекти "Feel" (наприклад, PlayFeedbacks)
        }
        else if (isBoosting)
        {
            // Якщо клавіша відпущена, або скінчився буст/паливо
            StopBoosting();
        }
    }

    private void StopBoosting()
    {
        isBoosting = false;
        // Повертаємо швидкість до норми
        arcadeVehicleController.MaxSpeed = originalMaxSpeed;

        // Тут можна зупинити ефекти "Feel" (наприклад, StopFeedbacks)
    }

    // --- Публічні методи для пікапів ---

    public void AddFuel(float amount)
    {
        if (currentFuel >= maxFuel) return; // Не додаємо, якщо повно

        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
        OnFuelAdded?.Invoke(); // <--- НОВЕ: Викликаємо подію "бампу"
    }

    public void AddBoost(float amount)
    {
        // Виконуємо перевірку, як ти і просив: 
        // не додаємо, якщо форсаж вже повний
        if (currentBoost >= maxBoost)
            return;

        currentBoost = Mathf.Min(currentBoost + amount, maxBoost);

        // ВИКЛИКАЄМО ПОДІЮ!
        OnBoostAdded?.Invoke(); // Повідомляємо UI, що треба зробити "бамп"
    }
}

