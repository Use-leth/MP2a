using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SetupRatAndCombatVisuals
{
    private const string RatFolder = "Assets/Lil Pupinduy/Character/MimicBook/";
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Rat And Combat Visuals")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        SetupRat();
        SetupCapTower();
        foreach (string name in new[] { "RoomRapidTower", "RoomLongRangeTower", "RoomSodaTower", "RoomDamageAmplifier", "RoomCoolingFan" })
        {
            string path = "Assets/" + name + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                MachineFeedback feedback = root.GetComponent<MachineFeedback>();
                if (feedback != null && feedback.effectVersion >= 2) continue;
                if (feedback == null) feedback = root.AddComponent<MachineFeedback>();
                feedback.amplifier = root.GetComponent<DamageAmplifier>();
                feedback.damageBoost = root.GetComponent<TowerDamageBoost>();
                feedback.cooling = root.GetComponent<TowerCooling>();
                feedback.fan = root.GetComponent<CoolingFan>();
                if (feedback.damageBoost != null || feedback.cooling != null)
                    feedback.flashRenderers = root.GetComponentsInChildren<MeshRenderer>(true)
                        .Where(r => r.enabled && r.GetComponent<TMPro.TMP_Text>() == null
                            && r.name != "CoolingMarker" && r.name != "DamageMarker"
                            && r.name != "UpgradeIndicator" && r.name != "SpeedUpgradePanel").Cast<Renderer>().ToArray();
                if (feedback.fan != null && feedback.fanWind == null)
                {
                    feedback.fanWind = Sparks(root.transform, "FanAirflow", Color.white, 0.15f, 0.4f);
                    var windMain = feedback.fanWind.main;
                    windMain.startLifetime = 0.55f;
                    windMain.startSpeed = 2f;
                    windMain.startSize = 0.025f;
                    var windEmission = feedback.fanWind.emission;
                    windEmission.rateOverTime = 28f;
                    var windRenderer = feedback.fanWind.GetComponent<ParticleSystemRenderer>();
                    windRenderer.renderMode = ParticleSystemRenderMode.Stretch;
                    windRenderer.velocityScale = 0.04f;
                    windRenderer.lengthScale = 2.5f;
                }
                if (feedback.amplifier != null && feedback.chargeSparks == null)
                    feedback.chargeSparks = Sparks(root.transform, "ChargeSparks", new Color(1f, 0.72f, 0.08f), 0.22f, 0.55f);
                if (feedback.damageBoost != null && feedback.damageSparks == null)
                    feedback.damageSparks = Sparks(root.transform, "DamageAura", new Color(1f, 0.65f, 0.08f), 0.32f, 0.15f);
                if (feedback.cooling != null && feedback.coolingSparks == null)
                    feedback.coolingSparks = Sparks(root.transform, "CoolingAura", Color.white, 0.1f, 0.05f);
                if (feedback.coolingSparks != null)
                {
                    ParticleSystem particles = feedback.coolingSparks;
                    var main = particles.main;
                    main.startColor = Color.white;
                    main.startLifetime = 0.4f;
                    main.startSize = 0.04f;
                    main.startSpeed = 0.5f;
                    var emission = particles.emission;
                    emission.rateOverTime = 10f;
                    var shape = particles.shape;
                    shape.radius = 0.35f;
                    shape.angle = 8f;
                    var renderer = particles.GetComponent<ParticleSystemRenderer>();
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                    renderer.velocityScale = 0.04f;
                    renderer.lengthScale = 2.5f;
                    renderer.sharedMaterial.SetColor("_BaseColor", Color.white);
                    EditorUtility.SetDirty(renderer.sharedMaterial);
                }
                if (feedback.chargeSparks != null)
                {
                    var main = feedback.chargeSparks.main;
                    main.startSize = 0.055f;
                    main.startSpeed = 0.8f;
                    main.maxParticles = 100;
                    if (feedback.chargeArc == null)
                    {
                        LineRenderer arc = new GameObject("TerminalElectricArc").AddComponent<LineRenderer>();
                        arc.transform.SetParent(root.transform, false);
                        arc.useWorldSpace = false;
                        arc.positionCount = 7;
                        arc.sharedMaterial = feedback.chargeSparks.GetComponent<ParticleSystemRenderer>().sharedMaterial;
                        arc.enabled = false;
                        feedback.chargeArc = arc;
                    }
                }
                feedback.effectVersion = 2;
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Rat, cap launcher and combat feedback setup complete.");
    }

    private static void SetupRat()
    {
        const string path = "Assets/RoomScaleEnemy.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            if (root.GetComponent<RatMovementVisual>() != null) return;
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(RatFolder + "Prefabs/Rat_V1.prefab");
            if (asset == null) throw new System.InvalidOperationException("Rat_V1 asset is missing.");
            Transform pivot = new GameObject("RatVisual").transform;
            pivot.SetParent(root.transform, false);
            GameObject rat = Object.Instantiate(asset, pivot);
            rat.name = "RatModel";
            foreach (Collider collider in rat.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            foreach (Rigidbody body in rat.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(body);
            foreach (MonoBehaviour script in rat.GetComponentsInChildren<MonoBehaviour>(true)) script.enabled = false;
            Renderer[] renderers = rat.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
            rat.transform.localScale *= 0.8f / Mathf.Max(bounds.size.x, bounds.size.z);
            bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
            rat.transform.position += root.transform.position - new Vector3(bounds.center.x, bounds.min.y + 0.21f, bounds.center.z);
            Renderer old = root.GetComponent<MeshRenderer>();
            if (old != null) old.enabled = false;
            Animator animator = rat.GetComponentInChildren<Animator>();
            const string controllerPath = "Assets/RatRun.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                const string runPath = "Assets/RatRunLoop.anim";
                AnimationClip run = AssetDatabase.LoadAssetAtPath<AnimationClip>(runPath);
                if (run == null)
                {
                    if (!AssetDatabase.CopyAsset(RatFolder + "AnimationClips/Run.anim", runPath))
                        throw new System.InvalidOperationException("Rat run clip missing.");
                    run = AssetDatabase.LoadAssetAtPath<AnimationClip>(runPath);
                    var settings = AnimationUtility.GetAnimationClipSettings(run);
                    settings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(run, settings);
                    EditorUtility.SetDirty(run);
                }
                AnimatorState state = controller.layers[0].stateMachine.AddState("Run");
                state.motion = run;
                controller.layers[0].stateMachine.defaultState = state;
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
            }
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            RatMovementVisual motion = root.AddComponent<RatMovementVisual>();
            motion.visual = pivot;
            motion.animator = animator;
            EnemyHitFlash flash = root.GetComponent<EnemyHitFlash>();
            flash.body = renderers.OfType<SkinnedMeshRenderer>().FirstOrDefault() ?? renderers[0];
            BoxCollider hit = root.GetComponent<BoxCollider>();
            // Keep the existing trigger collider; fit its volume to the new rat.
            hit.size = new Vector3(0.7f, 0.4f, 0.7f) / root.transform.localScale.x;
            hit.center = new Vector3(0f, -0.02f, 0f);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void SetupCapTower()
    {
        const string path = "Assets/RoomRapidTower.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            if (root.transform.Find("CapLauncherVisual") != null) return;
            // Replace only the three imported decorative bottle/can children.
            foreach (Transform child in root.transform.Cast<Transform>().ToArray())
            {
                if (child.GetComponentsInChildren<MeshFilter>(true).Any(m =>
                    AssetDatabase.GetAssetPath(m.sharedMesh).StartsWith("Assets/URP_WasteOvergrowth_SA/")))
                    Object.DestroyImmediate(child.gameObject);
            }
            Renderer oldBody = root.GetComponent<MeshRenderer>();
            if (oldBody != null) oldBody.enabled = false;
            Transform visual = new GameObject("CapLauncherVisual").transform;
            visual.SetParent(root.transform, false);
            SetupScrapMachineModels.Model("Prefab_SodaCan_1", visual, new Vector3(0f, -0.12f, 0f), new Vector3(0.7f, 0.65f, 0.65f), Quaternion.identity);
            for (int i = -1; i <= 1; i += 2)
                SetupScrapMachineModels.Model("Prefab_WaterBottle_S", visual, new Vector3(i * 0.2f, 0.35f, 0.08f), new Vector3(0.28f, 0.28f, 0.8f), Quaternion.Euler(90f, 0f, 0f));
            for (int i = 0; i < 5; i++)
                SetupScrapMachineModels.Model("Prefab_BotteCap", visual, new Vector3(-0.42f, -0.2f + i * 0.08f, 0f), new Vector3(0.22f, 0.07f, 0.22f), Quaternion.identity);
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "UpgradeIndicator";
            panel.transform.SetParent(visual, false);
            panel.transform.localPosition = new Vector3(0f, -0.15f, -0.36f);
            panel.transform.localScale = new Vector3(0.35f, 0.13f, 0.04f);
            Object.DestroyImmediate(panel.GetComponent<Collider>());
            panel.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ProjectileYellow.mat");
            root.GetComponent<TowerSpeedUpgrade>().upgradePanel = panel.GetComponent<Renderer>();
            root.GetComponent<TowerShooting>().firePoint.localPosition = new Vector3(0f, 0.35f, 0.52f);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static ParticleSystem Sparks(Transform parent, string name, Color color, float radius, float height)
    {
        string materialPath = "Assets/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        ParticleSystem particles = new GameObject(name).AddComponent<ParticleSystem>();
        particles.transform.SetParent(parent, false);
        particles.transform.localPosition = Vector3.up * height;
        particles.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = 0.6f;
        main.startSpeed = 0.5f;
        main.startSize = 0.035f;
        main.startColor = color;
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        var emission = particles.emission;
        emission.rateOverTime = 9f;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = radius;
        shape.angle = 8f;
        var size = particles.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
        particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
        return particles;
    }
}
