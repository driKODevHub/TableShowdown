using UnityEngine;
using UnityEngine.Events;
using ArcadeVP; 

public class PlayerResourceController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private ArcadeVehicleController arcadeVehicleController;

    [Header("Fuel Settings")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float fuelConsumptionRate = 0.5f; 
    private float currentFuel;

    [Header("Boost Settings")]
    [SerializeField] private float maxBoost = 100f;
    [SerializeField] private float boostConsumptionRate = 10f; 
    [SerializeField] private float boostMultiplier = 1.75f; 
    private float currentBoost;

    private float originalMaxSpeed;
    private float originalAcceleration;
    private bool isBoosting = false;

    [Header("UI Events")]
    public UnityEvent OnBoostAdded; 

    // --- Публічні властивості (для UI або інших систем) ---
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public float CurrentBoost => currentBoost;
    public float MaxBoost => maxBoost;
    public bool IsBoosting => isBoosting; 

    // --------------------------------------------------------

    // ВИПРАВЛЕННЯ 2: Використовуємо Awake() замість Start()
    // Awake() завжди виконується до Start().
    // Це гарантує, що коли PlayerUIController викличе Start(), 
    // ці значення ВЖЕ будуть встановлені.
    void Awake()
    {
        // Автоматично знаходимо контролер, якщо він не вказаний в інспекторі
        if (arcadeVehicleController == null)
            arcadeVehicleController = GetComponent<ArcadeVehicleController>();

        // Зберігаємо початкові налаштування машини
        originalMaxSpeed = arcadeVehicleController.MaxSpeed;
        originalAcceleration = arcadeVehicleController.accelaration;

        // Встановлюємо початкові значення
        currentFuel = maxFuel;
        currentBoost = 25f; // Тепер це значення буде готове для Start() в UI
    }

    void Update()
    {
        HandleFuel();
        HandleBoost();
    }

    private void HandleFuel()
    {
        if (currentFuel > 0)
        {
            bool isMoving = arcadeVehicleController.carVelocity.magnitude > 0.5f;
            bool isAccelerating = Input.GetAxis("Vertical") > 0.1f;

            if (isMoving && isAccelerating)
            {
                currentFuel -= fuelConsumptionRate * Time.deltaTime;
            }

            arcadeVehicleController.accelaration = originalAcceleration;
        }
        else
        {
            currentFuel = 0;
            arcadeVehicleController.accelaration = 0;

            if (isBoosting)
            {
                StopBoosting();
            }
        }
    }

    private void HandleBoost()
    {
        if (Input.GetKey(KeyCode.LeftShift) && currentBoost > 0 && currentFuel > 0)
        {
            isBoosting = true;
            currentBoost -= boostConsumptionRate * Time.deltaTime;
            arcadeVehicleController.MaxSpeed = originalMaxSpeed * boostMultiplier;
        }
        else if (isBoosting)
        {
            StopBoosting();
        }
    }

    private void StopBoosting()
    {
        isBoosting = false;
        arcadeVehicleController.MaxSpeed = originalMaxSpeed;
    }

    // --- Публічні методи для пікапів ---

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
    }

    public void AddBoost(float amount)
    {
        if (currentBoost >= maxBoost)
            return;

        currentBoost = Mathf.Min(currentBoost + amount, maxBoost);
        OnBoostAdded?.Invoke();
    }
}

