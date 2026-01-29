using System.Collections.Generic;
using UnityEngine;

public class EnemyPooling : MonoBehaviour
{
    public static EnemyPooling Instance;
    private int activeCount;
    public int ActiveCount => activeCount;

    [System.Serializable]
    public class Pool
    {
        public Enemy prefab;
        public int size;
    }

    [Header("Pools")]
    public Pool[] pools;

    private Dictionary<Enemy, Queue<Enemy>> poolDict = new();

    private void Awake()
    {
        Instance = this;

        foreach (var p in pools)
        {
            Queue<Enemy> q = new();

            for (int i = 0; i < p.size; i++)
            {
                Enemy e = Instantiate(p.prefab, transform);
                e.gameObject.SetActive(false);
                q.Enqueue(e);
            }

            poolDict.Add(p.prefab, q);
        }
    }

    public Enemy Get(Enemy prefab, Vector2 pos)
    {
        if (!poolDict.ContainsKey(prefab))
            return null;

        Enemy e = poolDict[prefab].Dequeue();
        if (e == null)
        {
            return null;
        }

        poolDict[prefab].Enqueue(e);

        e.transform.position = pos;
        e.gameObject.SetActive(true);

        e.ResetState();
        activeCount++;

        return e;
    }

    public void Return(Enemy e)
    {
        e.gameObject.SetActive(false);
        activeCount--;
    }
}
