using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Chain Laser")]
public class ChainLaserSkillData : SkillData
{
    [Header("연쇄 레이저 타겟팅")]
    [Min(0.1f)] public float jumpRangeMultiplier = 0.72f;
    [Range(0f, 0.5f)] public float damageFalloffPerJump = 0.12f;
    [Range(0.1f, 1f)] public float minimumDamageMultiplier = 0.55f;

    [Header("연쇄 레이저 라인 연출")]
    public Color glowColor = new Color(0.05f, 0.55f, 1f, 0.42f);
    public Color coreColor = new Color(0.72f, 0.95f, 1f, 1f);
    [Min(0.01f)] public float lineWidth = 0.08f;
    [Min(1f)] public float glowWidthMultiplier = 3.4f;
    [Range(3, 12)] public int jaggedPointCount = 7;
    [Range(0f, 0.5f)] public float jitterAmount = 0.16f;
    [Range(1, 4)] public int impactPulseCount = 2;
}
