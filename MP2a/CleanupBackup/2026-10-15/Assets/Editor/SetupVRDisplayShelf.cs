using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class SetupVRDisplayShelf
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup VR Bottle Cap Shelf")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string prefabPath = "Assets/VRBottleCapTower.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            GameObject tower = PrefabUtility.LoadPrefabContents("Assets/RoomRapidTower.prefab");
            try
            {
                Rigidbody body = tower.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                body.mass = 1f;
                PurchasedTowerGrab grab = tower.AddComponent<PurchasedTowerGrab>();
                grab.movementType = XRBaseInteractable.MovementType.Kinematic;
                grab.throwOnDetach = false;
                grab.useDynamicAttach = true;
                grab.colliders.Clear();
                grab.colliders.Add(tower.GetComponent<BoxCollider>());
                prefab = PrefabUtility.SaveAsPrefabAsset(tower, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(tower); }
        }
        const string scenePath = "Assets/Scenes/RoomScaleDefense.unity";
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            if (scene.GetRootGameObjects().Any(r => r.GetComponentInChildren<TowerDisplayShelf>(true) != null)) return;
            PlayerResources resources = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<PlayerResources>(true)).Single();
            foreach (DesktopBuildInput input in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<DesktopBuildInput>(true)))
                input.enableMouseBuilding = false;
            GameObject root = new GameObject("VRBottleCapShelf");
            root.transform.position = new Vector3(-7.5f, 0f, -2f);
            TowerDisplayShelf shelf = root.AddComponent<TowerDisplayShelf>();
            shelf.resources = resources;
            shelf.towerPrefab = prefab.GetComponent<PurchasedTowerGrab>();
            Transform point = new GameObject("DisplayPoint").transform;
            point.SetParent(root.transform, false);
            point.localPosition = new Vector3(0f, 1.48f, 0f);
            shelf.displayPoint = point;
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "DisplayStand";
            table.transform.SetParent(root.transform, false);
            table.transform.localPosition = new Vector3(0f, 0.37f, 0f);
            table.transform.localScale = new Vector3(1.1f, 0.74f, 0.9f);
            table.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TableFrame.mat");
            TextMeshPro label = new GameObject("PriceAndBalance").AddComponent<TextMeshPro>();
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = new Vector3(0f, 1.15f, -0.65f);
            label.fontSize = 1.1f;
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta = new Vector2(2.6f, 1f);
            label.text = "BOTTLE CAP\nGRAB: 20 SCRAP";
            shelf.label = label;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("VR bottle-cap shelf ready at front-left: purchase on grab, snap on release, free relocation. Existing XR rig unchanged.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
