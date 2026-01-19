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

    private Dictionary<int, Queue<ExpOrb>> poolDict = new();

    private void Awake()
    {
        Instance = this;

        foreach (var p in pools)
        {
            Queue<ExpOrb> q = new();

            for (int i = 0; i < p.size; i++)
            {
                ExpOrb e = Instantiate(p.prefab, transform);
                e.expValue = p.expValue;
                e.gameObject.SetActive(false);
                q.Enqueue(e);
            }

            poolDict.Add(p.expValue, q);
        }
    }

    // ==========================
    // 값에 따라 자동 선택
    // ==========================
    public ExpOrb Get(Vector2 pos, int value)
    {
        int key = GetOrbValue(value);

        if (!poolDict.ContainsKey(key))
        {
            Debug.LogError($"ExpOrbPool 없음: {key}");
            return null;
        }

        ExpOrb e = poolDict[key].Dequeue();
        poolDict[key].Enqueue(e);

        e.transform.position = pos;
        e.expValue = key;
        e.gameObject.SetActive(true);

        return e;
    }

    public void Return(ExpOrb e)
    {
        e.gameObject.SetActive(false);
    }

    // ==========================
    // 가장 가까운 오브 타입 선택
    // ==========================
    private int GetOrbValue(int value)
    {
        if (value >= 1000) return 1000;
        if (value >= 100) return 100;
        if (value >= 10) return 10;
        return 1;
    }

    public void DropExp(Vector2 pos, int value)
    {
        while (value > 0)
        {
            int v = GetOrbValue(value);
            Vector2 offset = Random.insideUnitCircle * 0.5f;
            Get(pos + offset, v);
            value -= v;
        }
    }
}
