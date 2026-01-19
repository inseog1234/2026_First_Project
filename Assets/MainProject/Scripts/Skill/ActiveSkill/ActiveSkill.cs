using UnityEngine;

public abstract class ActiveSkill
{
    protected SkillData data;
    protected SkillController owner;

    protected float cooldownTimer;
    protected int level = 1;

    public ActiveSkill(SkillData data, SkillController owner)
    {
        this.data = data;
        this.owner = owner;
        cooldownTimer = data.baseStat.cooldown;
    }

    public void Tick(float dt)
    {
        cooldownTimer -= dt;
        if (cooldownTimer <= 0f)
        {
            Cast();
            cooldownTimer = GetFinalCooldown();
        }
    }

    protected abstract void Cast();

    protected float GetFinalCooldown()
    {
        return data.baseStat.cooldown * owner.GlobalStats.cooldownMultiplier;
    }

    protected float GetFinalDamage()
    {
        return data.baseStat.baseDamage * owner.GlobalStats.damageMultiplier;
    }

    public void LevelUp()
    {
        if (level >= data.maxLevel) return;
        level++;
    }
}
