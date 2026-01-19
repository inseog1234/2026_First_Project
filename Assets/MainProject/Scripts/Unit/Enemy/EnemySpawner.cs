using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("적 프리팹")]
    public Enemy enemyPrefab;

    [Header("최대 스폰량")]
    public int maxEnemyCount = 50;

    [Header("스폰 반경 (가장자리쪽에 생성됨)")]
    public Vector2 spawnSize;

    [Header("스폰 간격")]
    public float spawnInterval = 1f;

    private float timer;
    private Transform player;
    private Vector2 UnitOffset;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        UnitOffset = player.GetComponent<Unit>().Get_Offset();
    }

    private void Update()
    {
        if (EnemyPooling.Instance.ActiveCount >= maxEnemyCount)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            Spawn();
        }
    }

    private void Spawn()
    {
        Vector2 pos = GetRandomEdgePosition();
        EnemyPooling.Instance.Get(enemyPrefab, pos);
    }

    private Vector2 GetRandomEdgePosition()
    {
        float halfW = spawnSize.x * 0.5f;
        float halfH = spawnSize.y * 0.5f;

        Vector2 center = (Vector2)player.position + UnitOffset;

        int side = Random.Range(0, 4);

        return side switch
        {
            0 => center + new Vector2(Random.Range(-halfW, halfW), halfH),   // 위
            1 => center + new Vector2(Random.Range(-halfW, halfW), -halfH),  // 아래
            2 => center + new Vector2(-halfW, Random.Range(-halfH, halfH)),  // 왼쪽
            _ => center + new Vector2(halfW, Random.Range(-halfH, halfH))    // 오른쪽
        };
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(player.position, new Vector3(spawnSize.x, spawnSize.y, 0));
    }
}
