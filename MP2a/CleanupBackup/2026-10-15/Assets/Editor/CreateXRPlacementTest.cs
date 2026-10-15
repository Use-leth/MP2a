using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Editor-only setup for this one test scene. No runtime setup system is needed.
public static class CreateXRPlacementTest
{
    private const string ScenePath = "Assets/Scenes/XRPlacementTest.unity";
    private const string RigPath = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

    [InitializeOnLoadMethod]
    private static void PrepareAfterImport()
    {
        EditorApplication.update += TryCreate;
    }

    private static void TryCreate()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EditorApplication.update -= TryCreate;
        if (!File.Exists(ScenePath))
            CreateScene();
    }

    [MenuItem("Tools/MP2a/Create XR Placement Test")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Exit Play mode before creating the XR test.");
            return;
        }

        if (File.Exists(ScenePath))
        {
            Debug.Log("XRPlacementTest already exists; no changes made.");
            return;
        }

        GameObject rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigPath);
        if (rigPrefab == null)
        {
            Debug.LogError("Import XR Starter Assets before creating the test.");
            return;
        }

        Scene previousScene = SceneManager.GetActiveScene();
        Scene testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(testScene);
        try
        {
            Material gray = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TabletopGray.mat");
            Material blue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TowerBlue.mat");
            Material green = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BaseGreen.mat");

            new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab, testScene);
            rig.transform.position = Vector3.zero;
            Transform locomotion = rig.transform.Find("Locomotion");
            if (locomotion != null)
                locomotion.gameObject.SetActive(false);

            Light light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

            Cube("Floor", new Vector3(0f, -0.05f, 0f), new Vector3(4f, 0.1f, 4f), gray);
            Cube("Tabletop", new Vector3(0f, 0.75f, 0.65f), new Vector3(1.2f, 0.1f, 0.65f), gray);

            GameObject tower = Cube("GrabTower", new Vector3(-0.25f, 0.9f, 0.55f), new Vector3(0.12f, 0.2f, 0.12f), blue);
            Rigidbody body = tower.AddComponent<Rigidbody>();
            body.mass = 0.2f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            XRGrabInteractable grab = tower.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.Kinematic;
            grab.throwOnDetach = false;
            grab.useDynamicAttach = false;

            Cube("SocketMarker", new Vector3(0.25f, 0.81f, 0.55f), new Vector3(0.24f, 0.02f, 0.24f), green);
            GameObject socketObject = new GameObject("TowerSocket");
            socketObject.transform.position = new Vector3(0.25f, 0.92f, 0.55f);
            BoxCollider trigger = socketObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(0.22f, 0.24f, 0.22f);
            XRSocketInteractor socket = socketObject.AddComponent<XRSocketInteractor>();
            socket.attachTransform = socketObject.transform;
            socket.showInteractableHoverMeshes = false;
            socket.hoverSocketSnapping = false;

            foreach (GameObject root in testScene.GetRootGameObjects())
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) > 0)
                        throw new System.InvalidOperationException("Missing script on " + child.name);
                }
            }

            if (!EditorSceneManager.SaveScene(testScene, ScenePath))
                throw new System.InvalidOperationException("Could not save XRPlacementTest.");
            Debug.Log("XRPlacementTest ready: blue grab tower, green socket, MP1a XR Rig. Headset test pending.");
        }
        finally
        {
            SceneManager.SetActiveScene(previousScene);
            EditorSceneManager.CloseScene(testScene, true);
        }
    }

    private static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }
}
