using UnityEngine;

public class FuelPickup : MonoBehaviour
{
    [SerializeField] private float fuelAmount = 25f;
    [SerializeField] private GameObject pickupEffect; // ≈фект (парт≥кли, звук) при п≥дбор≥

    private void OnTriggerEnter(Collider other)
    {
        // ѕереконуЇмось, що це гравець
        if (other.CompareTag("Player"))
        {
            // ЎукаЇмо контролер на гравц≥
            PlayerResourceController player = other.GetComponentInChildren<PlayerResourceController>();

            if (player != null)
            {
                player.AddFuel(fuelAmount);

                // якщо Ї ефект, створюЇмо його
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // «нищуЇмо п≥кап
                Destroy(gameObject);
            }
        }
    }
}