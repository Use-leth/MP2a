using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SetupReplacementModels
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Connect Replacement Models")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string fanPath = "Assets/RoomCoolingFan.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(fanPath);
        try
        {
            CoolingFan fan = root.GetComponent<CoolingFan>();
            bool cleanedWind = false;
            foreach (Transform effect in root.GetComponentsInChildren<Transform>(true))
            {
                if (effect.name != "WindParticle" && effect.name != "WindDistortion"
                    && effect.name != "WindDust" && effect.name != "WindSwirl") continue;
                if (effect.gameObject.activeSelf)
                {
                    effect.gameObject.SetActive(false);
                    cleanedWind = true;
                }
                foreach (Collider collider in effect.GetComponentsInChildren<Collider>(true))
                {
                    if (!collider.enabled) continue;
                    collider.enabled = false;
                    cleanedWind = true;
                }
            }
            if (fan.head == null)
            {
                MonoBehaviour demo = root.GetComponentsInChildren<MonoBehaviour>(true)
                    .FirstOrDefault(b => b != null && b.GetType().FullName == "WindTriggerSystem.WindTrigger");
                if (demo == null) throw new System.InvalidOperationException("Replacement fan demo controller not found; old visual was retained.");
                Transform rotor = new SerializedObject(demo).FindProperty("_fanRotation").objectReferenceValue as Transform;
                if (rotor == null) throw new System.InvalidOperationException("Replacement fan rotor missing; old visual was retained.");
                Transform model = rotor.parent;
                // Keep the imported asset intact; edit only this tower's nested instance.
                GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(demo.gameObject);
                if (instanceRoot != null && instanceRoot != root)
                    PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                demo.enabled = false;
                Transform pivot = new GameObject("FanHeadPivot").transform;
                pivot.SetParent(root.transform, false);
                pivot.position = rotor.position;
                Transform[] parts = model.Cast<Transform>().ToArray();
                foreach (Transform part in parts)
                    if (part == rotor || part.name == "Torus.001" || part.name == "Cylinder.002" || part.name == "tranform_orient")
                        part.SetParent(pivot, true);
                fan.blades = rotor;
                fan.bladeAxis = Vector3.up; // Matches the imported demo's rotor axis.
                fan.head = pivot;
                // The supplied wind visual follows the head as it turns.
                Transform wind = demo.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "WindParticle");
                if (wind != null) wind.SetParent(pivot, true);
                Transform old = root.transform.Find("Visual");
                if (old != null) Object.DestroyImmediate(old.gameObject);
                PrefabUtility.SaveAsPrefabAsset(root, fanPath);
                Debug.Log("Fan model connected: imported rotor, head swing, old block model removed.");
            }
            else if (cleanedWind) PrefabUtility.SaveAsPrefabAsset(root, fanPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }

        const string sodaPath = "Assets/RoomSodaTower.prefab";
        root = PrefabUtility.LoadPrefabContents(sodaPath);
        try
        {
            Transform old = root.transform.Find("Cylinder");
            // Remove only the old primitive, after confirming imported replacement meshes exist.
            if (old != null && root.GetComponentsInChildren<MeshFilter>(true)
                .Any(m => AssetDatabase.GetAssetPath(m.sharedMesh).StartsWith("Assets/URP_WasteOvergrowth_SA/")))
            {
                Object.DestroyImmediate(old.gameObject);
                PrefabUtility.SaveAsPrefabAsset(root, sodaPath);
                Debug.Log("Soda replacement retained; old cylinder removed, spray and gameplay references preserved.");
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
