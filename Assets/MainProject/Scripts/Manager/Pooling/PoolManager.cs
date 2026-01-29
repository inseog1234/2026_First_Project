using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [SerializeField] private EnemyPooling enemyPooling;
    [SerializeField] private ExpOrbPooling expOrbPooling;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 게터

    // 
    public EnemyPooling Enemy => enemyPooling;
    public ExpOrbPooling ExpOrb => expOrbPooling;
}
