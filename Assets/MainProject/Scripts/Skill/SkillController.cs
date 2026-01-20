using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SkillResultData
{
    public Sprite Icon;
    public string name;
    public int level;
    public float totalDamage;
    public float Time;
}


public class SkillController : MonoBehaviour
{
    [Header("처음 얻는 스킬")]
    public SkillData[] startSkills;

    [Header("기타 설정")]
    public GlobalStats GlobalStats = new GlobalStats();
    public Vector2 offset {get; private set;}

    private List<ActiveSkill> activeSkills = new();
    private List<PassiveSkill> passiveSkills = new();

    [Header("모든 스킬 DB")]
    public SkillData[] allSkillPool;

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

    // 강화 로직
    public void ApplySkill(SkillData selected)
    {
        foreach (var s in activeSkills)
        {
            if (s.Data == selected)
            {
                s.LevelUp();
                return;
            }
        }

        foreach (var s in passiveSkills)
        {
            if (s.Data == selected)
            {
                s.LevelUp();
                return;
            }
        }

        AddSkill(selected);
    }

    // 같은 스킬이 안나오는 랜덤 뽑기
    public List<SkillData> GetRandomSkillList(int count)
    {
        List<SkillData> result = new();
        List<SkillData> candidates = new();

        foreach (var s in allSkillPool)
        {
            if (!HasSkill(s))
                candidates.Add(s);
        }

        foreach (var s in GetOwnedSkillData())
        {
            candidates.Add(s);
        }

        Shuffle(candidates);

        for (int i = 0; i < count && i < candidates.Count; i++)
            result.Add(candidates[i]);

        return result;
    }

    // 같은 아이템도 나올 수 있는 뽑기
    public List<SkillData> GetRandomSkillList_Duplication(int count)
    {
        List<SkillData> result = new();

        List<SkillData> pool = new();

        foreach (var s in allSkillPool)
            pool.Add(s);

        foreach (var s in GetOwnedSkillData())
            pool.Add(s);

        for (int i = 0; i < count; i++)
        {
            SkillData pick = pool[Random.Range(0, pool.Count)];
            result.Add(pick);
        }

        return result;
    }


    public bool HasSkill(SkillData data)
    {
        foreach (var s in activeSkills)
            if (s.Data == data) return true;

        foreach (var s in passiveSkills)
            if (s.Data == data) return true;

        return false;
    }

    private List<SkillData> GetOwnedSkillData()
    {
        List<SkillData> list = new();

        foreach (var s in activeSkills)
            list.Add(s.Data);

        foreach (var s in passiveSkills)
            list.Add(s.Data);

        return list;
    }

    private void Shuffle(List<SkillData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public int GetSkillLevel(SkillData data)
    {
        foreach (var s in activeSkills)
            if (s.Data == data) return s.Level;

        foreach (var s in passiveSkills)
            if (s.Data == data) return s.Level;

        return 0;
    }

    public List<SkillResultData> GetSkillResults()
    {
        List<SkillResultData> list = new();

        foreach (var s in activeSkills)
        {
            list.Add(new SkillResultData
            {
                Icon = s.Data.icon,
                name = s.Data.skillName,
                level = s.Level,
                totalDamage = s.TotalDamage,
                Time = s.Time
            });
        }

        return list;
    }
}
