using System.Collections.Generic;
using UnityEngine;

public class ProjectilePooling : MonoBehaviour
{
    public static ProjectilePooling Instance;

    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;
        public int size;
    }

    public Pool[] pools;

    private Dictionary<GameObject, Queue<Projectile>> poolDict = new();

    private void Awake()
    {
        Instance = this;

        foreach (var p in pools)
        {
            Queue<Projectile> q = new();
            for (int i = 0; i < p.size; i++)
            {
                GameObject go = Instantiate(p.prefab, transform);
                go.SetActive(false);
                q.Enqueue(go.GetComponent<Projectile>());
            }
            poolDict.Add(p.prefab, q);
        }
    }

    public Projectile Get(GameObject prefab)
    {
        if (!poolDict.ContainsKey(prefab))
            return null;

        Projectile proj = poolDict[prefab].Dequeue();
        poolDict[prefab].Enqueue(proj);
        return proj;
    }

    public void Return(Projectile proj)
    {
        proj.gameObject.SetActive(false);
    }
}
