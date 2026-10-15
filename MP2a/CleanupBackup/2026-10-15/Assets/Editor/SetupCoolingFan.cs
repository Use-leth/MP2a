using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupCoolingFan
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Cooling Fan")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Material blue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TowerBlue.mat");
        foreach (string name in new[] { "RoomRapidTower", "RoomLongRangeTower", "RoomSodaTower" })
        {
            string path = "Assets/" + name + ".prefab";
            GameObject tower = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (tower.GetComponent<TowerCooling>() != null) continue;
                TowerCooling cooling = tower.AddComponent<TowerCooling>();
                TowerShooting shooting = tower.GetComponent<TowerShooting>();
                if (shooting != null) shooting.cooling = cooling;
                SodaTower soda = tower.GetComponent<SodaTower>();
                if (soda != null) soda.cooling = cooling;
                GameObject marker = Box("CoolingMarker", tower.transform, new Vector3(0f, 1.05f, 0f), Vector3.one * 0.15f, blue);
                marker.SetActive(false);
                cooling.blueMarker = marker;
                LineRenderer line = new GameObject("CoolingAirflow").AddComponent<LineRenderer>();
                line.transform.SetParent(tower.transform, false);
                line.sharedMaterial = blue;
                line.useWorldSpace = true;
                line.positionCount = 2;
                line.startWidth = line.endWidth = 0.025f;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.enabled = false;
                cooling.airflow = line;
                PrefabUtility.SaveAsPrefabAsset(tower, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(tower); }
        }
        TowerTargeting fan = CreateFan(blue).GetComponent<TowerTargeting>();
        ValidateBoost(fan.gameObject);
        const string scenePath = "Assets/Scenes/RoomScaleDefense.unity";
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            bool changed = false;
            foreach (TowerBuildSlot slot in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TowerBuildSlot>(true)))
            {
                if (slot.towerPrefabs.Length >= 4 && slot.towerPrefabs[3] == fan) continue;
                slot.towerPrefabs = new[] { slot.towerPrefabs[0], slot.towerPrefabs[1], slot.towerPrefabs[2], fan };
                changed = true;
            }
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Cooling fan ready: key 4, 1.5m range, +25% speed, blue preview and airflow links, strongest fan only.");
            }
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static GameObject CreateFan(Material blue)
    {
        const string path = "Assets/RoomCoolingFan.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;
        GameObject root = new GameObject("RoomCoolingFan");
        try
        {
            TowerTargeting targeting = root.AddComponent<TowerTargeting>();
            targeting.range = 1.5f;
            targeting.enabled = false;
            CoolingFan fan = root.AddComponent<CoolingFan>();
            fan.targeting = targeting;
            TowerRangeIndicator range = root.AddComponent<TowerRangeIndicator>();
            range.targeting = targeting;
            range.lineMaterial = blue;
            range.showRange = false;
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.85f, 1.4f, 0.85f);
            Transform visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            Box("Stand", visual, new Vector3(0f, -0.25f, 0f), new Vector3(0.25f, 0.9f, 0.3f), blue);
            Transform rotor = new GameObject("Rotor").transform;
            rotor.SetParent(visual, false);
            rotor.localPosition = new Vector3(0f, 0.35f, 0f);
            Box("HorizontalBlade", rotor, Vector3.zero, new Vector3(0.9f, 0.12f, 0.1f), blue);
            Box("VerticalBlade", rotor, Vector3.zero, new Vector3(0.12f, 0.9f, 0.1f), blue);
            fan.blades = rotor;
            TextMeshPro label = new GameObject("FanStatus").AddComponent<TextMeshPro>();
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = Vector3.up * 1.8f;
            label.rectTransform.sizeDelta = new Vector2(3.5f, 1.5f);
            label.fontSize = 1.6f;
            label.alignment = TextAlignmentOptions.Center;
            label.gameObject.SetActive(false);
            fan.label = label;
            return PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = position;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(box.GetComponent<Collider>());
        return box;
    }

    private static void ValidateBoost(GameObject prefab)
    {
        Scene preview = EditorSceneManager.NewPreviewScene();
        try
        {
            GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(prefab, preview);
            CoolingFan fan = root.GetComponent<CoolingFan>();
            root.transform.position = Vector3.zero;
            if (!fan.Covers(new Vector3(1f, 0f, 1f)) || fan.Covers(new Vector3(2f, 0f, 0f)))
                throw new System.InvalidOperationException("Fan neighbor range check failed.");
            TowerShooting shot = root.AddComponent<TowerShooting>();
            TowerCooling receiver = root.AddComponent<TowerCooling>();
            shot.cooling = receiver;
            shot.fireInterval = 0.6f;
            receiver.speedMultiplier = 1.25f;
            if (!Mathf.Approximately(shot.EffectiveInterval, 0.48f))
                throw new System.InvalidOperationException("Fan interval calculation failed.");
            shot.fireInterval *= 0.8f;
            receiver.speedMultiplier = 1f;
            if (!Mathf.Approximately(shot.EffectiveInterval, 0.48f))
                throw new System.InvalidOperationException("Removing boost changed upgrade.");
            Debug.Log("Fan checks passed: diagonal neighbors, range exclusion, boosted interval, upgraded base preserved.");
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }
    }
}
