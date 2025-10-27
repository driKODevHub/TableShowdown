using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int scoreValue = 1; // Можеш додати вартість монетки, якщо знадобиться
    [SerializeField] private GameObject pickupEffect; // Ефект (партікли, звук) при підборі

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Тут ти можеш додати логіку нарахування очок гравцю, наприклад:
            // ScoreManager.Instance.AddScore(scoreValue);

            // Якщо є ефект, створюємо його
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // Просто знищуємо монету. 
            // Скрипт ManagedPickup.cs автоматично повідомить менеджер.
            Destroy(gameObject);
        }
    }
}
