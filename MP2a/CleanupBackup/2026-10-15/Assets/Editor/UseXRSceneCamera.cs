using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UseXRSceneCamera
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }

    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Use XR Camera and Remove Overview")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetSceneByPath("Assets/Scenes/RoomScaleDefense.unity");
        // Work on the loaded scene so unsaved rig edits are preserved.
        if (!scene.isLoaded) return;
        XROrigin[] rigs = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<XROrigin>(true)).ToArray();
        if (rigs.Length != 1 || rigs[0].Camera == null)
        {
            Debug.LogError("XR camera setup needs exactly one XR Origin with its Camera assigned. No cameras were removed.");
            return;
        }
        Camera camera = rigs[0].Camera;
        Camera[] oldCameras = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Camera>(true))
            .Where(c => c != camera && (c.name == "OverviewCamera" || c.name == "StandingCamera_1.65m")).ToArray();
        RoomViewControls[] controls = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<RoomViewControls>(true)).ToArray();
        if (oldCameras.Length == 0 && controls.Length == 0) return;

        for (Transform t = camera.transform; t != null; t = t.parent) t.gameObject.SetActive(true);
        camera.enabled = true;
        camera.tag = "MainCamera";
        camera.targetDisplay = 0;
        camera.targetTexture = null;
        camera.stereoTargetEye = StereoTargetEyeMask.Both;
        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null) listener.enabled = true;
        PrefabUtility.RecordPrefabInstancePropertyModifications(camera);
        PrefabUtility.RecordPrefabInstancePropertyModifications(camera.gameObject);
        foreach (DesktopBuildInput input in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<DesktopBuildInput>(true)))
            input.viewCamera = camera;
        foreach (EnemyHealthBar bar in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<EnemyHealthBar>(true)))
            bar.viewCamera = camera;
        foreach (RoomViewControls control in controls) Object.DestroyImmediate(control);
        foreach (Camera old in oldCameras) Object.DestroyImmediate(old.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("XR camera scene setup saved: Overview and Standing cameras removed; gameplay camera references now use " + camera.name + ".");
    }
}
