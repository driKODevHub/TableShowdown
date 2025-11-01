using UnityEngine;
using System.Collections.Generic;

// ������������, �� �� ��'��� � Collider, � �� ���� ��������
[RequireComponent(typeof(Collider))]
public class WindArea : MonoBehaviour
{
    [Header("Wind Settings")]
    [Tooltip("���� ����. 50-100 = ������� �������.")]
    [SerializeField] private float windForce = 50f;

    [Tooltip("�������� ���� � ����� ����� ��'����. (0, 0, 1) - �� '������' �� ���� �� (Z).")]
    [SerializeField] private Vector3 localWindDirection = Vector3.forward;

    [Tooltip("����� ����: Acceleration ������ ���� ������ (�������������), Force - �������.")]
    [SerializeField] private ForceMode forceMode = ForceMode.Acceleration;

    // ������ ��� Rigidbody, �� ����� � ���
    private List<Rigidbody> rigidbodiesInZone = new List<Rigidbody>();
    private Vector3 globalWindDirection;

    private void Awake()
    {
        // ������������, �� ��� collider - �� ������
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("Collider �� 'WindArea' (" + name + ") �� � ��������! ���������� isTrigger = true.", this);
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        // ����������� ���������� �������� ���� ���� ���
        // �� �������� ��� �������� ��'��� ����, ��� ����� ����� �������� ����
        globalWindDirection = transform.TransformDirection(localWindDirection.normalized);
    }

    // ���� ������ �'�����
    private void OnTriggerEnter(Collider other)
    {
        // �� ������ ��� ����� 'rb', �� ����� ArcadeVehicleController ��������� ����.
        // �� ������ �������� ���� ����� 'attachedRigidbody'
        Rigidbody targetRb = other.attachedRigidbody;

        // ����������, �� �� ������ (�� Rigidbody) � �� �� �� �� ������ ��
        if (targetRb != null && !rigidbodiesInZone.Contains(targetRb))
        {
            // �������! �� ����������, �� �� 'rb' � ������ ����������,
            // � �� 'carBody', ���� � ������ ������� ���� ������ ����� Rigidbody.
            // �������� ������ ��������� ��� "Player".
            if (other.CompareTag("Player"))
            {
                rigidbodiesInZone.Add(targetRb);
            }
        }
    }

    // ���� ������ ������
    private void OnTriggerExit(Collider other)
    {
        Rigidbody targetRb = other.attachedRigidbody;

        if (targetRb != null && rigidbodiesInZone.Contains(targetRb))
        {
            rigidbodiesInZone.Remove(targetRb);
        }
    }

    // ����������� ���� � ������� ���� ������
    private void FixedUpdate()
    {
        // ��������� �� ������� Rigidbody � ���
        foreach (Rigidbody rb in rigidbodiesInZone)
        {
            if (rb != null)
            {
                // ������ ������� ���� ����
                rb.AddForce(globalWindDirection * windForce, forceMode);
            }
        }
    }

    // ³��������� �������� ���� � �������� ��� ��������
    private void OnDrawGizmos()
    {
        Vector3 globalDir = transform.TransformDirection(localWindDirection.normalized);
        Gizmos.color = new Color(0.5f, 0.8f, 1.0f, 0.5f); // ��������� ����

        // ������� ������, �� ������ ��������
        Vector3 center = transform.position;
        Vector3 arrowEnd = center + globalDir * 5f; // ������� ������ 5 �����

        Gizmos.DrawLine(center, arrowEnd);
        Gizmos.DrawSphere(arrowEnd, 0.5f);
    }
}