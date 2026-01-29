using UnityEngine;

public class GARLIC : ActiveSkill
{
    private float radius;
    private float tickInterval;
    private float timer;

    private GarlicSkillData gData;
    private bool initialized;
    private GarlicAuraVisual visual;
    private int enemyMask;

    public GARLIC(SkillData data, SkillController owner) : base(data, owner)
    {
        gData = (GarlicSkillData)data;
        enemyMask = LayerMask.GetMask("Enemy");
    }

    protected override void Cast()
    {
        if (initialized)
        {
            radius = GetFinalRange();
            if (visual != null)
                visual.SetRadius(radius);
            return;
        }

        initialized = true;

        radius = GetFinalRange();
        tickInterval = gData.tickInterval;
        timer = 0f;

        // 시각 오브젝트 1회 생성 --왜 자꾸 생성되느느가검?????????????????????--
        GameObject go = new GameObject("GarlicAura");
        go.transform.SetParent(owner.transform, false);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = gData.auraSprite;
        sr.color = gData.auraColor;
        sr.sortingOrder = -1;

        visual = go.AddComponent<GarlicAuraVisual>();
        visual.Init(owner, radius);

        owner.OnSkillUpdate += UpdateAura;
    }

    private void UpdateAura(float dt)
    {
        timer -= dt;
        if (timer > 0f) return;

        timer = tickInterval;

        // 결국엔 EnemyManager를 만들어서 관리
        Vector2 center = owner.transform.position;
        float radiusSqr = radius * radius;

        float damage = GetFinalDamage();
        float knockback = GetFinalKnockback();

        foreach (Enemy e in EnemyManager.ActiveEnemies)
        {
            if (e == null || e.isDead) continue;

            Vector2 diff = (Vector2)e.transform.position - center;
            if (diff.sqrMagnitude > radiusSqr) continue;

            e.TakeDamage(damage);
            AddDamage(damage);

            if (knockback > 0f)
                e.Knockback(diff.normalized, knockback);
        }
    }

    public void Dispose()
    {
        owner.OnSkillUpdate -= UpdateAura;
    }
}
