using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Game Data", menuName = "Content/Game Data")]
    public class GameData : ScriptableObject
    {
        [BoxGroup("Menu", "Main Menu")]
        [SerializeField] bool useMainMenu = false;
        public bool UseMainMenu => useMainMenu;

        [Space]
        [SerializeField] GameObject transactionUIPrefab;
        public GameObject TransactionUIPrefab => transactionUIPrefab;

        [SerializeField] EnemiesDatabase enemiesDatabase;
        public EnemiesDatabase EnemiesDatabase => enemiesDatabase;

        [BoxGroup("Storage Sound", "Storage Sound Data")]
        [SerializeField, Range(0, 1)] float storageSoundStartTime = 0.8f;
        [BoxGroup("Storage Sound")]
        [SerializeField] AudioClipHandler storageSoundHandler;

        public AudioClipHandler StorageSoundHandler => storageSoundHandler;
        public float StorageSoundStartTime => storageSoundStartTime;

        [BoxGroup("Steps Sound", "Steps Sound")]
        [SerializeField] DuoFloat stepsVolumeRange = new DuoFloat(0.4f, 0.7f);
        [BoxGroup("Steps Sound")]
        [SerializeField] float minSpeedToTriggerSteps = 0.2f;

        public DuoFloat StepsVolumeRange => stepsVolumeRange;
        public float MinSpeedToTriggerSteps => minSpeedToTriggerSteps;


        public void Init()
        {
            enemiesDatabase.Init();
        }
    }
}
