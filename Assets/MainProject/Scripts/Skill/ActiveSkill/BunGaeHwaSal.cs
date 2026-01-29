using UnityEngine;
using System.Collections;

public class BunGaeHwaSal : ActiveSkill
{
    public BunGaeHwaSal(SkillData data, SkillController owner) : base(data, owner) {}

    protected override void Cast()
    {
        EnsureProjectilePool(80, ProjectileParentType.World);
        if (projectilePool == null) return;

        Enemy target = FindNearestEnemy();
        if (target == null) return;

        Vector2 dir = (
            (Vector2)target.transform.position + target.Get_Offset()
            - (Vector2)owner.transform.position
        ).normalized;

        owner.StartCoroutine(FireBurst(dir));
    }

    private IEnumerator FireBurst(Vector2 dir)
    {
        int count = GetFinalProjectileCount() + owner.GlobalStats.projectileBonus;
        float delay = GetFinalProjectileDelay();

        for (int i = 0; i < count; i++)
        {
            Projectile p = projectilePool.Get();
            p.transform.position = owner.transform.position + (Vector3)owner.offset;

            p.Init(
                dir,
                GetFinalDamage(),
                GetFinalSpeed(),
                GetFinalLifetime(),
                GetFinalKnockback(),
                this,
                projectilePool
            );

            if (i < count - 1 && delay > 0f)
                yield return new WaitForSeconds(delay);
        }
    }

    private Enemy FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            owner.transform.position,
            GetFinalRange(),
            LayerMask.GetMask("Enemy")
        );

        float minDist = float.MaxValue;
        Enemy nearest = null;

        foreach (var hit in hits)
        {
            float d = Vector2.Distance(owner.transform.position, hit.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = hit.GetComponent<Enemy>();
            }
        }

        return nearest;
    }
}
