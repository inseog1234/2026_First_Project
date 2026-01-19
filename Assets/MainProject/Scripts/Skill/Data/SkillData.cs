using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Active,
    Passive
}

[System.Serializable]
public struct SkillStat
{
    public float baseDamage;
    public float cooldown;
    public float lifetime;
    public float range;
    public float speed;
    public float scale; // 기준 : localScale (Transform) 참고용
    public float knockback;
}

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("스킬 기본 구성 데이터")]
    public int skillID;
    public string skillName;
    public SkillType type;
    public Sprite icon;

    [Header("공격 수단(?)")]
    public GameObject projectilePrefab;
    public int projectileCount;
    

    [Header("기본 스탯")]
    public SkillStat baseStat;
        
    [Header("레벨업 될 때 증가 하는 스탯")]
    public List<SkillStat> levelUpStats;
    public int maxLevel => levelUpStats.Count;
    // 따로 추가 구성 해야함
}
