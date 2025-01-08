using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Watermelon
{
    public class DiggingController : MonoBehaviour
    {
        private static DiggingController instance;

        private bool isActive;
        private DiggingSpawnSettings settings;

        private Coroutine spawnCoroutine;

        private static List<DiggingSpotBehavior> activeDiggingPoints = new List<DiggingSpotBehavior>();
        private static List<DiggingSpawnPoint> registeredSpawnPoints = new List<DiggingSpawnPoint>();

        public void Initialise()
        {
            instance = this;
        }

        private IEnumerator SpawnCoroutine()
        {
            WaitForSeconds waitForSeconds;
            int startPoints = settings.StartActivePoints;
            for(int i = 0; i < startPoints; i++)
            {
                SpawnPoint();
            }

            while(isActive)
            {
                waitForSeconds = new WaitForSeconds(settings.SpawnDelay.Random());

                yield return waitForSeconds;

                if(activeDiggingPoints.Count <= settings.MaxActivePoints)
                {
                    SpawnPoint();
                }
            }
        }

        private void SpawnPoint()
        {
            if (registeredSpawnPoints.IsNullOrEmpty()) return;

            GameObject diggingPointPrefab = settings.Prefabs.GetRandomItem();

            if(diggingPointPrefab == null)
            {
                Debug.LogError("There are no linked digging point prefabs for the current world!");

                return;
            }

            IEnumerable<DiggingSpawnPoint> filteredPoints = registeredSpawnPoints.Where(x => x.IsActive).OrderBy(x => Random.value);

            int totalWeight = filteredPoints.Sum(x => x.SpawnPriorityWeight);
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            DiggingSpawnPoint selectedSpawnPoint = null;
            foreach(DiggingSpawnPoint point in filteredPoints)
            {
                currentWeight += point.SpawnPriorityWeight;

                if(currentWeight >= randomValue)
                {
                    selectedSpawnPoint = point;

                    break;
                }
            }

            if (selectedSpawnPoint == null) return;

            DiggingSpotBehavior diggingPointBehavior = selectedSpawnPoint.Spawn(diggingPointPrefab);
            if(diggingPointBehavior != null)
            {
                activeDiggingPoints.Add(diggingPointBehavior);
            }
        }

        public void Activate(DiggingSpawnSettings settings)
        {
            this.settings = settings;

            isActive = true;

            spawnCoroutine = StartCoroutine(SpawnCoroutine());
        }

        public void Disable()
        {
            isActive = false;

            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
        }

        public void Unload()
        {
            registeredSpawnPoints.Clear();
            activeDiggingPoints.Clear();
        }

        public static void OnDiggingPointCollected(DiggingSpotBehavior diggingPointBehavior)
        {
            activeDiggingPoints.Remove(diggingPointBehavior);
        }

        public static void OverrideSpawnSettings(DiggingSpawnSettings settings)
        {
            if (settings == null) return;

            instance.settings = settings;
        }

        public static void RegisterSpawnPoint(DiggingSpawnPoint spawnPoint)
        {
            registeredSpawnPoints.Add(spawnPoint);
        }
    }
}