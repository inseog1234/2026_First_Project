using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private readonly Dictionary<Component, object> pools = new();

    [SerializeField] private EnemyPooling enemyPooling;
    [SerializeField] private ExpOrbPooling expOrbPooling;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 게터
    public ObjectPool<T> GetPool<T>(T prefab, int preload) where T : Component
    {
        if (pools.TryGetValue(prefab, out object pool))
            return (ObjectPool<T>)pool;

        var newPool = new ObjectPool<T>(prefab, preload, transform);
        pools.Add(prefab, newPool);
        return newPool;
    }

    public EnemyPooling Enemy => enemyPooling;
    public ExpOrbPooling ExpOrb => expOrbPooling;
}
