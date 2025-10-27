using UnityEngine;

public class ManagedPickup : MonoBehaviour
{
    private PickupSpawnManager spawnManager;
    private PickupSpawnManager.SpawnableItem itemType;

    // Метод для передачі даних з менеджера
    public void Initialize(PickupSpawnManager manager, PickupSpawnManager.SpawnableItem type)
    {
        spawnManager = manager;
        itemType = type;
    }

    // Цей метод автоматично викликається Unity, коли об'єкт знищується (наприклад, коли гравець його підбирає)
    private void OnDestroy()
    {
        if (spawnManager != null)
        {
            spawnManager.NotifyItemDestroyed(itemType);
        }
    }
}