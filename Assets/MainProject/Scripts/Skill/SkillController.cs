using System;
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
    [SerializeField] private SkillData[] startSkills;

    [Header("기타 설정")]
    public GlobalStats GlobalStats = new GlobalStats();
    public Vector2 offset { get; private set; }

    [Header("모든 스킬 DB")]
    [SerializeField] private SkillData[] allSkillPool;

    // 런타임 스킬 인스턴스 보관
    private readonly List<ActiveSkill> activeSkills = new();
    private readonly List<PassiveSkill> passiveSkills = new();

    // Find 반복 제거용
    private readonly Dictionary<SkillData, ActiveSkill> activeMap = new();
    private readonly Dictionary<SkillData, PassiveSkill> passiveMap = new();

    public event Action<float> OnSkillUpdate;

    private void Start()
    {
        // 시작 스킬 1회 등록
        if (startSkills != null)
        {
            for (int i = 0; i < startSkills.Length; i++)
            {
                if (startSkills[i] == null) continue;
                AddSkill(startSkills[i]);
            }
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 스킬 Tick
        for (int i = 0; i < activeSkills.Count; i++)
        {
            activeSkills[i].Tick(dt);
        }

        OnSkillUpdate?.Invoke(dt);
    }

    public void Set_Offset(Vector2 offset)
    {
        this.offset = offset;
    }

    // 스킬 추가
    public void AddSkill(SkillData data)
    {
        if (data == null) return;

        if (data.type == SkillType.Active)
        {
            if (activeMap.ContainsKey(data))
                return;

            ActiveSkill skill = SkillSpawner.CreateActive(data, this);
            if (skill == null)
            {
                Debug.LogError($"[SkillController] CreateActive failed: {data.skillName}");
                return;
            }

            activeSkills.Add(skill);
            activeMap.Add(data, skill);
        }
        else
        {
            if (passiveMap.ContainsKey(data))
                return;

            PassiveSkill skill = SkillSpawner.CreatePassive(data, this);
            if (skill == null)
            {
                Debug.LogError($"[SkillController] CreatePassive failed: {data.skillName}");
                return;
            }

            passiveSkills.Add(skill);
            passiveMap.Add(data, skill);
        }
    }

    // 강화 로직 (동일 인스턴스 유지)
    public void ApplySkill(SkillData data)
    {
        if (data == null) return;

        if (data.type == SkillType.Active)
        {
            if (activeMap.TryGetValue(data, out ActiveSkill a))
            {
                a.LevelUp();
                return;
            }

            AddSkill(data);
            return;
        }

        // Passive
        if (passiveMap.TryGetValue(data, out PassiveSkill p))
        {
            p.LevelUp();
            return;
        }

        AddSkill(data);
    }

    // 소유 여부 / 레벨 조회 O(1)
    public bool HasSkill(SkillData data)
    {
        if (data == null) return false;
        return data.type == SkillType.Active ? activeMap.ContainsKey(data) : passiveMap.ContainsKey(data);
    }

    public int GetSkillLevel(SkillData data)
    {
        if (data == null) return 0;

        if (data.type == SkillType.Active)
            return activeMap.TryGetValue(data, out var a) ? a.Level : 0;

        return passiveMap.TryGetValue(data, out var p) ? p.Level : 0;
    }

    public bool IsSkillMaxLevel(SkillData data)
    {
        if (data == null) return true;
        int cur = GetSkillLevel(data);
        return cur >= data.maxLevel;
    }

    // 랜덤 스킬 뽑기 (중복 X)
    // 매번 Find() 돌리던걸 O(1)로 바꿈
    public List<SkillData> GetRandomSkillList(int count)
    {
        List<SkillData> pool = GetAvailableSkillList();
        List<SkillData> result = new();

        if (pool.Count == 0) return result;

        count = Mathf.Min(count, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        return result;
    }

    private List<SkillData> GetAvailableSkillList()
    {
        List<SkillData> list = new();

        if (allSkillPool == null) return list;

        for (int i = 0; i < allSkillPool.Length; i++)
        {
            SkillData data = allSkillPool[i];
            if (data == null) continue;

            if (data.type == SkillType.Active)
            {
                if (activeMap.TryGetValue(data, out var a))
                {
                    if (a.Level < data.maxLevel) list.Add(data);
                }
                else
                {
                    list.Add(data);
                }
            }
            else
            {
                if (passiveMap.TryGetValue(data, out var p))
                {
                    if (p.Level < data.maxLevel) list.Add(data);
                }
                else
                {
                    list.Add(data);
                }
            }
        }

        return list;
    }

    // 같은 아이템도 나올 수 있는 뽑기 (중복 허용)
    public List<SkillData> GetRandomSkillList_Duplication(int count)
    {
        List<SkillData> result = new();
        if (allSkillPool == null || allSkillPool.Length == 0 || count <= 0) return result;

        // (성능 필요하면 여기서도 캐싱 가능)
        List<SkillData> pool = new(allSkillPool.Length + activeSkills.Count + passiveSkills.Count);

        for (int i = 0; i < allSkillPool.Length; i++)
            if (allSkillPool[i] != null) pool.Add(allSkillPool[i]);

        for (int i = 0; i < activeSkills.Count; i++)
            pool.Add(activeSkills[i].Data);

        for (int i = 0; i < passiveSkills.Count; i++)
            pool.Add(passiveSkills[i].Data);

        for (int i = 0; i < count; i++)
        {
            SkillData pick = pool[UnityEngine.Random.Range(0, pool.Count)];
            result.Add(pick);
        }

        return result;
    }

    // 결과 출력 부분
    public List<SkillResultData> GetSkillResults()
    {
        List<SkillResultData> list = new();

        for (int i = 0; i < activeSkills.Count; i++)
        {
            var s = activeSkills[i];
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
