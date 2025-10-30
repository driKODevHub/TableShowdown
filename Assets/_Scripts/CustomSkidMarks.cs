using UnityEngine;
using ArcadeVP; // ��� ������� ��� namespace, ��� ������ ��� � ����������

// �������: �� ��� ��������� ������, �� � �����.
[RequireComponent(typeof(TrailRenderer), typeof(ParticleSystem))]
public class CustomSkidMarks : MonoBehaviour
{
    // ��������� �� ����������, �� �� �������� �����������
    private TrailRenderer skidMark;
    private ParticleSystem smoke;

    // ��������� �� ��������� ������ (������� ����������� � ���������)
    [Header("Core References")]
    [Tooltip("��������� ���� �������� ��'��� ������ � ArcadeVehicleController")]
    [SerializeField] private ArcadeVehicleController carController;

    // ���� ��� ������
    private float verticalInput; // W/S
    private float brakeInput; // Space (Jump)
    private bool isBoosting; // Left Shift

    [Header("Skid Settings")]
    [Tooltip("̳������� ������ �������� (����) ��� ����� ����.")]
    [SerializeField] private float minDriftSpeed = 10f;
    [Tooltip("̳������� �������� �������� (������/�����) ��� ����� ���� ��� ��������� 'Space'.")]
    [SerializeField] private float minBrakingSpeed = 20f;
    [Tooltip("̳������� �������� (������) ��� ����� ���� ��� ���������� ����� ����� (S).")]
    [SerializeField] private float minReverseBrakingSpeed = 15f;

    // --- ��² ������������ ---
    [Header("Burnout Settings")]
    [Tooltip("����������� ��������, ��� ��� 'burnout' (����� � ����) ������ ����.")]
    [SerializeField] private float maxBurnoutSpeed = 30f;


    // ����� ��� ����� ���������
    private float fadeOutSpeed;

    private void Awake()
    {
        // �������� ����������, �� ������ �� ����� � ��'���
        smoke = GetComponent<ParticleSystem>();
        skidMark = GetComponent<TrailRenderer>();

        // �������� ��������: ��� ���������� ������ �� �� �����
        if (carController == null)
        {
            Debug.LogError("CustomSkidMarks: 'Car Controller' �� ���������� � ���������! ������ ��������.", this);
            enabled = false;
            return;
        }

        // �� ������ ������� skidWidth, �� �� public ����
        skidMark.startWidth = carController.skidWidth;
        skidMark.emitting = false;
    }

    private void OnEnable()
    {
        if (skidMark != null)
            skidMark.enabled = true;
    }
    private void OnDisable()
    {
        if (skidMark != null)
            skidMark.enabled = false;
    }

    // ������� ������ � Update()
    private void Update()
    {
        if (!enabled) return; // �� ��������, ���� ������ �������� (����., � Awake)

        verticalInput = Input.GetAxis("Vertical");
        brakeInput = Input.GetAxis("Jump");
        isBoosting = Input.GetKey(KeyCode.LeftShift); // ������� ����
    }


    // ��������� ����� � FixedUpdate()
    void FixedUpdate()
    {
        if (!enabled) return; // �� ��������, ���� ������ ��������

        if (carController.grounded())
        {
            // �������� ������ �������� � ����������
            float lateralSpeed = Mathf.Abs(carController.carVelocity.x); // ������ ��������
            float forwardSpeed = carController.carVelocity.z; // ����� �������� (���� ���� ��'�����)
            float overallSpeed = carController.rb.linearVelocity.magnitude; // �������� ��������


            // --- �������� ��ò�� �˲Ĳ� ---

            // 1. ���� (��� ���)
            bool isDrifting = lateralSpeed > minDriftSpeed;

            // 2. ����������� (Space) �� �������� (��� ���)
            bool isBraking = brakeInput > 0.1f && overallSpeed > minBrakingSpeed;

            // 3. ����������� ����� ����� (S) - ��������
            // ����� ��������� Ҳ���� ���� �� ����� ������ ������ � ��������� S
            bool isReverseBraking = verticalInput < -0.1f && forwardSpeed > minReverseBrakingSpeed;

            // 4. ����������� "Burnout" (���� �����)
            // ��������� ���� ������� W + Shift, ��� �������� �� ������
            bool isBurnout = verticalInput > 0.1f && isBoosting && overallSpeed < maxBurnoutSpeed;


            // ������� ����, ���� ���������� ��� ���� � ����
            if (isDrifting || isBraking || isReverseBraking || isBurnout)
            {
                fadeOutSpeed = 0f;
                // ������ ��������, �� �������� �������
                if (skidMark.materials.Length > 0)
                    skidMark.materials[0].color = Color.black;

                skidMark.emitting = true;
            }
            else
            {
                skidMark.emitting = false;
            }
        }
        else
        {
            skidMark.emitting = false;
        }

        // ����� ��������� ����
        if (!skidMark.emitting)
        {
            fadeOutSpeed += Time.deltaTime / 2;
            if (skidMark.materials.Length > 0)
            {
                Color m_color = Color.Lerp(Color.black, new Color(0f, 0f, 0f, 0f), fadeOutSpeed);
                skidMark.materials[0].color = m_color;
            }
            if (fadeOutSpeed > 1)
            {
                skidMark.Clear();
            }
        }

        // ����� ���� (� �����������, ��� �� ��������� Play/Stop �������)
        if (skidMark.emitting == true)
        {
            if (!smoke.isPlaying)
                smoke.Play();
        }
        else
        {
            if (smoke.isPlaying)
                smoke.Stop();
        }
    }
}

