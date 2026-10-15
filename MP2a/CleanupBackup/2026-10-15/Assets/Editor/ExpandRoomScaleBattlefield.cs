using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ExpandRoomScaleBattlefield
{
    private const string ScenePath = "Assets/Scenes/RoomScaleDefense.unity";
    private const string MeleePrefabPath = "Assets/RoomMeleeTower.prefab";

    [InitializeOnLoadMethod]
    private static void QueueSetup()
    {
        EditorApplication.update += SetupWhenReady;
    }

    private static void SetupWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EditorApplication.update -= SetupWhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Expand Room Scale Battlefield")]
    public static void Setup()
    {
        if (!File.Exists(ScenePath) || EditorApplication.isPlayingOrWillChangePlaymode)
            return;


        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        try
        {
            Transform[] all = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            if (all.Any(t => t.name == "RoomScaleExpansionComplete"))
                return;

            TowerTargeting rapid = PrepareRangedTower("Assets/RoomRapidTower.prefab", 3.2f);
            TowerTargeting longRange = PrepareRangedTower("Assets/RoomLongRangeTower.prefab", 4.5f);
            TowerTargeting melee = CreateMeleeTower();

            Transform Find(string name) => all.First(t => t != null && t.name == name);
            Material tileA = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BoardTileA.mat");
            Material tileB = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BoardTileB.mat");

            Transform grid = all.First(t => t.name == "FloorGrid_8x6" || t.name == "ExpandedBattlefield_12x10");
            foreach (Transform child in grid.Cast<Transform>().ToArray())
                Object.DestroyImmediate(child.gameObject);
            grid.name = "ExpandedBattlefield_12x10";
            for (int x = 0; x < 12; x++)
                for (int z = 0; z < 10; z++)
                    Box("Cell_" + x + "_" + z,
                        new Vector3(-5.5f + x, 0.015f, -1.5f + z),
                        new Vector3(0.97f, 0.03f, 0.97f),
                        (x + z) % 2 == 0 ? tileA : tileB, grid);

            Vector3[] route =
            {
                new Vector3(-5.5f, 0.26f, -0.5f), new Vector3(-0.5f, 0.26f, -0.5f),
                new Vector3(-0.5f, 0.26f, 2.5f), new Vector3(3.5f, 0.26f, 2.5f),
                new Vector3(3.5f, 0.26f, 5.5f), new Vector3(-3.5f, 0.26f, 5.5f),
                new Vector3(-3.5f, 0.26f, 7.5f)
            };
            for (int i = 1; i < route.Length; i++)
            {
                Find("Waypoint" + i).position = route[i];
                Vector3 delta = route[i] - route[i - 1];
                Vector3 center = (route[i] + route[i - 1]) * 0.5f;
                center.y = 0.04f;
                Place(Find("RoadSegment" + i), center,
                    new Vector3(Mathf.Abs(delta.x) + 0.9f, 0.02f, Mathf.Abs(delta.z) + 0.9f));
            }

            Transform entry = Find("EntryMarker");
            Place(entry, new Vector3(route[0].x, 0.055f, route[0].z), new Vector3(0.7f, 0.02f, 0.7f));
            Find("EnemySpawnPoint").position = route[0];
            Find("Base").position = new Vector3(route[6].x, 0.3f, route[6].z);

            Place(Find("Floor"), new Vector3(0f, -0.1f, 3f), new Vector3(20f, 0.2f, 18f));
            Place(Find("BackWall"), new Vector3(0f, 1.8f, 12f), new Vector3(20f, 3.6f, 0.15f));
            Place(Find("LeftWall"), new Vector3(-10f, 1.8f, 3f), new Vector3(0.15f, 3.6f, 18f));
            Place(Find("RightWall"), new Vector3(10f, 1.8f, 3f), new Vector3(0.15f, 3.6f, 18f));
            Place(Find("BackWallAccent"), new Vector3(0f, 1.05f, 11.9f), new Vector3(19.8f, 0.06f, 0.03f));
            Place(Find("LeftAccent"), new Vector3(-9.9f, 1.05f, 3f), new Vector3(0.03f, 0.06f, 17.8f));
            Place(Find("RightAccent"), new Vector3(9.9f, 1.05f, 3f), new Vector3(0.03f, 0.06f, 17.8f));
            Place(Find("FrontBoundary_ColliderOnly"), new Vector3(0f, 1.8f, -6f), new Vector3(1f, 1f, 1f));
            Find("FrontBoundary_ColliderOnly").GetComponent<BoxCollider>().size = new Vector3(20f, 3.6f, 0.15f);

            Place(Find("LeftWalkway"), new Vector3(-7.2f, 0.012f, 3f), new Vector3(1.1f, 0.024f, 12f));
            Place(Find("RightWalkway"), new Vector3(7.2f, 0.012f, 3f), new Vector3(1.1f, 0.024f, 12f));
            Place(Find("FrontWalkway"), new Vector3(0f, 0.012f, -3.3f), new Vector3(15.5f, 0.024f, 1f));
            Find("ToolBench_ReservedForLater").position = new Vector3(-8.5f, 0.5f, 1f);

            Place(Find("ScoreboardPanel"), new Vector3(0f, 2.4f, 11.85f), new Vector3(6.8f, 2f, 0.08f));
            MoveText(Find("RoomTitle"), new Vector3(0f, 3.05f, 11.78f));
            MoveText(Find("BattleStatus"), new Vector3(0f, 2.5f, 11.78f));
            MoveText(Find("BuildInstructions"), new Vector3(0f, 1.85f, 11.78f));

            TowerBuildSlot template = all.Where(t => t != null).Select(t => t.GetComponent<TowerBuildSlot>()).First(slot => slot != null);
            TowerBuildSlot[] oldSlots = all.Where(t => t != null).Select(t => t.GetComponent<TowerBuildSlot>()).Where(slot => slot != null).ToArray();
            foreach (TowerBuildSlot oldSlot in oldSlots.Skip(1))
                Object.DestroyImmediate(oldSlot.gameObject);

            Vector3[] slotPositions =
            {
                new Vector3(-5.5f, 0.03f, 0.5f), new Vector3(-3.5f, 0.03f, 0.5f),
                new Vector3(-1.5f, 0.03f, 0.5f), new Vector3(0.5f, 0.03f, 1.5f),
                new Vector3(2.5f, 0.03f, 1.5f), new Vector3(-1.5f, 0.03f, 3.5f),
                new Vector3(0.5f, 0.03f, 3.5f), new Vector3(2.5f, 0.03f, 4.5f),
                new Vector3(4.5f, 0.03f, 3.5f), new Vector3(-4.5f, 0.03f, 6.5f),
                new Vector3(-1.5f, 0.03f, 6.5f), new Vector3(1.5f, 0.03f, 6.5f)
            };

            TowerBuildSlot[] slots = new TowerBuildSlot[slotPositions.Length];
            slots[0] = template;
            for (int i = 1; i < slots.Length; i++)
                slots[i] = Object.Instantiate(template.gameObject).GetComponent<TowerBuildSlot>();
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].name = "BuildSlot_" + (i + 1).ToString("00");
                slots[i].transform.position = slotPositions[i];
                slots[i].towerPrefabs = new[] { rapid, longRange, melee };
                slots[i].builtTower = null;
            }

            PlayerResources resources = all.Where(t => t != null).Select(t => t.GetComponent<PlayerResources>()).First(r => r != null);
            resources.gold = 120;

            Camera overview = Find("OverviewCamera").GetComponent<Camera>();
            overview.transform.position = new Vector3(0f, 16f, -18f);
            overview.transform.LookAt(new Vector3(0f, 0.7f, 3f));
            overview.farClipPlane = 70f;
            Transform walker = Find("DesktopWalker_NoXR");
            walker.position = new Vector3(0f, 0.05f, -4.8f);

            new GameObject("RoomScaleExpansionComplete");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Expanded battlefield ready: 12x10 grid, 12 build slots, melee tower, larger ranged tower ranges, visible range circles.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere)
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static TowerTargeting PrepareRangedTower(string path, float range)
    {
        GameObject tower = PrefabUtility.LoadPrefabContents(path);
        try
        {
            TowerTargeting targeting = tower.GetComponent<TowerTargeting>();
            targeting.range = range;
            TowerRangeIndicator indicator = tower.GetComponent<TowerRangeIndicator>();
            if (indicator == null)
                indicator = tower.AddComponent<TowerRangeIndicator>();
            indicator.targeting = targeting;
            indicator.lineMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ProjectileYellow.mat");
            PrefabUtility.SaveAsPrefabAsset(tower, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(tower);
        }
        return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<TowerTargeting>();
    }

    private static TowerTargeting CreateMeleeTower()
    {
        if (File.Exists(MeleePrefabPath))
            return AssetDatabase.LoadAssetAtPath<GameObject>(MeleePrefabPath).GetComponent<TowerTargeting>();

        Material blue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TowerBlue.mat");
        Material yellow = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ProjectileYellow.mat");
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = "RoomMeleeTower";
        root.transform.localScale = new Vector3(0.9f, 1.4f, 0.9f);
        root.GetComponent<Renderer>().sharedMaterial = blue;
        Object.DestroyImmediate(root.GetComponent<Collider>());

        GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftArm.name = "LeftHammerArm";
        leftArm.transform.SetParent(root.transform, false);
        leftArm.transform.localPosition = new Vector3(-0.7f, 0.25f, 0f);
        leftArm.transform.localScale = new Vector3(0.7f, 0.16f, 0.22f);
        leftArm.GetComponent<Renderer>().sharedMaterial = yellow;
        Object.DestroyImmediate(leftArm.GetComponent<Collider>());

        GameObject rightArm = Object.Instantiate(leftArm, root.transform);
        rightArm.name = "RightHammerArm";
        rightArm.transform.localPosition = new Vector3(0.7f, 0.25f, 0f);

        TowerTargeting targeting = root.AddComponent<TowerTargeting>();
        targeting.range = 1.35f;
        targeting.findEnemiesAutomatically = true;
        MeleeTowerAttack attack = root.AddComponent<MeleeTowerAttack>();
        attack.targeting = targeting;
        TowerRangeIndicator indicator = root.AddComponent<TowerRangeIndicator>();
        indicator.targeting = targeting;
        indicator.lineMaterial = yellow;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, MeleePrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<TowerTargeting>();
    }

    private static void Place(Transform target, Vector3 position, Vector3 scale)
    {
        target.position = position;
        target.localScale = scale;
    }

    private static void MoveText(Transform target, Vector3 position)
    {
        target.position = position;
        TMP_Text text = target.GetComponent<TMP_Text>();
        if (text != null)
            text.fontSize = Mathf.Min(text.fontSize, 2.2f);
    }

    private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent);
        Place(box.transform, position, scale);
        box.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(box.GetComponent<Collider>());
        return box;
    }
}
