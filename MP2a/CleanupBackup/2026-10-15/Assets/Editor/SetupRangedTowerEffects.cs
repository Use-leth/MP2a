using UnityEditor;
using UnityEngine;

public static class SetupRangedTowerEffects
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }

    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Ranged Tower Effects")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        SetupTower("Assets/RoomRapidTower.prefab", "Cap", new Color(1f, 0.85f, 0.15f), false);
        SetupTower("Assets/RoomLongRangeTower.prefab", "Punch", new Color(1f, 0.35f, 0.08f), true);
    }

    private static void SetupTower(string path, string kind, Color color, bool heavy)
    {
        GameObject tower = PrefabUtility.LoadPrefabContents(path);
        try
        {
            TowerShooting shooting = tower.GetComponent<TowerShooting>();
            if (shooting.muzzleFlash != null) return;
            Material material = EffectMaterial(kind, color);
            ParticleSystem impact = ImpactPrefab(kind, material, heavy);
            shooting.projectilePrefab = ProjectilePrefab(kind, shooting.projectilePrefab, material, impact, heavy);
            ParticleSystem flash = MakeParticles("MuzzleFlash", material, heavy ? 0.18f : 0.09f,
                heavy ? 0.2f : 0.1f, heavy ? 1.7f : 2.5f);
            flash.transform.SetParent(shooting.firePoint, false);
            var shape = flash.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = heavy ? 30f : 15f;
            shape.radius = 0.035f;
            shooting.muzzleFlash = flash;
            shooting.flashParticleCount = heavy ? 14 : 6;
            PrefabUtility.SaveAsPrefabAsset(tower, path);
            Debug.Log(kind + " effects ready: firing flash, projectile trail, impact burst.");
        }
        finally { PrefabUtility.UnloadPrefabContents(tower); }
    }

    private static Material EffectMaterial(string kind, Color color)
    {
        string path = "Assets/Materials/" + kind + "Effects.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        material.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static ParticleSystem MakeParticles(string name, Material material, float lifetime, float size, float speed)
    {
        ParticleSystem particles = new GameObject(name).AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.3f;
        main.startLifetime = lifetime;
        main.startSize = size;
        main.startSpeed = speed;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        var emission = particles.emission;
        emission.rateOverTime = 0f;
        var sizeOverLife = particles.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
        particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
        return particles;
    }

    private static ParticleSystem ImpactPrefab(string kind, Material material, bool heavy)
    {
        string path = "Assets/" + kind + "Impact.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing.GetComponent<ParticleSystem>();
        ParticleSystem impact = MakeParticles(kind + "Impact", material, heavy ? 0.35f : 0.2f,
            heavy ? 0.13f : 0.06f, heavy ? 1.5f : 1f);
        try
        {
            var shape = impact.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;
            var emission = impact.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(heavy ? 18 : 10)) });
            return PrefabUtility.SaveAsPrefabAsset(impact.gameObject, path).GetComponent<ParticleSystem>();
        }
        finally { Object.DestroyImmediate(impact.gameObject); }
    }

    private static Projectile ProjectilePrefab(string kind, Projectile source, Material material, ParticleSystem impact, bool heavy)
    {
        string path = "Assets/" + kind + "Projectile.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing.GetComponent<Projectile>();
        GameObject projectile = Object.Instantiate(source.gameObject);
        try
        {
            projectile.name = kind + "Projectile";
            projectile.GetComponent<Projectile>().impactEffect = impact;
            Renderer body = projectile.GetComponent<Renderer>();
            if (body != null) body.sharedMaterial = material;
            TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
            trail.sharedMaterial = material;
            trail.time = heavy ? 0.22f : 0.12f;
            trail.startWidth = heavy ? 0.1f : 0.04f;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.025f;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            return PrefabUtility.SaveAsPrefabAsset(projectile, path).GetComponent<Projectile>();
        }
        finally { Object.DestroyImmediate(projectile); }
    }
}
