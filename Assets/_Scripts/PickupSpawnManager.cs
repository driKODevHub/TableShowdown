using UnityEngine;
using System.Collections.Generic; // Потрібно для роботи зі списками

public class PickupSpawnManager : MonoBehaviour
{
    public static PickupSpawnManager Instance { get; private set; }

    // Вкладений клас для зручного налаштування кожного типу предметів в інспекторі
    [System.Serializable]
    public class SpawnableItem
    {
        public string name; // Просто для зручності в інспекторі
        public GameObject prefab;
        [Range(1, 50)]
        public int maxConcurrentCount = 5; // Максимальна кількість на сцені одночасно
        [Range(1f, 100f)]
        public float minRespawnDistance = 10f; // Мінімальна дистанція від попередньої точки

        // Внутрішні змінні для відстеження стану
        [HideInInspector] public int currentCount = 0;
        [HideInInspector] public Vector3 lastSpawnPosition = Vector3.zero;
    }

    [Header("General Settings")]
    [SerializeField] private Transform playerTransform; // <--- НОВЕ ПОЛЕ: Перетягни сюди гравця
    [SerializeField] private Vector3 spawnBoundBoxSize = new Vector3(100, 1, 100);
    [SerializeField] private float spawnCheckInterval = 1.0f; // Як часто перевіряти, чи потрібно щось заспавнити

    [Header("Items to Spawn")]
    [SerializeField] private List<SpawnableItem> itemsToSpawn;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Маленька перевірка, щоб ти не забув додати гравця
        if (playerTransform == null)
        {
            Debug.LogError("PickupSpawnManager: Player Transform не призначено в інспекторі! Можливий спавн на гравці.");
        }
    }

    private void Start()
    {
        // Одразу спавнимо початкову кількість предметів
        InitializeSpawns();
        // Запускаємо постійну перевірку для поповнення предметів
        InvokeRepeating(nameof(CheckAndSpawnItems), spawnCheckInterval, spawnCheckInterval);
    }

    private void InitializeSpawns()
    {
        foreach (var item in itemsToSpawn)
        {
            for (int i = 0; i < item.maxConcurrentCount; i++)
            {
                TrySpawnItem(item);
            }
        }
    }

    private void CheckAndSpawnItems()
    {
        foreach (var item in itemsToSpawn)
        {
            if (item.currentCount < item.maxConcurrentCount)
            {
                TrySpawnItem(item);
            }
        }
    }

    private void TrySpawnItem(SpawnableItem item)
    {
        Vector3 spawnPosition;
        // Робимо кілька спроб (наприклад, 20) знайти підходящу позицію
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-spawnBoundBoxSize.x / 2, spawnBoundBoxSize.x / 2),
                0, // Спавнимо на рівні землі
                Random.Range(-spawnBoundBoxSize.z / 2, spawnBoundBoxSize.z / 2)
            );

            // Конвертуємо локальну позицію відносно менеджера у світову
            spawnPosition = transform.position + randomPoint;

            // --- ОНОВЛЕНА ЛОГІКА ПЕРЕВІРКИ ---

            // Перевірка 1: Дистанція до гравця (завжди)
            bool tooCloseToPlayer = false;
            if (playerTransform != null)
            {
                // Перевіряємо, чи не занадто близько до гравця
                tooCloseToPlayer = Vector3.Distance(spawnPosition, playerTransform.position) < item.minRespawnDistance;
            }

            // Перевірка 2: Дистанція до останньої точки спавну (якщо це не перший спавн)
            bool tooCloseToLastSpawn = false;
            if (item.currentCount > 0) // Перевіряємо, лише якщо це НЕ перший об'єкт
            {
                tooCloseToLastSpawn = Vector3.Distance(spawnPosition, item.lastSpawnPosition) < item.minRespawnDistance;
            }

            // Якщо не занадто близько до гравця І не занадто близько до попереднього
            if (!tooCloseToPlayer && !tooCloseToLastSpawn)
            {
                SpawnItem(item, spawnPosition);
                return; // Успіх! Виходимо з циклу
            }
            // --- КІНЕЦЬ ОНОВЛЕНОЇ ЛОГІКИ ---
        }
        // Якщо за 20 спроб не вдалося знайти місце, ми просто пропустимо цей цикл спавну
    }

    private void SpawnItem(SpawnableItem item, Vector3 position)
    {
        GameObject newPickup = Instantiate(item.prefab, position, Quaternion.identity);
        // Додаємо компонент, який буде повідомляти нас про знищення об'єкта
        var managedPickup = newPickup.AddComponent<ManagedPickup>();
        managedPickup.Initialize(this, item);

        item.currentCount++;
        item.lastSpawnPosition = position;
    }

    // Цей метод викликатиметься з ManagedPickup, коли об'єкт буде знищено
    public void NotifyItemDestroyed(SpawnableItem item)
    {
        if (item != null)
        {
            item.currentCount--;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f); // Напівпрозорий зелений
        Gizmos.DrawWireCube(transform.position, spawnBoundBoxSize);
    }
}
