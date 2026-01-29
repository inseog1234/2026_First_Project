using UnityEngine;

[System.Serializable]
public class Pool
{
    public UnityEngine.GameObject prefab;
    public int size;
}

public abstract class ActiveSkill
{
    protected SkillData data;
    public SkillData Data => data;
    protected SkillController owner;

    protected SkillProjectilePool projectilePool;

    protected float cooldownTimer;
    protected int level = 1;
    public int Level => level;

    protected float totalDamage;
    public float TotalDamage => totalDamage;

    protected float time;
    public float Time => time;
    
    protected SkillStat currentStat;

    protected ActiveSkill(SkillData data, SkillController owner)
    {
        this.data = data;
        this.owner = owner;

        currentStat = data.baseStat.Clone();
        cooldownTimer = 0f;
    }

    protected void EnsureProjectilePool(int poolCount, ProjectileParentType parentType)
    {
        if (projectilePool != null) return;

        if (data.projectilePrefab == null)
        {
            Debug.LogError(
                $"[{GetType().Name}] projectilePrefab is NULL in SkillData ({data.skillName})"
            );
            return;
        }

        projectilePool = new SkillProjectilePool(data.projectilePrefab, owner.transform, poolCount, parentType);
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
    
    protected float GetFinalCooldown() => currentStat.cooldown * owner.GlobalStats.cooldownMultiplier;
    protected float GetFinalDamage() => currentStat.baseDamage * owner.GlobalStats.damageMultiplier;
    protected float GetFinalRange() => currentStat.range;
    protected float GetFinalSpeed() => currentStat.speed;
    protected float GetFinalLifetime() => currentStat.lifetime;
    protected float GetFinalKnockback() => currentStat.knockback;
    protected int GetFinalProjectileCount() => currentStat.projectileCount;
    protected float GetFinalProjectileDelay() => currentStat.projectilefiring_Delay;

    public void AddDamage(float dmg)
    {
        totalDamage += dmg;
    }

    public void LevelUp()
    {
        if (level >= data.maxLevel) return;
        
        currentStat.Add(data.levelUpStats[level - 1]);
        level++;
    }
}
