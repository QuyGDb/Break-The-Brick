using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    public class WorldController : MonoBehaviour
    {
        private static WorldController worldController;

        [SerializeField] WorldsDatabase database;

        [Header("Prefabs")]
        [SerializeField] GameObject playerPrefab;
        private static GameObject PlayerPrefab => worldController.playerPrefab;

        private static PlayerBehavior playerBehavior;

        private static WorldGlobalSave worldGlobalSave;

        public static WorldData CurrentWorld { get; private set; }
        public static WorldBehavior WorldBehavior { get; private set; }

        public static event SimpleCallback OnWorldLoaded;

        public void Initialise()
        {
            worldController = this;

            // Load save
            worldGlobalSave = SaveController.GetSaveObject<WorldGlobalSave>("worldGlobal");
        }

        public void UnloadWorld(SimpleCallback onWorldUnloaded)
        {
            NavMeshController.Reset();

            playerBehavior.Unload();

            MissionsController.Unload();

            FloatingCloud.Unload();

            WorldItemCollector.Unload();

            Currency[] currencies = CurrenciesController.Currencies;
            foreach (var currency in currencies)
            {
                currency.Data.DropResPool.ReturnToPoolEverything(true);
                currency.Data.FlyingResPool.ReturnToPoolEverything(true);
            }

            WorldBehavior.Unload();

            SceneManager.UnloadSceneAsync(CurrentWorld.Scene.Name, UnloadSceneOptions.None).OnCompleted(onWorldUnloaded);
        }

        public void LoadCurrentWorld()
        {
            string worldID = worldGlobalSave.worldID;
            if (string.IsNullOrEmpty(worldID))
                worldID = GetWorldData(0).ID;

            LoadWorld(database.GetWorldByID(worldID));
        }

        public void LoadWorld(string worldID)
        {
            worldGlobalSave.worldID = worldID;

            LoadWorld(database.GetWorldByID(worldID));
        }

        public void LoadWorld(WorldData worldData)
        {
            CurrentWorld = worldData;

            WorldItemCollector.Initialise();

            SceneManager.LoadScene(worldData.Scene.Name, LoadSceneMode.Additive);
        }

        public static void SetWorld(WorldBehavior worldBehavior)
        {
            WorldBehavior = worldBehavior;

            WorldBehavior.Initialise();
            WorldBehavior.OnPlayerEntered();

            // Spawn player
            GameObject playerObject = Instantiate(PlayerPrefab);
            playerObject.transform.position = WorldBehavior.SpawnPoint.position;

            playerBehavior = playerObject.GetComponent<PlayerBehavior>();
            playerBehavior.Initialise();

            DistanceToggle.Initialise(playerBehavior.transform);

            VirtualCameraCase gameplayCamera = CinemachineCameraController.GetCamera(CameraType.Preview);
            gameplayCamera.VirtualCamera.m_Follow = playerBehavior.transform;
            gameplayCamera.VirtualCamera.m_LookAt = playerBehavior.transform;

            CinemachineCameraController.EnableCamera(CameraType.Gameplay);
            CinemachineCameraController.SetMainTarget(playerBehavior.transform);

            WorldBehavior.OnWorldLoaded();

            GameController.OnWorldLoaded(worldBehavior);

            WorldBehavior.RegisterAndRecalculateNavMesh(() =>
            {
                MissionsController.ActivateNextMission();

                WorldBehavior.OnWorldNavMeshRecalculated();

                OnWorldLoaded?.Invoke();
            });
        }

        public static WorldData GetWorldData(string worldID)
        {
            return worldController.database.GetWorldByID(worldID);
        }

        public static WorldData GetWorldData(int worldIndex)
        {
            return worldController.database.GetWorldByIndex(worldIndex);
        }

        public static bool IsWorldExists(int worldIndex)
        {
            return worldController.database.IsWorldExists(worldIndex);
        }

        public static void UpdateWorldSave(string activeMissionName)
        {
            worldGlobalSave.activeMissionName = activeMissionName;
        }
    }
}