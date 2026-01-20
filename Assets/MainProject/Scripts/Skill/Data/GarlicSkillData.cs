using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Garlic")]
public class GarlicSkillData : SkillData
{
    [Header("마늘 범위 이미지")]
    public Sprite auraSprite;
    public Color auraColor = new Color(1, 1, 1, 0.25f);

    [Header("공격 빈도")]
    public float tickInterval;
}
