using System.Collections.Generic;
using UnityEngine;

public enum ProjectileParentType
{
    World,
    Owner
}

public class SkillProjectilePool
{
    private readonly Queue<Projectile> pool = new();
    private readonly Projectile prefab;
    private readonly Transform root;
    private readonly ProjectileParentType parentType;
    private readonly Transform owner;

    public SkillProjectilePool(Projectile prefab, Transform owner, int preload, ProjectileParentType parentType)
    {
        this.prefab = prefab;
        this.owner = owner;
        this.parentType = parentType;

        root = new GameObject($"{prefab.name}_Pool").transform;
        root.SetParent(owner, false);

        for (int i = 0; i < preload; i++)
            Create();
    }

    private Projectile Create()
    {
        Projectile p = GameObject.Instantiate(prefab, root);
        p.gameObject.SetActive(false);
        pool.Enqueue(p);
        return p;
    }

    public Projectile Get()
    {
        if (pool.Count == 0)
            Create();

        Projectile p = pool.Dequeue();

        if (parentType == ProjectileParentType.Owner)
            p.transform.SetParent(owner, false);
        else
            p.transform.SetParent(null, true);

        p.gameObject.SetActive(true);
        return p;
    }

    public void Return(Projectile p)
    {
        p.transform.SetParent(root, false);
        p.gameObject.SetActive(false);
        pool.Enqueue(p);
    }
}
