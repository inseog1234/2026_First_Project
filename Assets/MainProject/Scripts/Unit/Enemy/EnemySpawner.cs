using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    public Enemy prefab;

    [Tooltip("등장 시작 시간")]
    public float startTime;

    [Tooltip("확률 가중치")]
    public float weight;
}

[System.Serializable]
public class MiniBossSpawnData
{
    public Enemy prefab;
    public float spawnTime;
    [HideInInspector] public bool spawned;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("플레이어")]
    private Transform player;
    private Vector2 unitOffset;

    [Header("스폰 반경 (가장자리 스폰)")]
    public Vector2 spawnSize;

    [Header("일반 몹 세팅")]
    public List<EnemySpawnData> enemies;

    [Header("중간 보스 세팅")]
    public List<MiniBossSpawnData> miniBosses;

    [Header("스폰 파라미터")]
    public int baseMaxEnemyCount = 50;
    public float baseSpawnInterval = 1f;

    private float timer;

    private void Start()
    {
        player = PlayerControll.Player.Instance.transform;
        unitOffset = PlayerControll.Player.Instance.Get_Offset();
    }

    private void Update()
    {
        float gameTime = GameTimer.Instance.RealGameTime;

        gameTime += Time.deltaTime;

        HandleMiniBoss(gameTime);

        int maxEnemyCount = baseMaxEnemyCount + (int)(gameTime / 60f) * 8;
        float spawnInterval = Mathf.Max(0.2f, 1f - gameTime * 0.015f);

        if (EnemyPooling.Instance.ActiveCount >= maxEnemyCount)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnNormalEnemy(gameTime);
        }
    }

    private void SpawnNormalEnemy(float gameTime)
    {
        Enemy prefab = GetEnemyByTime(gameTime);
        if (prefab == null) return;

        Vector2 pos = GetRandomEdgePosition();
        EnemyPooling.Instance.Get(prefab, pos);
    }

    private Enemy GetEnemyByTime(float gameTime)
    {
        List<EnemySpawnData> candidates = new();
        float totalWeight = 0f;

        foreach (var e in enemies)
        {
            if (gameTime >= e.startTime)
            {
                candidates.Add(e);
                totalWeight += e.weight;
            }
        }

        if (candidates.Count == 0) return null;

        float r = Random.Range(0, totalWeight);
        float sum = 0f;

        foreach (var e in candidates)
        {
            sum += e.weight;
            if (r <= sum)
                return e.prefab;
        }

        return candidates[0].prefab;
    }

    private void HandleMiniBoss(float gameTime)
    {
        foreach (var b in miniBosses)
        {
            if (b.spawned) continue;

            if (gameTime >= b.spawnTime)
            {
                b.spawned = true;

                Vector2 pos = GetRandomEdgePosition();
                EnemyPooling.Instance.Get(b.prefab, pos);
                break;
            }
        }
    }

    private Vector2 GetRandomEdgePosition()
    {
        float halfW = spawnSize.x * 0.5f;
        float halfH = spawnSize.y * 0.5f;

        Vector2 center = (Vector2)player.position + unitOffset;

        int side = Random.Range(0, 4);

        return side switch
        {
            0 => center + new Vector2(Random.Range(-halfW, halfW), halfH),
            1 => center + new Vector2(Random.Range(-halfW, halfW), -halfH),
            2 => center + new Vector2(-halfW, Random.Range(-halfH, halfH)),
            _ => center + new Vector2(halfW, Random.Range(-halfH, halfH))
        };
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(player.position, new Vector3(spawnSize.x, spawnSize.y, 0));
    }
}