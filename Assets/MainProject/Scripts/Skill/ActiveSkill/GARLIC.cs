using UnityEngine;

public class GARLIC : ActiveSkill
{
    private float radius;
    private float tickInterval;
    private float timer;

    private GarlicSkillData gData;

    // 이걸로 초기화 했는지 확인할거잉ㅇ
    private bool initialized;
    private GarlicAuraVisual visual;

    public GARLIC(SkillData data, SkillController owner) : base(data, owner)
    {
        gData = (GarlicSkillData)data;
    }

    protected override void Cast()
    {
        // 초기화 한번만 할래요
        if (initialized)
        {
            radius = GetFinalRange();
            if (visual != null) visual.SetRadius(radius);
            return;
        }

        initialized = true;

        radius = GetFinalRange();
        tickInterval = gData.tickInterval;
        timer = 0f;

        // 시각 오브젝트 생성 1회
        GameObject go = new GameObject("GarlicAura");
        go.transform.SetParent(owner.transform);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = gData.auraSprite;
        sr.color = gData.auraColor;
        sr.sortingOrder = -1;

        visual = go.AddComponent<GarlicAuraVisual>();
        visual.Init(owner, radius);

        // 스킬 중복 추가 금지
        owner.OnSkillUpdate -= UpdateAura;
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
            AddDamage(GetFinalDamage());

            Vector2 dir = ((Vector2)e.transform.position - (Vector2)owner.transform.position).normalized;
            e.Knockback(dir, data.baseStat.knockback);
        }
    }
}
