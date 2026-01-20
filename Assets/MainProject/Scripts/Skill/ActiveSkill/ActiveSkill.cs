public abstract class ActiveSkill
{
    protected SkillData data;
    public SkillData Data => data;
    protected SkillController owner;

    protected float cooldownTimer;
    protected int level = 0;

    protected SkillStat currentStat;

    public ActiveSkill(SkillData data, SkillController owner)
    {
        this.data = data;
        this.owner = owner;

        currentStat = data.baseStat;
        cooldownTimer = currentStat.cooldown;
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
        return currentStat.cooldown * owner.GlobalStats.cooldownMultiplier;
    }

    protected float GetFinalDamage()
    {
        return currentStat.baseDamage * owner.GlobalStats.damageMultiplier;
    }

    protected float GetFinalRange()
    {
        return currentStat.range;
    }

    protected float GetFinalSpeed()
    {
        return currentStat.speed;
    }

    protected float GetFinalLifetime()
    {
        return currentStat.lifetime;
    }

    protected float GetFinalKnockback()
    {
        return currentStat.knockback;
    }

    public void LevelUp()
    {
        if (level >= data.maxLevel) return;

        SkillStat add = data.levelUpStats[level];

        currentStat.baseDamage += add.baseDamage;
        currentStat.cooldown += add.cooldown;
        currentStat.lifetime += add.lifetime;
        currentStat.range += add.range;
        currentStat.speed += add.speed;
        currentStat.scale += add.scale;
        currentStat.knockback += add.knockback;

        level++;
    }
}
