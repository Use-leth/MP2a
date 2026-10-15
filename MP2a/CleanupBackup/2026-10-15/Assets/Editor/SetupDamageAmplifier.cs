using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupDamageAmplifier
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Damage Amplifier")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        SetupCoolingFan.Setup();
        Material yellow = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ProjectileYellow.mat");
        foreach (string name in new[] { "RoomRapidTower", "RoomLongRangeTower", "RoomSodaTower" })
        {
            string path = "Assets/" + name + ".prefab";
            GameObject tower = PrefabUtility.LoadPrefabContents(path);
            try
            {
                TowerDamageBoost boost = tower.GetComponent<TowerDamageBoost>();
                if (boost != null && boost.powerLink != null) continue;
                if (boost == null) boost = tower.AddComponent<TowerDamageBoost>();
                TowerShooting shooting = tower.GetComponent<TowerShooting>();
                if (shooting != null) shooting.damageBoost = boost;
                SodaTower soda = tower.GetComponent<SodaTower>();
                if (soda != null) soda.damageBoost = boost;
                if (boost.yellowMarker == null)
                    boost.yellowMarker = Box("DamageMarker", tower.transform, new Vector3(0.25f, 1.05f, 0f), Vector3.one * 0.15f, yellow);
                boost.yellowMarker.SetActive(false);
                LineRenderer link = new GameObject("PowerLink").AddComponent<LineRenderer>();
                link.transform.SetParent(tower.transform, false);
                link.sharedMaterial = yellow;
                link.useWorldSpace = true;
                link.positionCount = 2;
                link.startWidth = link.endWidth = 0.025f;
                link.enabled = false;
                boost.powerLink = link;
                PrefabUtility.SaveAsPrefabAsset(tower, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(tower); }
        }
        TowerTargeting amplifier = CreateAmplifier(yellow).GetComponent<TowerTargeting>();
        Validate(amplifier.gameObject);
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
                if (slot.towerPrefabs.Length >= 5 && slot.towerPrefabs[4] == amplifier) continue;
                slot.towerPrefabs = new[] { slot.towerPrefabs[0], slot.towerPrefabs[1], slot.towerPrefabs[2], slot.towerPrefabs[3], amplifier };
                changed = true;
            }
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Damage amplifier ready: key 5, charge 6s, boost 4s, +50% damage. Strongest only; stacks with fan speed.");
            }
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static GameObject CreateAmplifier(Material yellow)
    {
        const string path = "Assets/RoomDamageAmplifier.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;
        GameObject root = new GameObject("RoomDamageAmplifier");
        try
        {
            TowerTargeting targeting = root.AddComponent<TowerTargeting>();
            targeting.range = 1.5f;
            targeting.enabled = false;
            DamageAmplifier amplifier = root.AddComponent<DamageAmplifier>();
            amplifier.targeting = targeting;
            root.AddComponent<BoxCollider>().size = new Vector3(0.85f, 1.4f, 0.85f);
            TowerRangeIndicator range = root.AddComponent<TowerRangeIndicator>();
            range.targeting = targeting;
            range.lineMaterial = yellow;
            range.showRange = false;
            Transform visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            Material body = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TableFrame.mat");
            Box("BatteryBody", visual, Vector3.zero, new Vector3(0.75f, 1.1f, 0.7f), body);
            Box("Terminal", visual, Vector3.up * 0.65f, new Vector3(0.3f, 0.2f, 0.3f), yellow);
            amplifier.chargeBar = Box("ChargeBar", visual, new Vector3(0f, -0.5f, -0.43f), new Vector3(0.18f, 0.01f, 0.18f), yellow).transform;
            LineRenderer pulse = new GameObject("ChargePulse").AddComponent<LineRenderer>();
            pulse.transform.SetParent(root.transform, false);
            pulse.transform.localPosition = Vector3.down * 0.6f;
            pulse.sharedMaterial = yellow;
            pulse.useWorldSpace = false;
            pulse.loop = true;
            pulse.positionCount = 48;
            pulse.startWidth = pulse.endWidth = 0.045f;
            for (int i = 0; i < 48; i++)
            {
                float angle = i * Mathf.PI * 2f / 48f;
                pulse.SetPosition(i, new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
            }
            pulse.enabled = false;
            amplifier.pulse = pulse;
            TextMeshPro label = new GameObject("AmplifierStatus").AddComponent<TextMeshPro>();
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = Vector3.up * 1.8f;
            label.rectTransform.sizeDelta = new Vector2(3.5f, 1.5f);
            label.fontSize = 1.6f;
            label.alignment = TextAlignmentOptions.Center;
            label.gameObject.SetActive(false);
            amplifier.label = label;
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

    private static void Validate(GameObject prefab)
    {
        Scene preview = EditorSceneManager.NewPreviewScene();
        try
        {
            GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(prefab, preview);
            DamageAmplifier amplifier = root.GetComponent<DamageAmplifier>();
            root.transform.position = Vector3.zero;
            TowerDamageBoost receiver = root.AddComponent<TowerDamageBoost>();
            amplifier.AdvanceCharge(10f);
            if (amplifier.charge != 0f || amplifier.activeSeconds != 0f)
                throw new System.InvalidOperationException("Empty amplifier should wait.");
            amplifier.connectedTowers = 3;
            if (amplifier.RequiredChargeSeconds != 10f) throw new System.InvalidOperationException("Amplifier load calculation failed.");
            amplifier.connectedTowers = 2;
            amplifier.AdvanceCharge(6f);
            if (amplifier.activeSeconds != 0f) throw new System.InvalidOperationException("Two towers should require eight seconds.");
            amplifier.AdvanceCharge(2f);
            if (amplifier.activeSeconds != 4f) throw new System.InvalidOperationException("Loaded amplifier did not fire.");
            amplifier.AdvanceCharge(4f);
            amplifier.connectedTowers = 1;
            amplifier.AdvanceCharge(5f);
            if (amplifier.activeSeconds != 0f) throw new System.InvalidOperationException("Amplifier fired early.");
            amplifier.AdvanceCharge(1f);
            receiver.Consider(amplifier);
            if (!Mathf.Approximately(receiver.Multiplier, 1.5f) || !Mathf.Approximately(receiver.Remaining, 4f))
                throw new System.InvalidOperationException("Amplifier charge or damage boost failed.");
            if (amplifier.Covers(Vector3.right * 2f)) throw new System.InvalidOperationException("Amplifier range failed.");
            amplifier.AdvanceCharge(4f);
            if (receiver.Multiplier != 1f) throw new System.InvalidOperationException("Amplifier boost did not expire.");
            Debug.Log("Amplifier checks passed: charge threshold, multiplier, duration, range, expiry.");
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }
    }
}
