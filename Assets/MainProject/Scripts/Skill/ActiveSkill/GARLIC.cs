using UnityEngine;

public class GARLIC : ActiveSkill
{
    private float radius;
    private float tickInterval;
    private float timer;

    private GarlicSkillData gData;

    public GARLIC(SkillData data, SkillController owner) : base(data, owner)
    {
        gData = (GarlicSkillData)data;
    }

    protected override void Cast()
    {
        radius = GetFinalRange();
        tickInterval = gData.tickInterval;
        timer = 0f;

        GameObject go = new GameObject("GarlicAura");
        go.transform.SetParent(owner.transform);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = gData.auraSprite;
        sr.color = gData.auraColor;
        sr.sortingOrder = -1;

        var visual = go.AddComponent<GarlicAuraVisual>();
        visual.Init(owner, GetFinalRange());

        owner.OnSkillUpdate += UpdateAura;
    }

    private void UpdateAura(float dt)
    {
        timer -= dt;
        if (timer > 0) return;

        timer = tickInterval;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            owner.transform.position,
            radius,
            LayerMask.GetMask("Enemy")
        );

        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e == null) continue;

            e.TakeDamage(GetFinalDamage());

            Vector2 dir = ((Vector2)e.transform.position - (Vector2)owner.transform.position).normalized;
            e.Knockback(dir, data.baseStat.knockback);
        }
    }
}
