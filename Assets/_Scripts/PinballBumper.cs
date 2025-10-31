using UnityEngine;

// ������������, �� �� ��'��� � Collider (�� ������!),
// ��� ������ ����� ����������� ��������.
[RequireComponent(typeof(Collider))]
public class PinballBumper : MonoBehaviour
{
    [Header("Bumper Settings")]
    [Tooltip("���� �������������. 50-100 = ������� �������, 500+ = ������ � ������.")]
    [SerializeField] private float bounceForce = 150f;

    [Tooltip("�� �������� ���� ����� '�����', ��� ����� �������� ������.")]
    [SerializeField] private float upwardForceMultiplier = 0.1f;

    [Header("Effects (Optional)")]
    [Tooltip("����� �������, �� ���������� � ����� �����")]
    [SerializeField] private GameObject impactEffect;
    [Tooltip("����, ���� ����������� ��� ����")]
    [SerializeField] private AudioClip impactSound;

    // �� ������ �������� AudioSource, ���� �� �������
    private AudioSource audioSource;

    private void Awake()
    {
        // ���� �� ������ ����, �� ����������� ������ ��������� ��� ���� ����������
        if (impactSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = impactSound;
            audioSource.spatialBlend = 1.0f; // ������� ���� 3D
        }
    }

    /// <summary>
    /// ��� ����� ����������� �����������, ���� ����� Rigidbody (���� �������)
    /// ���������� � Collider ����� ��'����.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // 1. ����������, �� � � ��'����, �� ��������, Rigidbody.
        // � ������ �������, collision.rigidbody - �� � ���� ��� ����� `rb`
        // � ������� ArcadeVehicleController.
        if (collision.rigidbody == null)
        {
            return; // �� �� �������� ��'��� (���������, ����), ��������.
        }

        // 2. �������� �������� �������������.
        // collision.contacts[0].normal - �� ������, �� "��������" ²� �������
        // ������� �������� ����, ��� ������. �� ���� ��� ��������, �� ��� �������!
        Vector3 bounceDirection = collision.contacts[0].normal;

        // 3. (�����������) ������ ����� ���� �����
        // �� ����� ����� "�������������"
        if (upwardForceMultiplier > 0)
        {
            bounceDirection = (bounceDirection + Vector3.up * upwardForceMultiplier).normalized;
        }

        // 4. ����������� ���Ҫ�� ���� (�������) �� Rigidbody �������.
        // ForceMode.Impulse - �� ���� ��, �� ������� ��� "����������" ������.
        collision.rigidbody.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);

        // 5. ³��������� ������ � ����� ��������
        PlayEffects(collision.contacts[0].point, bounceDirection);
    }

    private void PlayEffects(Vector3 impactPoint, Vector3 impactNormal)
    {
        // ����� ������� � ����� ��������
        if (impactEffect != null)
        {
            // ��������� ����� � ��������� ���� ���, ��� �� "�������" �� �������
            Instantiate(impactEffect, impactPoint, Quaternion.LookRotation(impactNormal));
        }

        // ����
        if (audioSource != null && impactSound != null)
        {
            audioSource.Play();
        }
    }
}