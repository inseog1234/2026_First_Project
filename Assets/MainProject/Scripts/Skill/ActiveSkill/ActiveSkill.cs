public abstract class ActiveSkill
{
    protected SkillData data;
    public SkillData Data => data;
    protected SkillController owner;

    protected float cooldownTimer;
    protected int level = 1;
    public int Level => level;

    protected float totalDamage;
    public float TotalDamage => totalDamage;

    protected float time;
    public float Time => time;


    protected SkillStat currentStat;

    public ActiveSkill(SkillData data, SkillController owner)
    {
        this.data = data;
        this.owner = owner;
       
        currentStat = data.baseStat;
        cooldownTimer = 0f;
    }

    public void Tick(float dt)
    {
        time += dt;
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

    protected int GetFinalProjectileCount()
    {
        return currentStat.projectileCount;
    }
    protected float GetFinalProjectileDelay()
    {
        return currentStat.projectilefiring_Delay;
    }

    public void AddDamage(float dmg)
    {
        totalDamage += dmg;
    }

    public void LevelUp()
    {
        if (level >= data.maxLevel) return;

        SkillStat add = data.levelUpStats[level-1];

        currentStat.baseDamage += add.baseDamage;
        currentStat.cooldown += add.cooldown;
        currentStat.lifetime += add.lifetime;
        currentStat.range += add.range;
        currentStat.speed += add.speed;
        currentStat.scale += add.scale;
        currentStat.knockback += add.knockback;
        currentStat.projectileCount += add.projectileCount;
        currentStat.projectilefiring_Delay += add.projectilefiring_Delay;

        level++;
    }
}
