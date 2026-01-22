using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HOLYBOOK : ActiveSkill
{
    private List<Projectile> books = new();
    private float angle;
    private float radius;
    private float rotateSpeed;
    private float lifeTime;
    private float damage;

    public HOLYBOOK(SkillData data, SkillController owner) : base(data, owner) {}

    protected override void Cast()
    {
        int count = GetFinalProjectileCount() + owner.GlobalStats.projectileBonus;

        radius = GetFinalRange();
        rotateSpeed = GetFinalSpeed() * 50f;
        lifeTime = GetFinalLifetime();
        damage = GetFinalDamage();

        books.Clear();
        angle = 0f;

        SpawnBooks(count);

        owner.StartCoroutine(LifeRoutine());
        
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

            Projectile p = ProjectilePooling.Instance.Get(data.projectilePrefab);
            p.transform.position = owner.transform.position + (Vector3)offset;
            p.transform.SetParent(owner.transform);

            p.Init(Vector2.zero, damage, 0f, lifeTime, 0f, this, 0, false, true);

            p.transform.rotation = Quaternion.identity;
            books.Add(p);
        }
    }

    private void UpdateOrbit(float dt)
    {
        angle += rotateSpeed * dt;

        for (int i = 0; i < books.Count; i++)
        {
            if (books[i] == null) continue;

            float a = angle + 360f / books.Count * i;
            Vector2 pos = new Vector2(
                Mathf.Cos(a * Mathf.Deg2Rad),
                Mathf.Sin(a * Mathf.Deg2Rad)
            ) * radius;

            books[i].transform.position = owner.transform.position + (Vector3)pos;
            books[i].transform.rotation = Quaternion.identity;
        }
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(lifeTime);

        owner.OnSkillUpdate -= UpdateOrbit;

        for (int i = 0; i < books.Count; i++)
        {
            if (books[i] != null)
                ProjectilePooling.Instance.Return(books[i]);
        }

        books.Clear();
    }
}
