using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Active,
    Passive
}

[System.Serializable]
public class SkillStat
{
    public float baseDamage;
    public float cooldown;
    public float lifetime;
    public float range;
    public float speed;
    public float scale; // 기준 : localScale (Transform) 참고용
    public float knockback;
    public int projectileCount;
    public float projectilefiring_Delay;

    public SkillStat Clone()
    {
        return new SkillStat
        {
            baseDamage = baseDamage,
            cooldown = cooldown,
            lifetime = lifetime,
            range = range,
            speed = speed,
            scale = scale,
            knockback = knockback,
            projectileCount = projectileCount,
            projectilefiring_Delay = projectilefiring_Delay
        };
    }

    public void Add(SkillStat add)
    {
        baseDamage += add.baseDamage;
        cooldown += add.cooldown;
        lifetime += add.lifetime;
        range += add.range;
        speed += add.speed;
        scale += add.scale;
        knockback += add.knockback;
        projectileCount += add.projectileCount;
        projectilefiring_Delay += add.projectilefiring_Delay;
    }
}

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("스킬 기본 구성 데이터")]
    public int skillID;
    public string skillName;
    public SkillType type;
    public Sprite icon;

    [Header("총알을 껴주셍요")]
    public Projectile projectilePrefab;

    [Header("기본 스탯")]
    public SkillStat baseStat;
        
    [Header("레벨업 될 때 증가 하는 스탯")]
    public List<SkillStat> levelUpStats;
    public int maxLevel => levelUpStats.Count;
}
