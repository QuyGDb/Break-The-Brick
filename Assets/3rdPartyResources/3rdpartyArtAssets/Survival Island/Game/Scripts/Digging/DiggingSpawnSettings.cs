using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class DiggingSpawnSettings
    {
        [SerializeField] DuoFloat spawnDelay = new DuoFloat(60, 60);
        public DuoFloat SpawnDelay => spawnDelay;

        [SerializeField] int startActivePoints = 2;
        public int StartActivePoints => startActivePoints;

        [SerializeField] int maxActivePoints = 3;
        public int MaxActivePoints => maxActivePoints;

        [Space]
        [SerializeField] PrefabsWeightedList prefabs;
        public PrefabsWeightedList Prefabs => prefabs;
    }
}