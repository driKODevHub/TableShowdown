using UnityEngine;
using UnityEngine.Events; // <--- ������
using ArcadeVP; // ������ ������ ���� �����, ��� ���� ������ �� ����������

public class PlayerResourceController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private ArcadeVehicleController arcadeVehicleController;

    [Header("Fuel Settings")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float fuelConsumptionRate = 0.5f; // ������ �� �������
    private float currentFuel;

    [Header("Boost Settings")]
    [SerializeField] private float maxBoost = 100f;
    [SerializeField] private float boostConsumptionRate = 10f; // ������� �� �������
    [SerializeField] private float boostMultiplier = 1.75f; // �������� �������� ����
    private float currentBoost;

    [Header("UI Events")]
    public UnityEvent OnBoostAdded;
    public UnityEvent OnFuelAdded; // <--- ����: ���� ��� UI ��� ����� ������

    // �������� ��������� �������� � ����������
    private float originalMaxSpeed;
    private float originalAcceleration;
    private bool isBoosting = false;

    // --- ������ ���������� (��� UI ��� ����� ������) ---
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public float CurrentBoost => currentBoost;
    public float MaxBoost => maxBoost;
    public bool IsBoosting => isBoosting;

    // --- ����������� ---
    // Awake() ����������� ����� �� Start()
    void Awake()
    {
        // ����������� ��������� ���������, ���� �� �� �������� � ���������
        if (arcadeVehicleController == null)
            arcadeVehicleController = GetComponent<ArcadeVehicleController>();

        // �������� �������� ������������ ������
        if (arcadeVehicleController != null)
        {
            originalMaxSpeed = arcadeVehicleController.MaxSpeed;
            originalAcceleration = arcadeVehicleController.accelaration;
        }
        else
        {
            Debug.LogError("ArcadeVehicleController �� ��������!", this);
        }

        // ������������ �������� ��������
        currentFuel = maxFuel;
        currentBoost = 25f; // ������� � ��������� ������� �����
    }

    void Update()
    {
        // ������� ��������: �������� ���������� ������, ���� ����
        HandleFuel();
        HandleBoost();
    }

    private void HandleFuel()
    {
        if (currentFuel > 0)
        {
            // ����������, �� ������ �������� (������������� carVelocity � ����������)
            // � �� ��������� ������ ���� (Vertical > 0)
            bool isMoving = arcadeVehicleController.carVelocity.magnitude > 0.5f;
            bool isAccelerating = Input.GetAxis("Vertical") > 0.1f;

            if (isMoving && isAccelerating)
            {
                currentFuel -= fuelConsumptionRate * Time.deltaTime;
            }

            // ���������� ����� ��������������
            arcadeVehicleController.accelaration = originalAcceleration;
        }
        else
        {
            // ������ ���������
            currentFuel = 0;

            // ����������� ����� ��������������, ������������ ����������� � 0
            arcadeVehicleController.accelaration = 0;

            // ���� �� �������, ���� ��������� ������, ��������� ����
            if (isBoosting)
            {
                StopBoosting();
            }
        }
    }

    private void HandleBoost()
    {
        // ���������� ���������� Shift, �������� ����� �� �������� ������
        if (Input.GetKey(KeyCode.LeftShift) && currentBoost > 0 && currentFuel > 0)
        {
            isBoosting = true;
            currentBoost -= boostConsumptionRate * Time.deltaTime;

            // �������� ����������� �������� ������
            arcadeVehicleController.MaxSpeed = originalMaxSpeed * boostMultiplier;

            // ��� ����� ������ ������ "Feel" (���������, PlayFeedbacks)
        }
        else if (isBoosting)
        {
            // ���� ������ ��������, ��� �������� ����/������
            StopBoosting();
        }
    }

    private void StopBoosting()
    {
        isBoosting = false;
        // ��������� �������� �� �����
        arcadeVehicleController.MaxSpeed = originalMaxSpeed;

        // ��� ����� �������� ������ "Feel" (���������, StopFeedbacks)
    }

    // --- ������ ������ ��� ����� ---

    public void AddFuel(float amount)
    {
        if (currentFuel >= maxFuel) return; // �� ������, ���� �����

        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
        OnFuelAdded?.Invoke(); // <--- ����: ��������� ���� "�����"
    }

    public void AddBoost(float amount)
    {
        // �������� ��������, �� �� � ������: 
        // �� ������, ���� ������ ��� ������
        if (currentBoost >= maxBoost)
            return;

        currentBoost = Mathf.Min(currentBoost + amount, maxBoost);

        // ���������� ��Ĳ�!
        OnBoostAdded?.Invoke(); // ����������� UI, �� ����� ������� "����"
    }
}

