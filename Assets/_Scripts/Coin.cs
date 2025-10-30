using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int scoreValue = 1;
    [SerializeField] private GameObject pickupEffect;

    // �Ū����ֲ� � �������� ��� ���� �� ����
    private void Start()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.RegisterCoin(this);
        }
    }

    // ���Ū����ֲ� ����� ���������
    private void OnDestroy()
    {
        // ������������, �� Instance �� ���� (���������, ��� ������� �����)
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.DeregisterCoin(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ��� �� ����� ������ ����� ����������� ���� ������, ���������:
            // ScoreManager.Instance.AddScore(scoreValue);

            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // ������ ������� ������.
            // ������ ManagedPickup.cs ����������� ��������� �����-��������,
            // � ��� ����� ����� OnDestroy() ��������� CoinManager.
            Destroy(gameObject);
        }
    }
}