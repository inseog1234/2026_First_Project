using UnityEngine;
public static class SkillSpawner
{
    public static ActiveSkill CreateActive(SkillData data, SkillController owner)
    {
        return data.skillID switch
        {
            0 => new BunGaeHwaSal(data, owner),
            1 => new HOLYBOOK(data, owner),
            _ => null
        };
    }

    public static PassiveSkill CreatePassive(SkillData data, SkillController owner)
    {
        return data.skillID switch
        {
            // 0 => new 패시브 스탯 클래스(data, owner),
            _ => null
        };
    }
}
