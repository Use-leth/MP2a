using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupDumpsterModel
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Use Dumpster Model")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        SetupVRWallWorkshop.Setup();
        const string path = "Assets/Scenes/RoomScaleDefense.unity";
        Scene scene = SceneManager.GetSceneByPath(path);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            VRRecycleBin bin = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<VRRecycleBin>(true)).Single();
            Transform root = bin.transform.parent;
            if (root.Find("DumpsterVisual") != null) return;
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DuksGames/VTDumpster/URP/Prefab/VTDumpster_01am_green.prefab");
            GameObject model = Object.Instantiate(asset, root);
            model.name = "DumpsterVisual";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            foreach (Collider collider in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            foreach (Animator animator in model.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
            foreach (Animation animation in model.GetComponentsInChildren<Animation>(true)) animation.enabled = false;
            // Sample each supplied opening animation, retaining each lid's final pose.
            AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath("Assets/DuksGames/VTDumpster/URP/Models/VTDumpster_01am.fbx").OfType<AnimationClip>().ToArray();
            Transform[] parts = model.GetComponentsInChildren<Transform>(true);
            var poses = new System.Collections.Generic.Dictionary<Transform, Quaternion>();
            foreach (string side in new[] { "L", "R" })
            {
                AnimationClip clip = clips.FirstOrDefault(c => c.name.EndsWith("Lid.Open." + side));
                if (clip == null) throw new System.InvalidOperationException("Dumpster opening animation missing: " + side);
                clip.SampleAnimation(model, clip.length);
                foreach (Transform part in parts.Where(t => t.name == "Lid." + side)) poses[part] = part.localRotation;
            }
            foreach (var pose in poses) pose.Key.localRotation = pose.Value;
            Renderer exterior = model.GetComponentsInChildren<Renderer>(true).First(r => r.name == "Exterior");
            model.transform.localScale *= 1.8f / exterior.bounds.size.x;
            Bounds bounds = exterior.bounds;
            model.transform.position += new Vector3(root.position.x - bounds.center.x, root.position.y - bounds.min.y + 0.12f, root.position.z - bounds.center.z);
            bounds = exterior.bounds;
            float width = bounds.size.x, depth = bounds.size.z, height = bounds.size.y;
            // Reuse old wall colliders, but hide their placeholder meshes.
            Fit(root.Find("Bottom"), new Vector3(0f, 0.13f, 0f), new Vector3(width, 0.08f, depth));
            Fit(root.Find("Left"), new Vector3(-width * 0.5f + 0.04f, height * 0.5f + 0.12f, 0f), new Vector3(0.08f, height, depth));
            Fit(root.Find("Right"), new Vector3(width * 0.5f - 0.04f, height * 0.5f + 0.12f, 0f), new Vector3(0.08f, height, depth));
            Fit(root.Find("Front"), new Vector3(0f, height * 0.5f + 0.12f, -depth * 0.5f + 0.04f), new Vector3(width, height, 0.08f));
            Fit(root.Find("Back"), new Vector3(0f, height * 0.5f + 0.12f, depth * 0.5f - 0.04f), new Vector3(width, height, 0.08f));
            bin.transform.localPosition = new Vector3(0f, height * 0.65f + 0.12f, 0f);
            bin.GetComponent<BoxCollider>().size = new Vector3(width - 0.2f, height * 0.6f, depth - 0.2f);
            if (bin.label != null) bin.label.transform.localPosition = new Vector3(0f, height + 0.65f, -depth * 0.5f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Green dumpster installed: open lids, hollow wall colliders, original release-to-recycle logic retained.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void Fit(Transform wall, Vector3 position, Vector3 size)
    {
        wall.localPosition = position;
        wall.localScale = size;
        wall.GetComponent<Renderer>().enabled = false;
    }
}
