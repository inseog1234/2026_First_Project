using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    [Header("Start Skills")]
    public SkillData[] startSkills;
    public GlobalStats GlobalStats = new GlobalStats();
    
    public Vector2 offset {get; private set;}
    private List<ActiveSkill> activeSkills = new();
    private List<PassiveSkill> passiveSkills = new();

    private void Start()
    {
        for (int i = 0; i < startSkills.Length; i++)
        {
            AddSkill(startSkills[i]);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < activeSkills.Count; i++)
            activeSkills[i].Tick(dt);
    }

    public void AddSkill(SkillData data)
    {
        if (data.type == SkillType.Active)
        {
            ActiveSkill skill = SkillSpawner.CreateActive(data, this);
            activeSkills.Add(skill);
        }
        else
        {
            PassiveSkill skill = SkillSpawner.CreatePassive(data, this);
            passiveSkills.Add(skill);
        }
    }

    public void Set_Offset(Vector2 offset)
    {
        this.offset = offset;
    }
}
