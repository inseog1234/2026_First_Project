using UnityEngine;

[CreateAssetMenu(fileName = "SkillLibrary", menuName = "Scriptable Objects/Skill Library")]
public class SkillLibrary : ScriptableObject
{
    public SkillData[] skills;
}
