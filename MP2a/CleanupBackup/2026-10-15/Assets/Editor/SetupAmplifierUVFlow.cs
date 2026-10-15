using UnityEditor;
using UnityEngine;

public static class SetupAmplifierUVFlow
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }
    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }

    [MenuItem("Tools/MP2a/Setup Amplifier UV Flow")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string path = "Assets/RoomDamageAmplifier.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            if (root.GetComponent<AmplifierUVFlow>() != null) return;
            const string texturePath = "Assets/Materials/ChargeArrows.asset";
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                texture = new Texture2D(64, 128, TextureFormat.RGBA32, false);
                texture.name = "ChargeArrows";
                texture.wrapMode = TextureWrapMode.Repeat;
                texture.filterMode = FilterMode.Bilinear;
                for (int y = 0; y < 128; y++)
                    for (int x = 0; x < 64; x++)
                    {
                        float arrow = 23f - Mathf.Abs(x - 31.5f) * 0.5f;
                        bool bright = x > 5 && x < 58 && Mathf.Abs(y % 32 - arrow) < 3f;
                        texture.SetPixel(x, y, bright ? Color.white : new Color(0.025f, 0.025f, 0.025f));
                    }
                texture.Apply();
                AssetDatabase.CreateAsset(texture, texturePath);
            }
            const string materialPath = "Assets/Materials/ChargeFlow.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                material.SetTexture("_BaseMap", texture);
                material.SetColor("_BaseColor", new Color(0.12f, 0.12f, 0.12f));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            Transform group = new GameObject("ChargeFlowPanels").transform;
            group.SetParent(root.transform, false);
            AmplifierUVFlow flow = root.AddComponent<AmplifierUVFlow>();
            flow.amplifier = root.GetComponent<DamageAmplifier>();
            flow.panels = new Renderer[4];
            int index = 0;
            for (int side = -1; side <= 1; side += 2)
                for (int column = -1; column <= 1; column += 2)
                {
                    GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    panel.name = "FlowPanel" + index;
                    panel.transform.SetParent(group, false);
                    panel.transform.localPosition = new Vector3(column * 0.22f, 0f, side * 0.32f);
                    panel.transform.localRotation = Quaternion.Euler(0f, side < 0 ? 0f : 180f, 0f);
                    panel.transform.localScale = new Vector3(0.23f, 0.8f, 1f);
                    Object.DestroyImmediate(panel.GetComponent<Collider>());
                    Renderer renderer = panel.GetComponent<Renderer>();
                    renderer.sharedMaterial = material;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    flow.panels[index++] = renderer;
                }
            PrefabUtility.SaveAsPrefabAsset(root, path);
            AssetDatabase.SaveAssets();
            Debug.Log("Amplifier UV flow ready: idle dim, charging amber/upward, release cyan/fast, build phase paused.");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
