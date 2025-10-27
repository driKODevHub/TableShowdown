using UnityEngine;

public class FuelPickup : MonoBehaviour
{
    [SerializeField] private float fuelAmount = 25f;
    [SerializeField] private GameObject pickupEffect; // ����� (�������, ����) ��� �����

    private void OnTriggerEnter(Collider other)
    {
        // ������������, �� �� �������
        if (other.CompareTag("Player"))
        {
            // ������ ��������� �� ������
            PlayerResourceController player = other.GetComponentInChildren<PlayerResourceController>();

            if (player != null)
            {
                player.AddFuel(fuelAmount);

                // ���� � �����, ��������� ����
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // ������� ����
                Destroy(gameObject);
            }
        }
    }
}