using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HOLYBOOK : ActiveSkill
{
    private readonly List<Projectile> books = new();
    private float angle;
    private float radius;
    private float rotateSpeed;
    private float lifeTime;
    private float damage;

    private Coroutine lifeRoutine;

    public HOLYBOOK(SkillData data, SkillController owner) : base(data, owner) {}

    protected override void Cast()
    {
        EnsureProjectilePool(10, ProjectileParentType.Owner);

        if (lifeRoutine != null)
            owner.StopCoroutine(lifeRoutine);

        int count = GetFinalProjectileCount() + owner.GlobalStats.projectileBonus;

        radius = GetFinalRange();
        rotateSpeed = GetFinalSpeed() * 50f;
        lifeTime = GetFinalLifetime();
        damage = GetFinalDamage();

        books.Clear();
        angle = 0f;

        SpawnBooks(count);

        lifeRoutine = owner.StartCoroutine(LifeRoutine());

        owner.OnSkillUpdate -= UpdateOrbit;
        owner.OnSkillUpdate += UpdateOrbit;
    }

    private void SpawnBooks(int count)
    {
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float a = step * i;
            Vector2 offset = new Vector2(
                Mathf.Cos(a * Mathf.Deg2Rad),
                Mathf.Sin(a * Mathf.Deg2Rad)
            ) * radius;

            Projectile p = projectilePool.Get();
            p.transform.position = owner.transform.position + (Vector3)offset;

            p.Init(
                Vector2.zero,
                damage,
                0f,
                lifeTime,
                0f,
                this,
                projectilePool,
                0,
                false,
                true
            );

            books.Add(p);
        }
    }

    private void UpdateOrbit(float dt)
    {
        angle += rotateSpeed * dt;

        for (int i = 0; i < books.Count; i++)
        {
            Projectile p = books[i];
            if (p == null) continue;

            float a = angle + 360f / books.Count * i;
            Vector2 pos = new Vector2(
                Mathf.Cos(a * Mathf.Deg2Rad),
                Mathf.Sin(a * Mathf.Deg2Rad)
            ) * radius;

            p.transform.position = owner.transform.position + (Vector3)pos;
        }
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(lifeTime);

        owner.OnSkillUpdate -= UpdateOrbit;

        for (int i = 0; i < books.Count; i++)
        {
            if (books[i] != null)
                projectilePool.Return(books[i]);
        }

        books.Clear();
        lifeRoutine = null;
    }
}
