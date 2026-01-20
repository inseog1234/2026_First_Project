using UnityEngine;

public class BunGaeHwaSal : ActiveSkill
{
    public BunGaeHwaSal(SkillData data, SkillController owner) : base(data, owner) {}

    protected override void Cast()
    {
        Enemy target = FindNearestEnemy();
        if (target == null) return;

        Vector2 dir = ((Vector2)target.transform.position + target.Get_Offset() - (Vector2)owner.transform.position).normalized;

        SpawnProjectile(dir);
    }

    private void SpawnProjectile(Vector2 dir)
    {
        Projectile p = ProjectilePooling.Instance.Get(data.projectilePrefab);

        p.transform.position = owner.transform.position + (Vector3)owner.offset;
        p.Init(
            dir,
            GetFinalDamage(),
            GetFinalSpeed(),
            GetFinalLifetime(),
            GetFinalKnockback()
        );
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
