using UnityEditor;
using UnityEngine;

public static class SetupRoadObstacles
{
    // Paused: only the explicit menu command may re-enable this prototype.
    private static void Queue() { EditorApplication.update += WhenReady; }

    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Road Obstacles")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string towerPath = "Assets/RoomMeleeTower.prefab";
        const string enemyPath = "Assets/RoomScaleEnemy.prefab";
        GameObject tower = PrefabUtility.LoadPrefabContents(towerPath);
        try
        {
            if (tower.GetComponent<TowerObstacle>() == null)
            {
                TowerObstacle obstacle = tower.AddComponent<TowerObstacle>();
                obstacle.body = tower.GetComponent<Renderer>();
                PrefabUtility.SaveAsPrefabAsset(tower, towerPath);
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(tower); }
        GameObject enemy = PrefabUtility.LoadPrefabContents(enemyPath);
        try
        {
            EnemyPathFollower follower = enemy.GetComponent<EnemyPathFollower>();
            if (!follower.attackObstacles)
            {
                follower.attackObstacles = true;
                PrefabUtility.SaveAsPrefabAsset(enemy, enemyPath);
                Debug.Log("Road obstacles ready: 30 HP, enemy attacks, E to repair.");
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(enemy); }
    }
}
