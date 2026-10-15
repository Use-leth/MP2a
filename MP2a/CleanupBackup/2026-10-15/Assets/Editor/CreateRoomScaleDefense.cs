using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time conversion of the tested desktop scene into a separate room-scale sample.
public static class CreateRoomScaleDefense
{
    private const string ScenePath = "Assets/Scenes/RoomScaleDefense.unity";

    [InitializeOnLoadMethod]
    private static void QueueCreation()
    {
        EditorApplication.update += WhenReady;
    }

    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        EditorApplication.update -= WhenReady;
        if (!File.Exists(ScenePath))
            Create();
    }

    [MenuItem("Tools/MP2a/Create Room Scale Defense")]
    public static void Create()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath))
            return;
        if (!AssetDatabase.CopyAsset("Assets/Scenes/TabletopRoom.unity", ScenePath))
            throw new System.InvalidOperationException("Could not copy TabletopRoom.");

        Scene previous = SceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            Transform[] objects = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
            Transform Find(string name) => objects.First(t => t != null && t.name == name);
            T Component<T>() where T : UnityEngine.Component => objects.Where(t => t != null).Select(t => t.GetComponent<T>()).First(c => c != null);

            Object.DestroyImmediate(Find("Tabletop").gameObject);
            foreach (Transform leg in objects.Where(t => t != null && t.name == "TableLeg"))
                Object.DestroyImmediate(leg.gameObject);
            Find("BattleTable").name = "GroundBattlefield";
            Find("Board_8x6_VisualGrid").name = "FloorGrid_8x6";

            // Double horizontal spacing; move the battle surface down onto the floor.
            foreach (Transform tile in objects.Where(t => t != null && t.name.StartsWith("Cell_")))
            {
                tile.position = new Vector3(tile.position.x * 2f, 0.015f, tile.position.z * 2f);
                tile.localScale = new Vector3(0.97f, 0.03f, 0.97f);
            }
            foreach (Transform road in objects.Where(t => t != null && t.name.StartsWith("RoadSegment")))
            {
                road.position = new Vector3(road.position.x * 2f, 0.04f, road.position.z * 2f);
                road.localScale = new Vector3(road.localScale.x * 2f, 0.02f, road.localScale.z * 2f);
            }
            Transform entry = Find("EntryMarker");
            entry.position = new Vector3(-3.5f, 0.055f, -0.5f);
            entry.localScale = new Vector3(0.7f, 0.02f, 0.7f);
            foreach (Transform waypoint in objects.Where(t => t != null && t.name.StartsWith("Waypoint")))
                waypoint.position = new Vector3(waypoint.position.x * 2f, 0.26f, waypoint.position.z * 2f);
            foreach (EnemyHealth enemy in objects.Where(t => t != null).Select(t => t.GetComponent<EnemyHealth>()).Where(e => e != null))
            {
                enemy.transform.position = new Vector3(enemy.transform.position.x * 2f, 0.26f, enemy.transform.position.z * 2f);
                enemy.transform.localScale = Vector3.one * 0.42f;
                enemy.GetComponent<EnemyPathFollower>().moveSpeed = 0.8f;
            }
            Transform baseObject = Find("Base");
            baseObject.position = new Vector3(-2.5f, 0.3f, 4.5f);
            baseObject.localScale = new Vector3(0.9f, 0.6f, 0.9f);

            Place(Find("Floor"), new Vector3(0f, -0.1f, 2f), new Vector3(12f, 0.2f, 11f));
            Place(Find("BackWall"), new Vector3(0f, 1.8f, 7.5f), new Vector3(12f, 3.6f, 0.15f));
            Place(Find("LeftWall"), new Vector3(-6f, 1.8f, 2f), new Vector3(0.15f, 3.6f, 11f));
            Place(Find("RightWall"), new Vector3(6f, 1.8f, 2f), new Vector3(0.15f, 3.6f, 11f));
            Place(Find("BackWallAccent"), new Vector3(0f, 1.05f, 7.4f), new Vector3(11.8f, 0.06f, 0.03f));
            Place(Find("LeftAccent"), new Vector3(-5.9f, 1.05f, 2f), new Vector3(0.03f, 0.06f, 10.8f));
            Place(Find("RightAccent"), new Vector3(5.9f, 1.05f, 2f), new Vector3(0.03f, 0.06f, 10.8f));
            Transform bench = Find("ToolBench_ReservedForLater");
            bench.position += new Vector3(-2.5f, 0f, 1f);
            Find("BenchLabel").GetComponent<TMP_Text>().text = "MAINTENANCE BENCH";
            Find("BenchLabel").GetComponent<TMP_Text>().fontSize = 1.2f;

            Material laneMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TowerBlue.mat");
            Transform lanes = new GameObject("MaintenanceWalkways").transform;
            Box("LeftWalkway", new Vector3(-4.7f, 0.012f, 2f), new Vector3(1.1f, 0.024f, 6.8f), laneMaterial, lanes);
            Box("RightWalkway", new Vector3(4.7f, 0.012f, 2f), new Vector3(1.1f, 0.024f, 6.8f), laneMaterial, lanes);
            Box("FrontWalkway", new Vector3(0f, 0.012f, -1.7f), new Vector3(10.5f, 0.024f, 1f), laneMaterial, lanes);
            GameObject frontBoundary = new GameObject("FrontBoundary_ColliderOnly");
            frontBoundary.transform.position = new Vector3(0f, 1.8f, -3.5f);
            frontBoundary.AddComponent<BoxCollider>().size = new Vector3(12f, 3.6f, 0.15f);

            TowerTargeting rapid = TowerPrefab("RapidTower", "RoomRapidTower", 2.2f);
            TowerTargeting longRange = TowerPrefab("LongRangeTower", "RoomLongRangeTower", 2.7f);
            TowerBuildSlot[] slots = objects.Where(t => t != null).Select(t => t.GetComponent<TowerBuildSlot>()).Where(s => s != null).OrderBy(s => s.name).ToArray();
            for (int i = 0; i < slots.Length; i++)
            {
                if (i >= 2)
                {
                    Object.DestroyImmediate(slots[i].gameObject);
                    continue;
                }
                TowerBuildSlot slot = slots[i];
                slot.transform.position = i == 0 ? new Vector3(-3.5f, 0.03f, 0.5f) : new Vector3(3.5f, 0.03f, 2.5f);
                slot.towerPrefabs = new[] { rapid, longRange };
                slot.buildPoint.localPosition = new Vector3(0f, 0.7f, 0f);
                slot.buildPoint.localRotation = Quaternion.Euler(0f, i == 0 ? 90f : -90f, 0f);
                BoxCollider collider = slot.GetComponent<BoxCollider>();
                collider.size = new Vector3(0.95f, 1.6f, 0.95f);
                collider.center = new Vector3(0f, 0.8f, 0f);
                slot.transform.Find("GreenPad").localScale = new Vector3(1f, 0.025f, 1f);
            }
            Component<PlayerResources>().gold = 40;

            Place(Find("ScoreboardPanel"), new Vector3(0f, 2.4f, 7.35f), new Vector3(5.8f, 2f, 0.08f));
            TextMeshPro title = Find("RoomTitle").GetComponent<TextMeshPro>();
            title.text = "DEFENSE WORKSHOP";
            title.transform.position = new Vector3(0f, 3.05f, 7.28f);
            title.rectTransform.sizeDelta = new Vector2(5.5f, 0.4f);
            title.fontSize = 3f;
            TextMeshPro status = Find("BattleStatus").GetComponent<TextMeshPro>();
            status.transform.position = new Vector3(0f, 2.5f, 7.28f);
            status.rectTransform.sizeDelta = new Vector2(5.5f, 0.65f);
            status.fontSize = 2.4f;
            status.text = "BUILD YOUR DEFENSES\nGOLD  40     LIVES  5     ENEMIES  5";
            TextMeshPro instructions = Find("BuildInstructions").GetComponent<TextMeshPro>();
            instructions.transform.position = new Vector3(0f, 1.85f, 7.28f);
            instructions.rectTransform.sizeDelta = new Vector2(5.5f, 0.55f);
            instructions.fontSize = 1.45f;

            Camera overview = Find("DesktopCamera").GetComponent<Camera>();
            overview.name = "OverviewCamera";
            overview.transform.position = new Vector3(0f, 9f, -10f);
            overview.transform.LookAt(new Vector3(0f, 0.7f, 2f));
            overview.farClipPlane = 50f;
            GameObject player = new GameObject("DesktopWalker_NoXR");
            player.transform.position = new Vector3(0f, 0.05f, -2.2f);
            CharacterController walker = player.AddComponent<CharacterController>();
            walker.height = 1.8f;
            walker.center = new Vector3(0f, 0.9f, 0f);
            walker.radius = 0.23f;
            walker.stepOffset = 0.15f;
            Camera standing = new GameObject("StandingCamera_1.65m").AddComponent<Camera>();
            standing.transform.SetParent(player.transform, false);
            standing.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            standing.nearClipPlane = 0.05f;
            standing.farClipPlane = 50f;
            standing.fieldOfView = 70f;
            standing.enabled = false;
            standing.gameObject.AddComponent<AudioListener>().enabled = false;
            RoomViewControls controls = player.AddComponent<RoomViewControls>();
            controls.overviewCamera = overview;
            controls.standingCamera = standing;
            controls.walker = walker;
            controls.buildInput = Component<DesktopBuildInput>();
            controls.buildInput.viewCamera = overview;
            controls.buildInput.extraInstructions = "TAB: view   WASD: walk   Hold middle mouse: look";

            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) > 0)
                        throw new System.InvalidOperationException("Missing script: " + child.name);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new System.InvalidOperationException("Could not save RoomScaleDefense.");
            CapturePreview(scene, overview);
            Debug.Log("RoomScaleDefense ready: ground board, 2 full-size build slots, maintenance lanes, Tab view toggle. Play test pending.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void Place(Transform target, Vector3 position, Vector3 scale)
    {
        target.position = position;
        target.localScale = scale;
    }

    private static void CapturePreview(Scene scene, Camera camera)
    {
        // Capture after saving; temporary preview changes are discarded when the scene closes.
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = 31;
        camera.cullingMask = 1 << 31;
        RenderTexture target = new RenderTexture(1440, 1000, 24);
        RenderTexture previous = RenderTexture.active;
        Texture2D image = new Texture2D(1440, 1000, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1440, 1000), 0, 0);
            image.Apply();
            Directory.CreateDirectory("Previews");
            File.WriteAllBytes("Previews/RoomScaleDefense.png", image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            Object.DestroyImmediate(image);
            Object.DestroyImmediate(target);
        }
    }

    private static void Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent);
        Place(box.transform, position, scale);
        box.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(box.GetComponent<Collider>());
    }

    private static TowerTargeting TowerPrefab(string source, string destination, float range)
    {
        string path = "Assets/" + destination + ".prefab";
        if (!File.Exists(path))
            AssetDatabase.CopyAsset("Assets/" + source + ".prefab", path);
        GameObject tower = PrefabUtility.LoadPrefabContents(path);
        try
        {
            tower.name = destination;
            tower.transform.localScale = new Vector3(0.8f, 1.4f, 0.8f);
            tower.GetComponent<TowerTargeting>().range = range;
            // A visible panel marks the future repair interaction area, without adding repair logic.
            if (tower.transform.Find("RepairPanel_VisualOnly") == null)
            {
                GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = "RepairPanel_VisualOnly";
                panel.transform.SetParent(tower.transform, false);
                panel.transform.localPosition = new Vector3(0f, 0.05f, -0.51f);
                panel.transform.localScale = new Vector3(0.55f, 0.28f, 0.05f);
                panel.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BaseGreen.mat");
                Object.DestroyImmediate(panel.GetComponent<Collider>());
            }
            PrefabUtility.SaveAsPrefabAsset(tower, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(tower);
        }
        return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<TowerTargeting>();
    }
}
