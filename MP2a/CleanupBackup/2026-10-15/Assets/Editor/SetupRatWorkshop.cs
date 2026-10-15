using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupRatWorkshop
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }

    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Rat Workshop")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string scenePath = "Assets/Scenes/RoomScaleDefense.unity";
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            Transform[] all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
            if (all.Any(t => t.GetComponent<ScrapSpawner>() != null)) return;
            if (!all.Any(t => t.name == "GridBuildingReady"))
            {
                Debug.LogWarning("Run Setup Grid Building before Setup Rat Workshop.");
                return;
            }
            PlayerResources resources = all.Select(t => t.GetComponent<PlayerResources>()).First(r => r != null);
            DesktopBuildInput input = all.Select(t => t.GetComponent<DesktopBuildInput>()).First(r => r != null);
            BaseHealth cheese = all.Select(t => t.GetComponent<BaseHealth>()).First(r => r != null);
            ScrapSpawner spawner = new GameObject("RandomScrapCans").AddComponent<ScrapSpawner>();
            spawner.canPrefab = CreateCan();
            spawner.resources = resources;
            spawner.input = input;
            spawner.cheese = cheese;
            spawner.cells = all.Select(t => t.GetComponent<TowerBuildSlot>()).Where(s => s != null).ToArray();
            TMP_Text title = all.First(t => t.name == "RoomTitle").GetComponent<TMP_Text>();
            title.text = "MIDNIGHT CHEESE DEFENSE";
            cheese.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ProjectileYellow.mat");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Rat workshop ready: 6 random cans, left click to recycle, +10 SCRAP, first-use spatial tutorial.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static ScrapPickup CreateCan()
    {
        const string path = "Assets/ScrapCan.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing.GetComponent<ScrapPickup>();
        GameObject root = new GameObject("ScrapCan");
        try
        {
            ScrapPickup pickup = root.AddComponent<ScrapPickup>();
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.3f, 0f);
            collider.size = new Vector3(0.55f, 0.6f, 0.55f);
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            visual.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
            visual.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ProjectileYellow.mat");
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            TextMeshPro hint = new GameObject("FirstRecycleHint").AddComponent<TextMeshPro>();
            hint.transform.SetParent(root.transform, false);
            hint.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            hint.rectTransform.sizeDelta = new Vector2(2f, 1.1f);
            hint.fontSize = 2f;
            hint.alignment = TextAlignmentOptions.Center;
            hint.color = Color.white;
            hint.gameObject.SetActive(false);
            pickup.hint = hint;
            return PrefabUtility.SaveAsPrefabAsset(root, path).GetComponent<ScrapPickup>();
        }
        finally { Object.DestroyImmediate(root); }
    }
}
