using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private List<Coin> activeCoins = new List<Coin>();

    private void Awake()
    {
        // Класична реалізація одинака (singleton)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterCoin(Coin coin)
    {
        if (!activeCoins.Contains(coin))
        {
            activeCoins.Add(coin);
        }
    }

    public void DeregisterCoin(Coin coin)
    {
        if (activeCoins.Contains(coin))
        {
            activeCoins.Remove(coin);
        }
    }

    public Transform GetClosestCoin(Vector3 playerPosition)
    {
        if (activeCoins.Count == 0)
        {
            return null; // Немає активних монет
        }

        Transform closestCoin = null;
        float minDistance = float.MaxValue;

        foreach (var coin in activeCoins)
        {
            float distance = Vector3.Distance(playerPosition, coin.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCoin = coin.transform;
            }
        }

        return closestCoin;
    }
}