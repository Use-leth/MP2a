using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupBaseFoodHealth
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Connect Base Food Health")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string path = "Assets/Scenes/RoomScaleDefense.unity";
        Scene scene = SceneManager.GetSceneByPath(path);
        bool openedHere = !scene.isLoaded;
        if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        try
        {
            var roots = scene.GetRootGameObjects();
            BaseHealth health = roots.SelectMany(r => r.GetComponentsInChildren<BaseHealth>(true)).Single();
            bool changed = false;
            if (health.foodStages == null || health.foodStages.Length == 0)
            {
                Consumer food = roots.SelectMany(r => r.GetComponentsInChildren<Consumer>(true))
                    .Single(c => c.name == "Ham");
                food.enabled = false;
                health.foodStages = new[] { "Ham4/4", "Ham3/4", "Ham2/4", "Ham1/4" }
                    .Select(n => food.transform.Cast<Transform>().Single(t => t.name == n).gameObject).ToArray();
                for (int i = 0; i < health.foodStages.Length; i++) health.foodStages[i].SetActive(i == 0);
                EditorUtility.SetDirty(food);
                EditorUtility.SetDirty(health);
                changed = true;
            }
            SimpleWaveController wave = roots.SelectMany(r => r.GetComponentsInChildren<SimpleWaveController>(true)).Single();
            if (wave.spawnPoint.parent != wave.waypoints[0].parent)
            {
                wave.spawnPoint.SetParent(wave.waypoints[0].parent, true);
                Vector3 position = wave.spawnPoint.position;
                position.z = wave.waypoints[0].position.z;
                position.y = wave.waypoints[0].position.y;
                wave.spawnPoint.position = position;
                changed = true;
            }
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Base food now follows lives; spawn aligned to the first road and parented under Path.");
            }
            for (int i = 1; i < wave.waypoints.Length; i++)
            {
                Vector3 delta = wave.waypoints[i].position - wave.waypoints[i - 1].position;
                if (Mathf.Abs(delta.x) > 0.01f && Mathf.Abs(delta.z) > 0.01f)
                    Debug.LogWarning("Diagonal path segment: " + i);
            }
        }
        finally { if (openedHere) EditorSceneManager.CloseScene(scene, true); }
    }
}
