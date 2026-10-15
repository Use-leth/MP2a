using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ExpandGridForVR
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Expand Grid For VR")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string path = "Assets/Scenes/RoomScaleDefense.unity";
        Scene scene = SceneManager.GetSceneByPath(path);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            Transform[] all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            if (all.Any(t => t.name == "GridSpacing110Complete")) return;
            Transform grid = all.Single(t => t.name == "ExpandedBattlefield_12x10");
            TowerBuildSlot[] slots = grid.GetComponentsInChildren<TowerBuildSlot>(true);
            if (slots.Length != 120) throw new System.InvalidOperationException("Expected 120 grid cells; expansion stopped.");
            Vector3 center = slots.Aggregate(Vector3.zero, (sum, cell) => sum + cell.transform.position) / slots.Length;
            Vector3 Expanded(Vector3 p) => new Vector3(center.x + (p.x - center.x) * 1.1f, p.y, center.z + (p.z - center.z) * 1.1f);
            SimpleWaveController wave = all.Select(t => t.GetComponent<SimpleWaveController>()).First(w => w != null);
            BaseHealth health = wave.baseHealth;
            Vector3 baseDelta = Expanded(health.transform.position) - health.transform.position;
            // Food is a separate scene prefab, so move its common parent with the base.
            Transform food = health.foodStages != null && health.foodStages.Length > 0 && health.foodStages[0] != null
                ? health.foodStages[0].transform.parent : null;
            if (food != null && !food.IsChildOf(health.transform)) food.position += baseDelta;
            health.transform.position += baseDelta;
            foreach (TowerBuildSlot slot in slots)
            {
                Vector3 point = Expanded(slot.buildPoint.position);
                slot.transform.position = Expanded(slot.transform.position);
                Vector3 scale = slot.transform.localScale;
                slot.transform.localScale = new Vector3(scale.x * 1.1f, scale.y, scale.z * 1.1f);
                slot.buildPoint.position = point;
                if (slot.builtTower != null) slot.builtTower.transform.position = point;
            }
            foreach (Transform point in wave.waypoints.Append(wave.spawnPoint).Distinct()) point.position = Expanded(point.position);
            foreach (Transform part in all.Where(t => t.name.StartsWith("RoadSegment") || t.name == "EntryMarker"))
            {
                part.position = Expanded(part.position);
                Vector3 scale = part.localScale;
                part.localScale = new Vector3(scale.x * 1.1f, scale.y, scale.z * 1.1f);
            }
            foreach (Transform walkway in all.Where(t => t.name == "LeftWalkway" || t.name == "RightWalkway"))
            {
                Vector3 p = walkway.position;
                p.x += p.x < center.x ? -0.6f : 0.6f;
                walkway.position = p;
            }
            string[] towers = { "RoomRapidTower", "RoomLongRangeTower", "RoomSodaTower", "RoomCoolingFan", "RoomDamageAmplifier" };
            float[] ranges = { 3.52f, 4.95f, 3.3f, 1.65f, 1.65f };
            for (int i = 0; i < towers.Length; i++)
            {
                string prefabPath = "Assets/" + towers[i] + ".prefab";
                GameObject tower = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    tower.GetComponent<TowerTargeting>().range = ranges[i];
                    PrefabUtility.SaveAsPrefabAsset(tower, prefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(tower); }
            }
            // Every waypoint must remain centered on a forbidden road cell.
            foreach (Transform point in wave.waypoints.Append(wave.spawnPoint))
            {
                bool aligned = slots.Any(s => s.isPath && Vector2.Distance(new Vector2(s.transform.position.x, s.transform.position.z),
                    new Vector2(point.position.x, point.position.z)) < 0.02f);
                if (!aligned) throw new System.InvalidOperationException("Path/grid alignment failed at " + point.name);
            }
            new GameObject("GridSpacing110Complete");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("VR grid expansion complete: 120 cells, 1.1m spacing, paths aligned, tower ranges +10%. Room decoration retained.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
