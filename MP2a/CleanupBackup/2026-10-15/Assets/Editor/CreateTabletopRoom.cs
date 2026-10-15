using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// One-time scene authoring only. Everything created is saved as editable scene objects.
public static class CreateTabletopRoom
{
    private const string ScenePath = "Assets/Scenes/TabletopRoom.unity";

    [InitializeOnLoadMethod]
    private static void QueueCreation()
    {
        EditorApplication.update += CreateWhenReady;
    }

    private static void CreateWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        EditorApplication.update -= CreateWhenReady;
        if (!File.Exists(ScenePath))
            Create();
    }

    [MenuItem("Tools/MP2a/Create Tabletop Room")]
    public static void Create()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath))
            return;

        Scene previous = SceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            Material floor = MaterialAsset("RoomFloor", new Color(0.12f, 0.16f, 0.2f));
            Material wall = MaterialAsset("RoomWall", new Color(0.27f, 0.33f, 0.38f));
            Material wood = MaterialAsset("TableFrame", new Color(0.2f, 0.12f, 0.075f));
            Material tileA = MaterialAsset("BoardTileA", new Color(0.15f, 0.25f, 0.24f));
            Material tileB = MaterialAsset("BoardTileB", new Color(0.18f, 0.29f, 0.27f));
            Material road = MaterialAsset("RoadSand", new Color(0.7f, 0.59f, 0.38f));
            Material dark = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TabletopGray.mat");
            Material blue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TowerBlue.mat");
            Material green = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BaseGreen.mat");
            Material red = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/EnemyRed.mat");
            Material yellow = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ProjectileYellow.mat");

            Transform room = new GameObject("Room").transform;
            Box("Floor", new Vector3(0f, -0.1f, 0.5f), new Vector3(8f, 0.2f, 7f), floor, room, true);
            Box("BackWall", new Vector3(0f, 1.6f, 4f), new Vector3(8f, 3.2f, 0.15f), wall, room, true);
            Box("LeftWall", new Vector3(-4f, 1.6f, 0.5f), new Vector3(0.15f, 3.2f, 7f), wall, room, true);
            Box("RightWall", new Vector3(4f, 1.6f, 0.5f), new Vector3(0.15f, 3.2f, 7f), wall, room, true);
            // The front is open so the desktop overview camera can see into the room.
            Box("BackWallAccent", new Vector3(0f, 1.05f, 3.9f), new Vector3(7.8f, 0.06f, 0.03f), blue, room);
            Box("LeftAccent", new Vector3(-3.9f, 1.05f, 0.5f), new Vector3(0.03f, 0.06f, 6.8f), blue, room);
            Box("RightAccent", new Vector3(3.9f, 1.05f, 0.5f), new Vector3(0.03f, 0.06f, 6.8f), blue, room);

            Transform table = new GameObject("BattleTable").transform;
            Box("Tabletop", new Vector3(0f, 0.85f, 1f), new Vector3(4.2f, 0.14f, 3.2f), wood, table, true);
            foreach (float x in new[] { -1.8f, 1.8f })
                foreach (float z in new[] { -0.3f, 2.3f })
                    Box("TableLeg", new Vector3(x, 0.39f, z), new Vector3(0.15f, 0.78f, 0.15f), wood, table, true);

            Transform board = new GameObject("Board_8x6_VisualGrid").transform;
            board.SetParent(table);
            for (int x = 0; x < 8; x++)
                for (int z = 0; z < 6; z++)
                    Box("Cell_" + x + "_" + z, new Vector3(-1.75f + x * 0.5f, 0.93f, -0.25f + z * 0.5f),
                        new Vector3(0.485f, 0.02f, 0.485f), (x + z) % 2 == 0 ? tileA : tileB, board);

            Vector3[] route = {
                new Vector3(-1.75f, 1.064f, -0.25f), new Vector3(-0.25f, 1.064f, -0.25f),
                new Vector3(-0.25f, 1.064f, 0.75f), new Vector3(1.25f, 1.064f, 0.75f),
                new Vector3(1.25f, 1.064f, 1.75f), new Vector3(-1.25f, 1.064f, 1.75f),
                new Vector3(-1.25f, 1.064f, 2.25f)
            };
            Transform path = new GameObject("Path").transform;
            path.SetParent(table);
            Transform[] waypoints = new Transform[route.Length - 1];
            for (int i = 1; i < route.Length; i++)
            {
                Transform waypoint = new GameObject("Waypoint" + i).transform;
                waypoint.SetParent(path);
                waypoint.position = route[i];
                waypoints[i - 1] = waypoint;
                Vector3 delta = route[i] - route[i - 1];
                Vector3 center = (route[i] + route[i - 1]) * 0.5f;
                center.y = 0.947f;
                Box("RoadSegment" + i, center,
                    new Vector3(Mathf.Abs(delta.x) + 0.42f, 0.014f, Mathf.Abs(delta.z) + 0.42f), road, path);
            }
            Box("EntryMarker", new Vector3(-1.75f, 0.96f, -0.25f), new Vector3(0.32f, 0.02f, 0.32f), red, path);

            GameObject baseObject = Box("Base", new Vector3(-1.25f, 1.015f, 2.25f), new Vector3(0.42f, 0.13f, 0.42f), green, table);
            BaseHealth baseHealth = baseObject.AddComponent<BaseHealth>();
            baseHealth.lives = 5;
            PlayerResources resources = new GameObject("PlayerResources").AddComponent<PlayerResources>();

            Transform enemyGroup = new GameObject("Enemies").transform;
            EnemyHealth[] enemies = new EnemyHealth[5];
            for (int i = 0; i < enemies.Length; i++)
            {
                GameObject enemy = Box("Enemy" + (i + 1), route[0] + Vector3.right * (i * 0.28f), Vector3.one * 0.22f, red, enemyGroup);
                EnemyHealth health = enemy.AddComponent<EnemyHealth>();
                health.health = 4f;
                health.goldReward = 10;
                health.playerResources = resources;
                enemies[i] = health;
                EnemyPathFollower follower = enemy.AddComponent<EnemyPathFollower>();
                follower.moveSpeed = 0.4f;
                follower.waypoints = waypoints;
                follower.baseHealth = baseHealth;

                GameObject bar = new GameObject("HealthBar");
                bar.transform.SetParent(enemy.transform, false);
                bar.transform.localPosition = new Vector3(0f, 0.85f, 0f);
                GameObject background = Box("Background", Vector3.zero, new Vector3(1.25f, 0.15f, 0.15f), dark, null);
                background.transform.SetParent(bar.transform, false);
                GameObject fill = Box("Fill", Vector3.zero, new Vector3(1.2f, 0.1f, 0.17f), green, null);
                fill.transform.SetParent(bar.transform, false);
                EnemyHealthBar healthBar = bar.AddComponent<EnemyHealthBar>();
                healthBar.health = health;
                healthBar.fill = fill.transform;
            }

            Transform towers = new GameObject("Towers").transform;
            Projectile projectile = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Projectile.prefab").GetComponent<Projectile>();
            for (int i = 0; i < 2; i++)
            {
                Vector3 position = i == 0 ? new Vector3(-0.75f, 1.105f, 0.25f) : new Vector3(0.75f, 1.105f, 1.25f);
                GameObject tower = Box(i == 0 ? "RapidTower" : "LongRangeTower", position, new Vector3(0.28f, 0.33f, 0.28f), i == 0 ? blue : yellow, towers);
                TowerTargeting targeting = tower.AddComponent<TowerTargeting>();
                targeting.enemies = enemies;
                targeting.range = i == 0 ? 0.95f : 1.15f;
                Transform firePoint = new GameObject("FirePoint").transform;
                firePoint.SetParent(tower.transform, false);
                firePoint.localPosition = new Vector3(0f, 0.7f, 0f);
                GameObject cap = Box("TurretTop", Vector3.zero, new Vector3(0.85f, 0.25f, 0.85f), dark, null);
                cap.transform.SetParent(tower.transform, false);
                cap.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                TowerShooting shooting = tower.AddComponent<TowerShooting>();
                shooting.targeting = targeting;
                shooting.projectilePrefab = projectile;
                shooting.firePoint = firePoint;
                shooting.fireInterval = i == 0 ? 0.6f : 1f;
            }

            Transform bench = new GameObject("ToolBench_ReservedForLater").transform;
            Box("BenchTop", new Vector3(-3f, 0.8f, 1f), new Vector3(0.7f, 0.12f, 1.8f), wood, bench, true);
            Box("BenchCabinet", new Vector3(-3f, 0.37f, 1f), new Vector3(0.6f, 0.74f, 1.6f), dark, bench, true);

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (font == null)
                throw new System.InvalidOperationException("Import TMP Essential Resources before creating TabletopRoom.");
            Box("ScoreboardPanel", new Vector3(0f, 2.1f, 3.82f), new Vector3(3.7f, 1f, 0.08f), dark, room);
            Label("RoomTitle", "TABLETOP COMMAND", new Vector3(0f, 2.42f, 3.76f), new Vector2(3.5f, 0.35f), 2.3f, font, room);
            TextMeshPro scoreText = Label("BattleStatus", "DEFEND THE BASE\nGOLD  0     LIVES  5     ENEMIES  5", new Vector3(0f, 2.02f, 3.76f), new Vector2(3.5f, 0.65f), 1.7f, font, room);
            BattleReadout readout = scoreText.gameObject.AddComponent<BattleReadout>();
            readout.resources = resources;
            readout.baseHealth = baseHealth;
            readout.enemies = enemies;
            readout.display = scoreText;
            Label("BenchLabel", "TOOL BENCH", new Vector3(-3f, 1.2f, 1.5f), new Vector2(1.2f, 0.3f), 1.5f, font, bench);

            Light sun = new GameObject("RoomLight").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.3f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.65f, 0.7f, 0.75f);

            Camera camera = new GameObject("DesktopCamera_DisableForVR").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 4.4f, -4.7f);
            camera.transform.LookAt(new Vector3(0f, 1f, 1.25f));
            camera.fieldOfView = 52f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 30f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.09f, 0.13f);
            camera.gameObject.AddComponent<AudioListener>();

            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) > 0)
                        throw new System.InvalidOperationException("Missing script: " + child.name);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new System.InvalidOperationException("Could not save TabletopRoom.");

            RenderTexture preview = new RenderTexture(1440, 1000, 24);
            RenderTexture previousTarget = RenderTexture.active;
            // Preview only the new scene, even if a test scene is already open.
            // This temporary preview layer is applied after saving the scene.
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    child.gameObject.layer = 31;
            camera.cullingMask = 1 << 31;
            camera.targetTexture = preview;
            camera.Render();
            RenderTexture.active = preview;
            Texture2D image = new Texture2D(1440, 1000, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1440, 1000), 0, 0);
            image.Apply();
            Directory.CreateDirectory("Previews");
            File.WriteAllBytes("Previews/TabletopRoom.png", image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previousTarget;
            Object.DestroyImmediate(image);
            Object.DestroyImmediate(preview);
            Debug.Log("TabletopRoom ready: 8x6 board, room, 5 enemies, 2 towers, status display and health bars. Play test pending.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static Material MaterialAsset(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
            return material;
        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", 0.15f);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, Transform parent, bool collider = false)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.position = position;
        box.transform.localScale = scale;
        box.transform.SetParent(parent, true);
        box.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider)
            Object.DestroyImmediate(box.GetComponent<Collider>());
        return box;
    }

    private static TextMeshPro Label(string name, string text, Vector3 position, Vector2 size, float fontSize, TMP_FontAsset font, Transform parent)
    {
        TextMeshPro label = new GameObject(name).AddComponent<TextMeshPro>();
        label.transform.SetParent(parent);
        label.transform.position = position;
        label.rectTransform.sizeDelta = size;
        label.font = font;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.9f, 0.95f, 1f);
        label.text = text;
        return label;
    }
}
