using UnityEngine;
using MoreMountains.Tools; // �� ������������� ������ ���� MoreMountains
using TMPro; // ��� TextMeshPro
using UnityEngine.UI; // ��� ������� �� Image, ���� �����������

public class PlayerUIController : MonoBehaviour
{
    [Header("Player Data Source")]
    [Tooltip("��������� ���� ��'��� ������, �� ����� ������ PlayerResourceController")]
    [SerializeField] private PlayerResourceController playerResources;

    [Header("UI Bars")]
    [Tooltip("��������� ���� MMProgressBar ��� ������")]
    // ������� ������ ����, ��� �������� ��������� � ����� ��������
    [SerializeField] private MoreMountains.Tools.MMProgressBar fuelBar;

    [Tooltip("��������� ���� MMProgressBar ��� �������")]
    [SerializeField] private MoreMountains.Tools.MMProgressBar boostBar;

    [Header("UI Text (Optional)")]
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private TextMeshProUGUI boostText;

    private bool wasBoostingLastFrame = false;

    // �� ��������� �� ���������� ��� ������� �������
    private Image _boostForegroundImage;
    private Image _fuelForegroundImage;

    void Start()
    {
        if (playerResources == null)
        {
            Debug.LogError("PlayerResourceController �� ���������� � PlayerUIController! ������ ��������.", this);
            enabled = false;
            return;
        }

        // --- �������� ���������� ��� ������� ������� ---
        if (boostBar != null && boostBar.ForegroundBar != null)
        {
            _boostForegroundImage = boostBar.ForegroundBar.GetComponent<Image>();
        }
        if (fuelBar != null && fuelBar.ForegroundBar != null)
        {
            _fuelForegroundImage = fuelBar.ForegroundBar.GetComponent<Image>();
        }

        // --- ϳ��������� �� ��䳿 ---
        playerResources.OnBoostAdded.AddListener(HandleBoostAdded);
        playerResources.OnFuelAdded.AddListener(HandleFuelAdded); // <--- ����: ϳ������ �� ���� ������

        // --- ���������� ���� ---
        InitializeBars();
    }

    void OnDestroy()
    {
        if (playerResources != null)
        {
            playerResources.OnBoostAdded.RemoveListener(HandleBoostAdded);
            playerResources.OnFuelAdded.RemoveListener(HandleFuelAdded); // <--- ����: ³������
        }
    }

    /// <summary>
    /// ���������� ���������� ���� UI (����������� ���� ��� � Start)
    /// </summary>
    private void InitializeBars()
    {
        // ������������� SetBar, ��� ������ ���������� �Ѳ ������
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
    /// ������������ ��Ĳ��: ���� ������� ����
    /// </summary>
    public void HandleBoostAdded()
    {
        if (boostBar != null)
        {
            // ������������� UpdateBar, ��� ��������� "����" �� ������� "������������"
            boostBar.UpdateBar(playerResources.CurrentBoost, 0f, playerResources.MaxBoost);
        }
    }

    /// <summary>
    /// ����� ����� - ������������ ��Ĳ��: ���� ������� ������
    /// </summary>
    public void HandleFuelAdded()
    {
        if (fuelBar != null)
        {
            // ������������� UpdateBar, ��� ��������� "����"
            fuelBar.UpdateBar(playerResources.CurrentFuel, 0f, playerResources.MaxFuel);
        }
    }

    /// <summary>
    /// ������������ � UPDATE: ��� ����� "����������" �� "������������" ����
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
            // 1. �����в�: "����������" (Shift ����������)
            // �� ������ ������ Ҳ���� ������� ������, ��������� `SetBar`
            ManuallySetBarFill(boostBar, _boostForegroundImage, normalizedBoost);
        }
        else if (wasBoostingLastFrame && !isBoostingNow)
        {
            // 2. �����в�: "������������" (����� ��������� Shift)
            // ��������� "������" UpdateBar, ��� �������� ������� ������
            boostBar.UpdateBar(playerResources.CurrentBoost, 0f, playerResources.MaxBoost);
        }

        wasBoostingLastFrame = isBoostingNow;
        UpdateBoostText();
    }

    /// <summary>
    /// ������������ � UPDATE: ��������� ������
    /// </summary>
    private void UpdateFuelBar()
    {
        // ��������: �� ����� �� ������������� ������ ����� ManuallySetBarFill
        // �� ���� ��������� ("��������� �� ���� ��������").
        // �� ��������� SetBar, ���� ������ ������� �Ѳ ������ (���, �������)
        // �� ������ �������� ��� "�����". �� �������� ��� ����������.
        if (fuelBar != null)
        {
            fuelBar.SetBar(playerResources.CurrentFuel, 0f, playerResources.MaxFuel);
            UpdateFuelText();
        }
    }

    /// <summary>
    /// ��� ������� "�����" �����, ���� ���� Ҳ���� ������� ������ (��� Ͳ���)
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

    // --- ������� ������ ��� ��������� ������ ---

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

