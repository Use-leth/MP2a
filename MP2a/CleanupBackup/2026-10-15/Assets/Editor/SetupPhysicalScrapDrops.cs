using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupPhysicalScrapDrops
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }

    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Physical Scrap Drops")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string canPath = "Assets/ScrapCan.prefab";
        const string enemyPath = "Assets/RoomScaleEnemy.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(canPath) == null) return;
        GameObject can = PrefabUtility.LoadPrefabContents(canPath);
        try
        {
            ScrapPickup pickup = can.GetComponent<ScrapPickup>();
            if (pickup.appearances == null || pickup.appearances.Length == 0)
            {
                pickup.appearances = can.transform.Cast<Transform>()
                    .Where(child => child.GetComponentInChildren<MeshFilter>(true) != null
                        && child.GetComponentInChildren<TMPro.TMP_Text>(true) == null)
                    .Select(child => child.gameObject).ToArray();
                PrefabUtility.SaveAsPrefabAsset(can, canPath);
            }
            if (can.GetComponent<Rigidbody>() == null)
            {
                Rigidbody body = can.AddComponent<Rigidbody>();
                body.mass = 0.5f;
                body.useGravity = true;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                PrefabUtility.SaveAsPrefabAsset(can, canPath);
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(can); }
        ScrapPickup prefab = AssetDatabase.LoadAssetAtPath<GameObject>(canPath).GetComponent<ScrapPickup>();
        GameObject enemy = PrefabUtility.LoadPrefabContents(enemyPath);
        try
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health.scrapDropPrefab != prefab)
            {
                health.scrapDropPrefab = prefab;
                PrefabUtility.SaveAsPrefabAsset(enemy, enemyPath);
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(enemy); }

        const string scenePath = "Assets/Scenes/RoomScaleDefense.unity";
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            bool changed = false;
            foreach (ScrapSpawner spawner in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<ScrapSpawner>(true)))
            {
                if (!spawner.enabled) continue;
                spawner.enabled = false;
                changed = true;
            }
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Physical scrap ready: enemy death drops, Rigidbody + BoxCollider, no grid spawning or reservation.");
            }
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
