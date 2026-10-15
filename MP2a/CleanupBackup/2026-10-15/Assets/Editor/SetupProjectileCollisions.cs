using UnityEditor;
using UnityEngine;

public static class SetupProjectileCollisions
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }

    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Projectile Collisions")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        foreach (string name in new[] { "Projectile", "CapProjectile", "PunchProjectile", "RoomScaleEnemy" })
        {
            string path = "Assets/" + name + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                bool changed = false;
                if (root.GetComponent<EnemyHealth>() != null)
                {
                    if (root.GetComponent<Collider>() == null)
                    {
                        // Trigger only: rats remain free to follow their existing path.
                        root.AddComponent<BoxCollider>().isTrigger = true;
                        changed = true;
                    }
                }
                else
                {
                    SphereCollider collider = root.GetComponent<SphereCollider>();
                    if (collider == null) { collider = root.AddComponent<SphereCollider>(); changed = true; }
                    if (!collider.isTrigger) { collider.isTrigger = true; changed = true; }
                    Rigidbody body = root.GetComponent<Rigidbody>();
                    if (body == null) { body = root.AddComponent<Rigidbody>(); changed = true; }
                    if (body.useGravity || !body.isKinematic || body.interpolation != RigidbodyInterpolation.Interpolate)
                    {
                        body.useGravity = false;
                        body.isKinematic = true;
                        body.interpolation = RigidbodyInterpolation.Interpolate;
                        changed = true;
                    }
                }
                if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
