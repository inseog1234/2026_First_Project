using UnityEngine;

public class GARLIC : ActiveSkill
{
    private float radius;
    private float tickInterval;
    private float timer;

    private GarlicSkillData gData;
    private bool initialized;
    private GarlicAuraVisual visual;
    private readonly Collider2D[] hitBuffer = new Collider2D[100];
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

        // 이거 고민좀 해봐야 하는데 일단 NonAlloc 씀
        int count = Physics2D.OverlapCircleNonAlloc(owner.transform.position, radius, hitBuffer, enemyMask);

        float damage = GetFinalDamage();
        float knockback = GetFinalKnockback();
        Vector2 origin = owner.transform.position;

        for (int i = 0; i < count; i++)
        {
            Enemy e = hitBuffer[i].GetComponent<Enemy>();
            if (e == null || e.isDead) continue;

            e.TakeDamage(damage);
            AddDamage(damage);

            if (knockback > 0f)
            {
                Vector2 dir = ((Vector2)e.transform.position - origin).normalized;
                e.Knockback(dir, knockback);
            }
        }
    }

    public void Dispose()
    {
        owner.OnSkillUpdate -= UpdateAura;
    }
}
