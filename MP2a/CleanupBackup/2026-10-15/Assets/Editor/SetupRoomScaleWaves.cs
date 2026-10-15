using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupRoomScaleWaves
{
    private const string ScenePath = "Assets/Scenes/RoomScaleDefense.unity";
    private const string EnemyPrefabPath = "Assets/RoomScaleEnemy.prefab";

    [InitializeOnLoadMethod]
    private static void QueueSetup()
    {
        EditorApplication.update += SetupWhenReady;
    }

    private static void SetupWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        EditorApplication.update -= SetupWhenReady;
        if (File.Exists(ScenePath))
            Setup();
    }

    [MenuItem("Tools/MP2a/Setup Room Scale Waves")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.isLoaded;
        Scene previous = SceneManager.GetActiveScene();
        if (openedHere)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        try
        {
            SimpleWaveController existing = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<SimpleWaveController>(true))
                .FirstOrDefault();
            if (existing != null)
                return;

            EnemyHealth[] sceneEnemies = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<EnemyHealth>(true))
                .Where(enemy => enemy.gameObject.scene == scene)
                .OrderBy(enemy => enemy.name)
                .ToArray();
            if (sceneEnemies.Length == 0)
                throw new InvalidOperationException("No sample enemy found in RoomScaleDefense.");

            EnemyPathFollower sampleFollower = sceneEnemies[0].GetComponent<EnemyPathFollower>();
            Transform[] path = sampleFollower.waypoints;
            BaseHealth baseHealth = sampleFollower.baseHealth;
            PlayerResources resources = sceneEnemies[0].playerResources;
            Vector3 spawnPosition = sceneEnemies[0].transform.position;

            GameObject prefabCopy = UnityEngine.Object.Instantiate(sceneEnemies[0].gameObject);
            prefabCopy.name = "RoomScaleEnemy";
            EnemyHealth copyHealth = prefabCopy.GetComponent<EnemyHealth>();
            EnemyPathFollower copyFollower = prefabCopy.GetComponent<EnemyPathFollower>();
            copyHealth.playerResources = null;
            copyFollower.baseHealth = null;
            copyFollower.waypoints = Array.Empty<Transform>();
            GameObject enemyPrefab = PrefabUtility.SaveAsPrefabAsset(prefabCopy, EnemyPrefabPath);
            UnityEngine.Object.DestroyImmediate(prefabCopy);

            GameObject spawnObject = new GameObject("EnemySpawnPoint");
            spawnObject.transform.position = spawnPosition;
            GameObject controllerObject = new GameObject("SimpleWaveController");
            SimpleWaveController controller = controllerObject.AddComponent<SimpleWaveController>();
            controller.enemyPrefab = enemyPrefab.GetComponent<EnemyHealth>();
            controller.spawnPoint = spawnObject.transform;
            controller.waypoints = path;
            controller.baseHealth = baseHealth;
            controller.resources = resources;
            controller.enemyCounts = new[] { 5, 7, 10 };
            controller.enemyHealth = new[] { 4f, 6f, 9f };
            controller.enemySpeeds = new[] { 0.8f, 1f, 1.2f };
            controller.spawnInterval = 0.75f;
            controller.waveBonus = 15;

            DesktopBuildInput buildInput = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<DesktopBuildInput>(true)).First();
            buildInput.waveController = controller;
            BattleReadout readout = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<BattleReadout>(true)).First();
            readout.waveController = controller;
            readout.enemies = Array.Empty<EnemyHealth>();

            foreach (TowerBuildSlot slot in scene.GetRootGameObjects()
                         .SelectMany(root => root.GetComponentsInChildren<TowerBuildSlot>(true)))
                slot.enemies = Array.Empty<EnemyHealth>();

            foreach (string towerPath in new[] { "Assets/RoomRapidTower.prefab", "Assets/RoomLongRangeTower.prefab" })
            {
                GameObject towerRoot = PrefabUtility.LoadPrefabContents(towerPath);
                towerRoot.GetComponent<TowerTargeting>().findEnemiesAutomatically = true;
                PrefabUtility.SaveAsPrefabAsset(towerRoot, towerPath);
                PrefabUtility.UnloadPrefabContents(towerRoot);
            }

            foreach (EnemyHealth enemy in sceneEnemies)
                UnityEngine.Object.DestroyImmediate(enemy.gameObject);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("RoomScaleDefense waves ready: 3 waves, build breaks, increasing health/speed, 15 gold clear bonus.");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            if (openedHere)
                EditorSceneManager.CloseScene(scene, true);
        }
    }
}
