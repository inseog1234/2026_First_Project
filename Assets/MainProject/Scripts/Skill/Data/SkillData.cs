using UnityEngine;

public enum SkillType
{
    Active,
    Passive
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
    

    [Header("스킬 스탯")]
    public float baseDamage;
    public float cooldown;
    public float lifetime;
    public float range;
    public float speed;
    public float scale; // 기준 : localScale (Transform) 참고용
    public float knockback;
    
    [Header("레벨 데이터")]
    public int maxLevel;
    // 따로 추가 구성 해야함
}
