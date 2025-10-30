using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int scoreValue = 1;
    [SerializeField] private GameObject pickupEffect;

    // РЕЄСТРАЦІЯ в менеджері при появі на сцені
    private void Start()
    {
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.RegisterCoin(this);
        }
    }

    // ДЕРЕЄСТРАЦІЯ перед знищенням
    private void OnDestroy()
    {
        // Переконуємось, що Instance ще існує (наприклад, при закритті сцени)
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.DeregisterCoin(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Тут ти можеш додати логіку нарахування очок гравцю, наприклад:
            // ScoreManager.Instance.AddScore(scoreValue);

            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // Просто знищуємо монету.
            // Скрипт ManagedPickup.cs автоматично повідомить спавн-менеджер,
            // а наш новий метод OnDestroy() повідомить CoinManager.
            Destroy(gameObject);
        }
    }
}