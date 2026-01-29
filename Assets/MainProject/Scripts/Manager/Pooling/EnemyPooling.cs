using System.Collections.Generic;
using UnityEngine;

public class EnemyPooling : MonoBehaviour
{
    public static EnemyPooling Instance;

    [System.Serializable]
    public class Pool
    {
        public Enemy prefab;
        public int size;
    }

    public Pool[] pools;

    private readonly Dictionary<Enemy, ObjectPool<Enemy>> poolDict = new();
    private int activeCount;
    public int ActiveCount => activeCount;

    private void Awake()
    {
        Instance = this;

        foreach (var p in pools)
        {
            var pool = new ObjectPool<Enemy>(
                p.prefab,
                p.size,
                transform,
                $"{p.prefab.name}_Pool"
            );

            poolDict[p.prefab] = pool;
        }
    }

    public Enemy Get(Enemy prefab, Vector2 pos)
    {
        var pool = poolDict[prefab];
        Enemy e = pool.Get();

        e.transform.position = pos;
        e.SetPrefabKey(prefab);
        e.ResetState();

        activeCount++;
        return e;
    }

    public void Return(Enemy e)
    {
        if (e == null || e.prefabKey == null) return;

        poolDict[e.prefabKey].Return(e);
        activeCount--;
    }
}
