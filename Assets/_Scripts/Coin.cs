using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int scoreValue = 1; // ����� ������ ������� �������, ���� ����������� 
    [SerializeField] private GameObject pickupEffect; // ����� (�������, ����) ��� �����

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ��� �� ����� ������ ����� ����������� ���� ������, ���������:
            // ScoreManager.Instance.AddScore(scoreValue);

            // ���� � �����, ��������� ����
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // ������ ������� ������. 
            // ������ ManagedPickup.cs ����������� ��������� ��������.
            Destroy(gameObject);
        }
    }
}
