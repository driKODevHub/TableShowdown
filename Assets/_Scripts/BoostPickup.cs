using UnityEngine;

public class BoostPickup : MonoBehaviour
{
    [SerializeField] private float boostAmount = 30f;
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
                player.AddBoost(boostAmount);

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