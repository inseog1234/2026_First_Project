using System.Collections.Generic;
using UnityEngine;

public class ExpOrbPooling : MonoBehaviour
{
    public static ExpOrbPooling Instance;

    [System.Serializable]
    public class OrbPool
    {
        public ExpOrb prefab;
        public int size;
        public int expValue;
    }

    public OrbPool[] pools;

    private readonly Dictionary<int, ObjectPool<ExpOrb>> poolDict = new();

    private void Awake()
    {
        Instance = this;

        foreach (var p in pools)
        {
            if (p.prefab == null || p.size <= 0) continue;

            int key = p.expValue;

            var pool = new ObjectPool<ExpOrb>(
                p.prefab,
                p.size,
                transform,
                poolName: $"{p.prefab.name}_{key}_Pool",
                onGet: (orb) => { orb.expValue = key; }
            );

            poolDict[key] = pool;
        }
    }

    public ExpOrb Get(Vector2 pos, int value)
    {
        int key = GetOrbValue(value);

        if (!poolDict.TryGetValue(key, out var pool)) return null;

        ExpOrb e = pool.Get();
        e.transform.position = pos;
        e.expValue = key;

        return e;
    }

    public void Return(ExpOrb e)
    {
        if (e == null) return;

        int key = GetOrbValue((int)e.expValue);
        if (poolDict.TryGetValue(key, out var pool))
        {
            pool.Return(e);
            return;
        }

        e.gameObject.SetActive(false);
    }

    private int GetOrbValue(int value)
    {
        if (value >= 1000) return 1000;
        if (value >= 100) return 100;
        if (value >= 10) return 10;
        return 1;
    }

    public void DropExp(Vector2 pos, float _offset, int value)
    {
        while (value > 0)
        {
            int v = GetOrbValue(value);
            Vector2 offset = Random.insideUnitCircle * _offset;
            Get(pos + offset, v);
            value -= v;
        }
    }
}
