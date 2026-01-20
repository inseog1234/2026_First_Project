using UnityEngine;

public abstract class PassiveSkill
{
    protected SkillData data;
    public SkillData Data => data;
    
    protected SkillController owner;
    protected int level = 1;
    public int Level => level;


    public PassiveSkill(SkillData data, SkillController owner)
    {
        this.data = data;
        this.owner = owner;
        Apply();
    }

    protected abstract void Apply();

    public virtual void LevelUp()
    {
        level++;
        Apply();
    }
}
