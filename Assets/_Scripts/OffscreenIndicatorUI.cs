using UnityEngine;
using UnityEngine.UI; // ��� ������ � Image

public class OffscreenIndicatorUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("��������� ���� RectTransform UI-��������, ���� ���� �����������")]
    [SerializeField] private RectTransform indicatorRectTransform;
    [Tooltip("��������� ���� ��'��� ������ (������)")]
    [SerializeField] private Transform playerTransform;

    [Header("Settings")]
    [Tooltip("³����� �� ���� ������ � �������")]
    [SerializeField] private float screenBorderMargin = 50f;

    private Camera mainCamera;
    private Transform target; // ������� ���� (��������� ������)

    void Start()
    {
        mainCamera = Camera.main;
        if (indicatorRectTransform == null || playerTransform == null)
        {
            Debug.LogError("�� �� ��������� ������ � OffscreenIndicatorUI! ������ ��������.", this);
            enabled = false;
            return;
        }
        // ������ ��������� �� �����
        indicatorRectTransform.gameObject.SetActive(false);
    }

    void Update()
    {
        // ��������� ��������� ������ � ������� ����
        if (CoinManager.Instance != null)
        {
            target = CoinManager.Instance.GetClosestCoin(playerTransform.position);
        }

        // ���� ����� ����, ������ ��������� � ��������
        if (target == null)
        {
            indicatorRectTransform.gameObject.SetActive(false);
            return;
        }

        // --- ������� ����� ---

        // 1. ���������� ������ ������� ������ � ������� �� �����
        Vector3 targetScreenPosition = mainCamera.WorldToScreenPoint(target.position);

        // 2. ����������, �� ���� ������ �� �����
        bool isOffScreen = targetScreenPosition.z <= 0 ||
                           targetScreenPosition.x <= screenBorderMargin ||
                           targetScreenPosition.x >= Screen.width - screenBorderMargin ||
                           targetScreenPosition.y <= screenBorderMargin ||
                           targetScreenPosition.y >= Screen.height - screenBorderMargin;

        if (isOffScreen)
        {
            indicatorRectTransform.gameObject.SetActive(true);

            // 3. �������� ������� ���������� � ����� ������ (���������� �����)
            Vector3 cappedTargetScreenPosition = targetScreenPosition;
            if (cappedTargetScreenPosition.z < 0)
            {
                // ���� ���� �� �������, ��������� �������, ��� ��������� ��������� �����
                cappedTargetScreenPosition *= -1;
            }

            // ��������� ����� ������
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2f;

            // �������� �� ������ ������ �� ���
            cappedTargetScreenPosition -= screenCenter;

            // ���������, �� �� ������ �� ������������� �� ��������������� ����
            float slope = cappedTargetScreenPosition.y / cappedTargetScreenPosition.x;
            float screenSlope = (float)Screen.height / Screen.width;

            if (Mathf.Abs(slope) > screenSlope) // ������ �� ���������/�������� ����
            {
                // �������� �� Y
                float y = Mathf.Sign(cappedTargetScreenPosition.y) * (Screen.height / 2f - screenBorderMargin);
                cappedTargetScreenPosition = new Vector3(y / slope, y, 0);
            }
            else // ������ �� ����� ����
            {
                // �������� �� X
                float x = Mathf.Sign(cappedTargetScreenPosition.x) * (Screen.width / 2f - screenBorderMargin);
                cappedTargetScreenPosition = new Vector3(x, x * slope, 0);
            }

            // ��������� ���������� ������� ����� �������� ����
            indicatorRectTransform.position = screenCenter + cappedTargetScreenPosition;

            // 4. ��������� ��������� � �������� ���
            float angle = Mathf.Atan2(cappedTargetScreenPosition.y, cappedTargetScreenPosition.x) * Mathf.Rad2Deg;
            indicatorRectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            indicatorRectTransform.gameObject.SetActive(false);
        }
    }
}