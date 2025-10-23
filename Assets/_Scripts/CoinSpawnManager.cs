using UnityEngine;

public class CoinSpawnManager : MonoBehaviour
{
    public static CoinSpawnManager Instance { get; private set; }

    [SerializeField] GameObject coinPrefab;
    [SerializeField] Vector3 spawnBoundBoxSize;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
    }

    private void Start()
    {
        RandomizeCoinPosition();
    }

    public void RandomizeCoinPosition()
    {
        Vector3 newPosition = new Vector3(
            Random.Range(transform.position.x - spawnBoundBoxSize.x / 2, transform.position.x + spawnBoundBoxSize.x / 2),
            Random.Range(transform.position.y - spawnBoundBoxSize.y / 2, transform.position.y + spawnBoundBoxSize.y / 2),
            Random.Range(transform.position.z - spawnBoundBoxSize.z / 2, transform.position.z + spawnBoundBoxSize.z / 2));

        newPosition.y = 0;

        coinPrefab.transform.position = newPosition;
       
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, spawnBoundBoxSize);
    }
}
