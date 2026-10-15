using UnityEditor;
using UnityEngine;
using TMPro;

public static class SetupTowerSpeedUpgrades
{
    [InitializeOnLoadMethod]
    private static void Queue() { EditorApplication.update += WhenReady; }

    private static void WhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= WhenReady;
        Setup();
    }
    private static readonly string[] TowerPaths =
    {
        "Assets/RoomRapidTower.prefab",
        "Assets/RoomLongRangeTower.prefab"
    };

    [MenuItem("Tools/MP2a/Setup Tower Speed Upgrades")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        foreach (string path in TowerPaths)
        {
            GameObject tower = PrefabUtility.LoadPrefabContents(path);
            try
            {
                TowerShooting shooting = tower.GetComponent<TowerShooting>();
                TowerSpeedUpgrade upgrade = tower.GetComponent<TowerSpeedUpgrade>();
                if (upgrade != null && upgrade.statusLabel != null) continue;
                if (upgrade == null)
                    upgrade = tower.AddComponent<TowerSpeedUpgrade>();

                upgrade.shooting = shooting;
                upgrade.fireIntervalMultiplier = 0.8f;
                upgrade.machineName = path.Contains("Rapid") ? "BOTTLE CAP" : "SPRING PUNCH";
                upgrade.tutorialIndex = path.Contains("Rapid") ? 0 : 1;
                Transform panel = tower.transform.Find("SpeedUpgradePanel");
                if (panel == null)
                    panel = tower.transform.Find("RepairPanel_VisualOnly");
                if (panel != null)
                {
                    panel.name = "SpeedUpgradePanel";
                    upgrade.upgradePanel = panel.GetComponent<Renderer>();
                }

                BoxCollider collider = tower.GetComponent<BoxCollider>();
                if (collider == null)
                    collider = tower.AddComponent<BoxCollider>();
                collider.center = new Vector3(0f, 0.5f, 0f);
                collider.size = new Vector3(1f, 1.5f, 1f);

                TextMeshPro label = new GameObject("UpgradeStatus").AddComponent<TextMeshPro>();
                label.transform.SetParent(tower.transform, false);
                label.transform.localPosition = new Vector3(0f, 1.25f, 0f);
                // Counter the non-uniform scale of the existing placeholder root.
                Vector3 scale = tower.transform.lossyScale;
                label.transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
                label.rectTransform.sizeDelta = new Vector2(3f, 1.2f);
                label.fontSize = 1.7f;
                label.alignment = TextAlignmentOptions.Center;
                label.color = Color.white;
                label.gameObject.SetActive(false);
                upgrade.statusLabel = label;

                PrefabUtility.SaveAsPrefabAsset(tower, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(tower);
            }
        }

        Debug.Log("Scrap upgrades ready: point at machine or cell + E, costs 15/30, damage +1, interval x0.8.");
    }
}
