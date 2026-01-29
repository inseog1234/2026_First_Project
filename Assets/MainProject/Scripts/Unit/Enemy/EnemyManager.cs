using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static readonly List<Enemy> ActiveEnemies = new();

    public static void Register(Enemy enemy)
    {
        if (enemy == null) return;
        ActiveEnemies.Add(enemy);
    }

    public static void Unregister(Enemy enemy)
    {
        if (enemy == null) return;
        ActiveEnemies.Remove(enemy);
    }
}
