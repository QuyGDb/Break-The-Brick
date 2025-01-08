using UnityEngine;

namespace Watermelon
{
    public class DiggingSpotBehavior : MonoBehaviour, IHitable
    {
        [SerializeField] GameObject chestPrefab;

        [Slider(0.0f, 1.0f)]
        [ShowIf("IsChestNull")]
        [SerializeField] float chestSpawnChance;
        [ShowIf("IsChestNull")]
        [SerializeField] Resource[] chestDrop;
        [ShowIf("IsChestNull")]
        [SerializeField] int valueOfSingleDropItemInChest = 1;

        [LineSpacer]
        [SerializeField] Resource[] drop;
        [SerializeField] int valueOfSingleDropItem = 1;
        [SerializeField] DropAnimation dropAnimation;

        [LineSpacer]
        [SerializeField] ParticleSystem hitParticle;
        [SerializeField] AudioClip hitAudioClip;

        public InteractionAnimationType InteractionAnimationType => InteractionAnimationType.Digging;

        public Transform SnappingTransform => transform;

        public bool IsActive => gameObject.activeSelf;

        public bool IsMutlipleObjectsHitRestricted => true;
        public int HittableID => 0;

        public bool RotateBeforeHit => true;
        public bool HasSnappingDistance => false;
        public float SnappingDistance => 0.5f;
        public float SnappingSpeedMultiplier => 10;

        private UnlockableTool unlockableTool;

        private DiggingSpawnPoint linkedSpawnPoint;

        private void Awake()
        {
            unlockableTool = UnlockableToolsController.GetUnlockableTool(InteractionAnimationType);

            hitParticle.transform.SetParent(null);
            hitParticle.gameObject.SetActive(false);
        }

        public void LinkSpawnPoint(DiggingSpawnPoint spawnPoint)
        {
            linkedSpawnPoint = spawnPoint;
        }

        public void PlaySpawnAnimation()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(1.0f, 0.3f).SetEasing(Ease.Type.SineOut);
        }

        public void ActivateInteractionAnimation(Interaction interactionAnimations)
        {
            interactionAnimations.Activate(InteractionAnimationType);
        }

        public void GetHit(Vector3 hitSourcePosition, bool param = true, IHitter resourcePicked = null)
        {
            if(chestPrefab != null && Random.value < chestSpawnChance)
            {
                ChestSave chestSave = SaveController.GetSaveObject<ChestSave>("digging_chest");
                chestSave.IsOpened = false;

                GameObject chestObject = Instantiate(chestPrefab);
                chestObject.transform.position = transform.position;
                chestObject.transform.rotation = Quaternion.identity;

                ChestBehavior chestBehavior = chestObject.GetComponent<ChestBehavior>();
                chestBehavior.SetDrop(chestDrop, valueOfSingleDropItemInChest);
                chestBehavior.SetUnlockType(ChestBehavior.UnlockType.Unlocked);

                chestBehavior.Initialise(chestSave);
                chestBehavior.ActivateColliderWithDelay(0.5f);
            }
            else
            {
                for (int j = 0; j < drop.Length; j++)
                {
                    Resource dropRes = drop[j];

                    int totalResourcesAmount = dropRes.amount;
                    int droppedItemsAmount = Mathf.Clamp(totalResourcesAmount / valueOfSingleDropItem, 1, int.MaxValue);
                    int currentValueOfDropItem = valueOfSingleDropItem;

                    Currency currency = CurrenciesController.GetCurrency(dropRes.currency);

                    for (int d = 0; d < droppedItemsAmount; d++)
                    {
                        totalResourcesAmount -= currentValueOfDropItem;

                        if (d == droppedItemsAmount - 1)
                        {
                            currentValueOfDropItem += totalResourcesAmount;
                        }

                        GameObject dropObj = currency.Data.DropResPool.GetPooledObject();
                        ResourceDropBehavior dropResource = dropObj.GetComponent<ResourceDropBehavior>();

                        dropObj.transform.position = gameObject.transform.position.AddToY(dropResource.VerticalOffset);

                        dropResource.Initialise(currentValueOfDropItem).SetDropAnimation(dropAnimation).SetDisableTime(30).Throw(transform, transform.position, hitSourcePosition);

                        if (resourcePicked != null && resourcePicked.AutoPickResources)
                        {
                            dropResource.ActivateAutoPick(0.5f, resourcePicked);
                        }
                    }
                }
            }

            hitParticle.gameObject.SetActive(true);
            hitParticle.transform.position = transform.position;
            hitParticle.Play();

            if(hitAudioClip != null)
                AudioController.PlaySound(hitAudioClip, transform.position, 0.8f);

            gameObject.SetActive(false);

            PlayerBehavior.GetBehavior().OnHittableOutsideRangeOrCompleted(this);

            if (linkedSpawnPoint != null)
            {
                linkedSpawnPoint.OnPointCollected();
                linkedSpawnPoint = null;
            }
        }

        public bool IsHittable()
        {
            if (unlockableTool != null)
                return unlockableTool.IsUnlocked;

            return true;
        }

        public void Unload()
        {
            ParticleSystem tempHitParticle = hitParticle;

            tempHitParticle.WaitForEnd().OnComplete(() =>
            {
                if(tempHitParticle != null)
                    Destroy(tempHitParticle.gameObject);
            });
        }

        private bool IsChestNull()
        {
            return chestPrefab != null;
        }
    }
}
