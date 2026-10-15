using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupDismantling
{
    // Not part of the selected game design; no automatic setup.
    private static void Queue() { EditorApplication.update += WhenReady; }

    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Dismantling")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string path = "Assets/Scenes/RoomScaleDefense.unity";
        Scene scene = SceneManager.GetSceneByPath(path);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            if (scene.GetRootGameObjects().Any(g => g.name == "DismantlingCorner")) return;
            PlayerResources resources = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<PlayerResources>()).First();
            GameObject prefab = CreatePrefab();
            GameObject corner = new GameObject("DismantlingCorner");
            for (int i = 0; i < 3; i++)
            {
                GameObject appliance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                appliance.transform.SetParent(corner.transform);
                appliance.transform.position = new Vector3(8.4f, 0.05f, i * 2.2f);
                appliance.GetComponent<BreakableAppliance>().resources = resources;
            }
            TextMeshPro sign = new GameObject("RecyclingSign").AddComponent<TextMeshPro>();
            sign.transform.SetParent(corner.transform);
            sign.transform.position = new Vector3(8.4f, 1.8f, -0.5f);
            sign.text = "RECYCLING CORNER\nPoint + Left click: dismantle";
            sign.fontSize = 2f;
            sign.alignment = TextAlignmentOptions.Center;
            sign.rectTransform.sizeDelta = new Vector2(2.8f, 1f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Dismantling ready: 3 toasters outside the grid, 3 clicks each, physical scrap rewards.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static GameObject CreatePrefab()
    {
        const string path = "Assets/BrokenToaster.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;
        GameObject root = new GameObject("BrokenToaster");
        try
        {
            BreakableAppliance appliance = root.AddComponent<BreakableAppliance>();
            appliance.scrapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ScrapCan.prefab").GetComponent<ScrapPickup>();
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.4f, 0f);
            collider.size = new Vector3(1.1f, 0.8f, 0.7f);
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = collider.center;
            visual.transform.localScale = collider.size;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            appliance.shell = visual.GetComponent<Renderer>();
            appliance.shell.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RoomWall.mat");
            TextMeshPro label = new GameObject("DismantleTutorial").AddComponent<TextMeshPro>();
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = Vector3.up * 1.6f;
            label.rectTransform.sizeDelta = new Vector2(3f, 1.3f);
            label.fontSize = 1.8f;
            label.alignment = TextAlignmentOptions.Center;
            label.gameObject.SetActive(false);
            appliance.label = label;
            return PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { Object.DestroyImmediate(root); }
    }
}
