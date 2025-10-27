using UnityEngine;
using System.Collections.Generic; // ������� ��� ������ � ��������

public class PickupSpawnManager : MonoBehaviour
{
    public static PickupSpawnManager Instance { get; private set; }

    // ��������� ���� ��� �������� ������������ ������� ���� �������� � ���������
    [System.Serializable]
    public class SpawnableItem
    {
        public string name; // ������ ��� �������� � ���������
        public GameObject prefab;
        [Range(1, 50)]
        public int maxConcurrentCount = 5; // ����������� ������� �� ���� ���������
        [Range(1f, 100f)]
        public float minRespawnDistance = 10f; // ̳������� ��������� �� ���������� �����

        // ������� ���� ��� ���������� �����
        [HideInInspector] public int currentCount = 0;
        [HideInInspector] public Vector3 lastSpawnPosition = Vector3.zero;
    }

    [Header("General Settings")]
    [SerializeField] private Transform playerTransform; // <--- ���� ����: ��������� ���� ������
    [SerializeField] private Vector3 spawnBoundBoxSize = new Vector3(100, 1, 100);
    [SerializeField] private float spawnCheckInterval = 1.0f; // �� ����� ���������, �� ������� ���� ����������

    [Header("Items to Spawn")]
    [SerializeField] private List<SpawnableItem> itemsToSpawn;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // �������� ��������, ��� �� �� ����� ������ ������
        if (playerTransform == null)
        {
            Debug.LogError("PickupSpawnManager: Player Transform �� ���������� � ���������! �������� ����� �� ������.");
        }
    }

    private void Start()
    {
        // ������ �������� ��������� ������� ��������
        InitializeSpawns();
        // ��������� ������� �������� ��� ���������� ��������
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
        // ������ ����� ����� (���������, 20) ������ �������� �������
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-spawnBoundBoxSize.x / 2, spawnBoundBoxSize.x / 2),
                0, // �������� �� ��� ����
                Random.Range(-spawnBoundBoxSize.z / 2, spawnBoundBoxSize.z / 2)
            );

            // ���������� �������� ������� ������� ��������� � ������
            spawnPosition = transform.position + randomPoint;

            // --- �������� ��ò�� ����²��� ---

            // �������� 1: ��������� �� ������ (������)
            bool tooCloseToPlayer = false;
            if (playerTransform != null)
            {
                // ����������, �� �� ������� ������� �� ������
                tooCloseToPlayer = Vector3.Distance(spawnPosition, playerTransform.position) < item.minRespawnDistance;
            }

            // �������� 2: ��������� �� �������� ����� ������ (���� �� �� ������ �����)
            bool tooCloseToLastSpawn = false;
            if (item.currentCount > 0) // ����������, ���� ���� �� �� ������ ��'���
            {
                tooCloseToLastSpawn = Vector3.Distance(spawnPosition, item.lastSpawnPosition) < item.minRespawnDistance;
            }

            // ���� �� ������� ������� �� ������ � �� ������� ������� �� ������������
            if (!tooCloseToPlayer && !tooCloseToLastSpawn)
            {
                SpawnItem(item, spawnPosition);
                return; // ����! �������� � �����
            }
            // --- ʲ���� �������ί ��ò�� ---
        }
        // ���� �� 20 ����� �� ������� ������ ����, �� ������ ���������� ��� ���� ������
    }

    private void SpawnItem(SpawnableItem item, Vector3 position)
    {
        GameObject newPickup = Instantiate(item.prefab, position, Quaternion.identity);
        // ������ ���������, ���� ���� ���������� ��� ��� �������� ��'����
        var managedPickup = newPickup.AddComponent<ManagedPickup>();
        managedPickup.Initialize(this, item);

        item.currentCount++;
        item.lastSpawnPosition = position;
    }

    // ��� ����� ��������������� � ManagedPickup, ���� ��'��� ���� �������
    public void NotifyItemDestroyed(SpawnableItem item)
    {
        if (item != null)
        {
            item.currentCount--;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f); // ������������ �������
        Gizmos.DrawWireCube(transform.position, spawnBoundBoxSize);
    }
}
