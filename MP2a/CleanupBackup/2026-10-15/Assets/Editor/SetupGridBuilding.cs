using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupGridBuilding
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }

    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Grid Building")]
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
            Transform[] all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
            if (all.Any(t => t.name == "GridBuildingReady")) return;
            Transform grid = all.FirstOrDefault(t => t.name == "ExpandedBattlefield_12x10");
            if (grid == null) return;
            TowerBuildSlot[] oldSlots = all.Select(t => t.GetComponent<TowerBuildSlot>()).Where(s => s != null && !s.name.StartsWith("Cell_")).ToArray();
            PlayerResources resources = all.Select(t => t.GetComponent<PlayerResources>()).First(r => r != null);
            SimpleWaveController waves = all.Select(t => t.GetComponent<SimpleWaveController>()).First(w => w != null);
            DesktopBuildInput input = all.Select(t => t.GetComponent<DesktopBuildInput>()).First(b => b != null);
            TowerTargeting[] prefabs = new[] { "RoomRapidTower", "RoomLongRangeTower", "RoomMeleeTower" }
                .Select(n => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/" + n + ".prefab").GetComponent<TowerTargeting>()).ToArray();
            Vector3[] route = new[] { waves.spawnPoint.position }.Concat(waves.waypoints.Select(w => w.position)).ToArray();
            int ground = 0, road = 0;
            foreach (Transform cell in grid)
            {
                if (!cell.name.StartsWith("Cell_")) continue;
                Vector3 p = cell.position;
                bool onPath = false;
                for (int i = 1; i < route.Length; i++)
                {
                    Vector3 a = route[i - 1], b = route[i];
                    a.y = b.y = p.y;
                    Vector3 ab = b - a;
                    float t = ab.sqrMagnitude > 0 ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude) : 0;
                    if (Vector3.Distance(p, a + ab * t) < 0.1f) onPath = true;
                }
                bool reserved = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(route[0].x, route[0].z)) < 0.1f
                    || Vector2.Distance(new Vector2(p.x, p.z), new Vector2(route[route.Length - 1].x, route[route.Length - 1].z)) < 0.1f;
                // Road cells sit above the old road strips so hover color stays visible.
                if (onPath)
                {
                    cell.position = new Vector3(p.x, 0.055f, p.z);
                    cell.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RoadSand.mat");
                }
                TowerBuildSlot slot = cell.GetComponent<TowerBuildSlot>();
                if (slot == null) slot = cell.gameObject.AddComponent<TowerBuildSlot>();
                slot.resources = resources;
                slot.towerPrefabs = prefabs;
                slot.restrictTowerType = true;
                slot.isPath = onPath;
                slot.blocked = reserved;
                slot.cellRenderer = cell.GetComponent<Renderer>();
                Transform point = cell.Find("BuildPoint");
                if (point == null)
                {
                    point = new GameObject("BuildPoint").transform;
                    point.SetParent(cell, false);
                }
                point.position = new Vector3(p.x, 0.73f, p.z);
                slot.buildPoint = point;
                BoxCollider collider = cell.GetComponent<BoxCollider>();
                if (collider == null) collider = cell.gameObject.AddComponent<BoxCollider>();
                collider.size = Vector3.one;
                if (!reserved) { if (onPath) road++; else ground++; }
            }
            foreach (TowerBuildSlot old in oldSlots) Object.DestroyImmediate(old.gameObject);
            TowerRangeIndicator preview = new GameObject("PlacementRangePreview").AddComponent<TowerRangeIndicator>();
            preview.lineMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ProjectileYellow.mat");
            preview.showRange = false;
            input.placementPreview = preview;
            new GameObject("GridBuildingReady");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Grid building ready: " + ground + " ranged cells, " + road + " melee road cells, 2 reserved cells.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
