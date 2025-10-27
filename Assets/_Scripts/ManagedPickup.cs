using UnityEngine;

public class ManagedPickup : MonoBehaviour
{
    private PickupSpawnManager spawnManager;
    private PickupSpawnManager.SpawnableItem itemType;

    // ����� ��� �������� ����� � ���������
    public void Initialize(PickupSpawnManager manager, PickupSpawnManager.SpawnableItem type)
    {
        spawnManager = manager;
        itemType = type;
    }

    // ��� ����� ����������� ����������� Unity, ���� ��'��� ��������� (���������, ���� ������� ���� ������)
    private void OnDestroy()
    {
        if (spawnManager != null)
        {
            spawnManager.NotifyItemDestroyed(itemType);
        }
    }
}