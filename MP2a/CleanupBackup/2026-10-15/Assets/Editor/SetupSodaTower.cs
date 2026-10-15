using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupSodaTower
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Soda Tower")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        TowerTargeting prefab = CreatePrefab().GetComponent<TowerTargeting>();
        ValidateSoda(prefab.gameObject);
        const string enemyPath = "Assets/RoomScaleEnemy.prefab";
        GameObject enemy = PrefabUtility.LoadPrefabContents(enemyPath);
        try
        {
            if (enemy.GetComponent<EnemyHitFlash>() == null)
            {
                enemy.AddComponent<EnemyHitFlash>().body = enemy.GetComponent<Renderer>();
                PrefabUtility.SaveAsPrefabAsset(enemy, enemyPath);
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(enemy); }
        const string path = "Assets/Scenes/RoomScaleDefense.unity";
        Scene scene = SceneManager.GetSceneByPath(path);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            bool changed = false;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == "DismantlingCorner")
                {
                    Object.DestroyImmediate(root);
                    changed = true;
                    continue;
                }
                foreach (TowerBuildSlot slot in root.GetComponentsInChildren<TowerBuildSlot>(true))
                {
                    if (slot.towerPrefabs.Length < 3 || slot.towerPrefabs[2] != prefab)
                    {
                        slot.towerPrefabs = new[] { slot.towerPrefabs[0], slot.towerPrefabs[1], prefab };
                        changed = true;
                    }
                }
            }
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Soda tower ready: key 3, R rotates preview or built tower, E upgrades, 60-degree area damage.");
            }
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static GameObject CreatePrefab()
    {
        const string path = "Assets/RoomSodaTower.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;
        GameObject root = new GameObject("RoomSodaTower");
        try
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TowerBlue.mat");
            TowerTargeting targeting = root.AddComponent<TowerTargeting>();
            targeting.range = 3f;
            targeting.enabled = false; // Soda uses its own cone scan, not nearest-target selection.
            SodaTower soda = root.AddComponent<SodaTower>();
            soda.targeting = targeting;
            TowerRangeIndicator range = root.AddComponent<TowerRangeIndicator>();
            range.targeting = targeting;
            range.coneAngle = soda.coneAngle;
            range.showRange = false;
            range.lineMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ProjectileYellow.mat");
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.8f, 1.4f, 0.8f);
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = new Vector3(0.8f, 0.7f, 0.8f);
            visual.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            GameObject nozzle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nozzle.name = "NozzleVisual";
            nozzle.transform.SetParent(root.transform, false);
            nozzle.transform.localPosition = new Vector3(0f, 0.1f, 0.5f);
            nozzle.transform.localScale = new Vector3(0.3f, 0.25f, 0.6f);
            nozzle.GetComponent<Renderer>().sharedMaterial = range.lineMaterial;
            Object.DestroyImmediate(nozzle.GetComponent<Collider>());

            ParticleSystem spray = new GameObject("SprayOrigin").AddComponent<ParticleSystem>();
            spray.transform.SetParent(root.transform, false);
            spray.transform.localPosition = new Vector3(0f, -0.35f, 0f);
            spray.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = spray.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startLifetime = 0.5f;
            main.startSpeed = 6f;
            main.startSize = 0.09f;
            main.startColor = Color.cyan;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 150;
            var emission = spray.emission;
            emission.rateOverTime = 100f;
            var shape = spray.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = 0.02f;
            var renderer = spray.GetComponent<ParticleSystemRenderer>();
            const string sprayMaterialPath = "Assets/Materials/SodaSpray.mat";
            Material sprayMaterial = AssetDatabase.LoadAssetAtPath<Material>(sprayMaterialPath);
            if (sprayMaterial == null)
            {
                sprayMaterial = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                sprayMaterial.SetColor("_BaseColor", Color.cyan);
                AssetDatabase.CreateAsset(sprayMaterial, sprayMaterialPath);
            }
            renderer.sharedMaterial = sprayMaterial;
            soda.spray = spray;
            TextMeshPro label = new GameObject("SodaStatus").AddComponent<TextMeshPro>();
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = Vector3.up * 1.8f;
            label.rectTransform.sizeDelta = new Vector2(3.5f, 1.5f);
            label.fontSize = 1.6f;
            label.alignment = TextAlignmentOptions.Center;
            label.gameObject.SetActive(false);
            soda.label = label;
            return PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static void ValidateSoda(GameObject prefab)
    {
        Scene preview = EditorSceneManager.NewPreviewScene();
        try
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, preview);
            SodaTower soda = instance.GetComponent<SodaTower>();
            instance.transform.position = Vector3.zero;
            soda.SetFacing(0f);
            Require(soda.ContainsPoint(new Vector3(0f, 0f, 2f)), "Forward point rejected");
            Require(!soda.ContainsPoint(new Vector3(0f, 0f, -2f)), "Rear point accepted");
            Require(!soda.ContainsPoint(new Vector3(0f, 0f, 3.1f)), "Out-of-range point accepted");
            Require(!soda.ContainsPoint(Quaternion.Euler(0f, 31f, 0f) * Vector3.forward * 2f), "Outside cone accepted");
            Require(soda.ContainsPoint(Quaternion.Euler(0f, 29f, 0f) * Vector3.forward * 2f), "Inside cone rejected");
            soda.RotateQuarterTurn();
            Require(soda.ContainsPoint(Vector3.right * 2f) && !soda.ContainsPoint(Vector3.forward * 2f), "Rotation did not move cone");
            PlayerResources resources = instance.AddComponent<PlayerResources>();
            soda.resources = resources;
            resources.gold = 14;
            soda.Upgrade();
            Require(resources.gold == 14 && soda.level == 0, "Insufficient balance changed state");
            resources.gold = 45;
            soda.Upgrade();
            soda.Upgrade();
            soda.Upgrade();
            Require(resources.gold == 0 && soda.level == 2 && Mathf.Approximately(soda.damagePerSecond, 6f), "Upgrade costs or cap incorrect");
            Debug.Log("Soda checks passed: range, cone edges, rotation, insufficient funds, two upgrades and max level.");
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.InvalidOperationException("Soda validation: " + message);
    }
}
