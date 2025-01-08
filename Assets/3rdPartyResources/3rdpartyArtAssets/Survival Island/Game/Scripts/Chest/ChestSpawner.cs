using UnityEngine;

namespace Watermelon
{
    public class ChestSpawner : MonoBehaviour, IWorldElement, IGroundOpenable
    {
        [UniqueID]
        [SerializeField] string uniqueSaveID;[SerializeField] GameObject chestPrefab;

        [Space]
        [Slider(0.0f, 1.0f)]
        [SerializeField] float spawnChance = 0.5f;
        [SerializeField] DuoFloat spawnDelay = new DuoFloat(30, 60);

        [Space]
        [SerializeField] ChestBehavior.UnlockType unlockType;

        [ShowIf("IsPurchaseType")]
        [SerializeField] CurrencyPrice unlockPrice;

        [Space]
        [SerializeField] Resource[] drop;
        [SerializeField] int valueOfSingleDropItem = 1;

        [Space]
        [SerializeField] DuoFloat respawnDelay;

        public int InitialisationOrder => 0;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        private TweenCase spawnTweenCase;
        private bool isHiddenByGround = false;

        public void OnWorldLoaded()
        {
            // skipping one frame to allow isHiddenByGround to be initialised
            Tween.NextFrame(() =>
            {
                TrySpawnChest();
            });
        }

        private void TrySpawnChest()
        {
            if (isHiddenByGround)
                return;

            if (Random.value < spawnChance)
            {
                float delay = spawnDelay.Random();
                if (delay > 0)
                {
                    spawnTweenCase = Tween.DelayedCall(delay, SpawnChest);
                }
                else
                {
                    SpawnChest();
                }
            }
        }

        public void OnWorldUnloaded()
        {
            spawnTweenCase.KillActive();
        }

        private void SpawnChest()
        {
            ChestSave chestSave = SaveController.GetSaveObject<ChestSave>(uniqueSaveID);

            if (chestSave.IsOpened)
                return;

            chestSave.IsOpened = false;

            GameObject chestObject = Instantiate(chestPrefab);
            chestObject.transform.position = transform.position;
            chestObject.transform.rotation = transform.rotation;

            ChestBehavior chestBehavior = chestObject.GetComponent<ChestBehavior>();
            chestBehavior.SetDrop(drop, valueOfSingleDropItem);
            chestBehavior.SetUnlockType(unlockType, unlockPrice);

            chestBehavior.LinkedWorldBehavior = LinkedWorldBehavior;

            chestBehavior.Initialise(chestSave);
            chestBehavior.ChestOpened += OnChestOpened;
        }

        private void OnChestOpened(ChestBehavior chestBehavior)
        {
            chestBehavior.ChestOpened -= OnChestOpened;

            if (Random.value < spawnChance)
            {
                spawnTweenCase = Tween.DelayedCall(respawnDelay.Random(), SpawnChest);
            }
        }

        private bool IsPurchaseType()
        {
            return unlockType == ChestBehavior.UnlockType.Purchase;
        }

        public void OnGroundOpen(bool immediately = false)
        {
            isHiddenByGround = false;

            TrySpawnChest();
        }

        public void OnGroundHidden(bool immediately = false)
        {
            isHiddenByGround = true;
        }

        #region Development

#if UNITY_EDITOR
        private Color gizmoDefaultColorDev;

        private void OnValidate()
        {
            gizmoDefaultColorDev = Gizmos.color;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawWireCube(transform.position, Vector3.one);

            Gizmos.color = gizmoDefaultColorDev;
        }
#endif

        #endregion
    }
}