using UnityEditor;
using UnityEngine;

public static class SetupScrapMachineModels
{
    private const string Bottles = "Assets/URP_WasteOvergrowth_SA/Prefabs/Bottles/";

    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Assemble Scrap Machine Models")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Assemble("Assets/RoomLongRangeTower.prefab", false);
        Assemble("Assets/RoomDamageAmplifier.prefab", true);
    }

    private static void Assemble(string path, bool battery)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            if (root.transform.Find("ScrapMachineVisual") != null) return;
            Transform visual = new GameObject("ScrapMachineVisual").transform;
            visual.SetParent(root.transform, false);
            Material metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TableFrame.mat");
            Material yellow = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ProjectileYellow.mat");
            if (battery)
            {
                Model("Prefab_SodaCan_1", visual, new Vector3(-0.22f, 0f, 0f), new Vector3(0.4f, 1f, 0.5f), Quaternion.identity);
                Model("Prefab_SodaCan_2", visual, new Vector3(0.22f, 0f, 0f), new Vector3(0.4f, 1f, 0.5f), Quaternion.identity);
                Box("BottomBracket", visual, new Vector3(0f, -0.53f, 0f), new Vector3(0.9f, 0.12f, 0.6f), metal);
                Box("TopBridge", visual, new Vector3(0f, 0.53f, 0f), new Vector3(0.7f, 0.08f, 0.3f), metal);
                Box("PositiveTerminal", visual, new Vector3(-0.22f, 0.65f, 0f), new Vector3(0.13f, 0.2f, 0.13f), yellow);
                Box("NegativeTerminal", visual, new Vector3(0.22f, 0.65f, 0f), new Vector3(0.13f, 0.2f, 0.13f), yellow);
                Transform old = root.transform.Find("Visual");
                if (old != null)
                {
                    Remove(old.Find("BatteryBody"));
                    Remove(old.Find("Terminal"));
                }
                // Existing charge bar and pulse keep their script references and coordinates.
            }
            else
            {
                Model("Prefab_SodaCan_2", visual, new Vector3(0f, -0.2f, 0f), new Vector3(0.65f, 0.7f, 0.65f), Quaternion.identity);
                Model("Prefab_BeerBottle", visual, new Vector3(0f, 0.4f, 0f), new Vector3(0.44f, 0.44f, 1.05f), Quaternion.Euler(90f, 0f, 0f));
                Box("Mount", visual, new Vector3(0f, 0.12f, 0f), new Vector3(0.5f, 0.12f, 0.5f), metal);
                Box("Foot", visual, new Vector3(0f, -0.59f, 0f), new Vector3(0.8f, 0.08f, 0.8f), metal);
                Renderer oldBody = root.GetComponent<MeshRenderer>();
                if (oldBody != null) oldBody.enabled = false;
                Remove(root.transform.Find("TurretTop"));
                TowerShooting shot = root.GetComponent<TowerShooting>();
                shot.firePoint.localPosition = new Vector3(0f, 0.4f, 0.58f);
                // Preserve the existing upgrade panel, labels, muzzle effect, and boost markers.
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Debug.Log("Assembled recycled machine appearance: " + path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    public static void Model(string name, Transform parent, Vector3 center, Vector3 size, Quaternion rotation)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Bottles + name + ".prefab");
        if (prefab == null) throw new System.InvalidOperationException("Missing model: " + name);
        Transform holder = new GameObject(name + "Mount").transform;
        holder.SetParent(parent, false);
        GameObject model = Object.Instantiate(prefab, holder);
        model.name = name;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = rotation;
        foreach (Collider collider in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
        foreach (Rigidbody body in model.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(body);
        foreach (MonoBehaviour script in model.GetComponentsInChildren<MonoBehaviour>(true)) script.enabled = false;
        Bounds bounds = new Bounds();
        bool first = true;
        foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null) continue;
            Bounds local = filter.sharedMesh.bounds;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 point = holder.InverseTransformPoint(filter.transform.TransformPoint(local.center
                            + Vector3.Scale(local.extents, new Vector3(x, y, z))));
                        if (first) { bounds = new Bounds(point, Vector3.zero); first = false; }
                        else bounds.Encapsulate(point);
                    }
        }
        if (first) throw new System.InvalidOperationException("Model has no mesh: " + name);
        float scale = Mathf.Min(size.x / Mathf.Max(0.001f, bounds.size.x),
            size.y / Mathf.Max(0.001f, bounds.size.y), size.z / Mathf.Max(0.001f, bounds.size.z));
        holder.localScale = Vector3.one * scale;
        model.transform.localPosition -= bounds.center;
        holder.localPosition = center;
    }

    private static void Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = position;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(box.GetComponent<Collider>());
    }

    private static void Remove(Transform part)
    {
        if (part != null) Object.DestroyImmediate(part.gameObject);
    }
}
