using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ChestBehavior : MonoBehaviour, IDistanceToggle, IWorldElement, IHitable, IGroundOpenable
    {
        private static readonly int SPAWN_HASH = Animator.StringToHash("Spawn");
        private static readonly int OPEN_HASH = Animator.StringToHash("Open");

        private static readonly int PLAYER_IS_NEAR_HASH = Animator.StringToHash("IsPlayerNear");

        [SerializeField] Animator animatorRef;
        [SerializeField] ChestUIBehavior chestUI;
        [SerializeField] DropAnimation dropAnimation;
        [SerializeField] ParticleSystem openParticleSystem;

        [Space]
        [SerializeField] Transform resourcesSpawnPoint;
        [SerializeField] Vector3 spawnZone;

        [Space]
        [SerializeField] bool isManuallyPlacedInWorld = false;

        [UniqueID]
        [ShowIf("isManuallyPlacedInWorld")]
        [SerializeField] string uniqueSaveID;

        [ShowIf("isManuallyPlacedInWorld")]
        [SerializeField] Resource[] drop;

        [ShowIf("isManuallyPlacedInWorld")]
        [SerializeField] int valueOfSingleDropItem = 1;

        [Space]
        [ShowIf("isManuallyPlacedInWorld")]
        [SerializeField] UnlockType unlockType;

        [ShowIf("ShowUnlockPriceInspector")]
        [SerializeField] CurrencyPrice unlockPrice;
        public CurrencyPrice UnlockPrice => unlockPrice;

        public bool ShowUnlockPriceInspector => unlockType == UnlockType.Purchase && isManuallyPlacedInWorld;

        private AnimatorEventListener animatorEventListener;

        private bool isOpened;

        private bool isUnlocked;
        public bool IsUnlocked => isUnlocked;

        public bool DistanceToggleActivated => true;

        private bool isDistanceToggleInCloseMode;
        public bool IsDistanceToggleInCloseMode => isDistanceToggleInCloseMode;

        public float ActivationDistanceOfDT => 3;
        public Vector3 OriginPositionOfDT => transform.position;

        public int InitialisationOrder => 0;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        public InteractionAnimationType InteractionAnimationType => InteractionAnimationType.Default;

        public Transform SnappingTransform => transform;

        public bool IsActive => !isOpened && isUnlocked;
        public bool IsMutlipleObjectsHitRestricted => true;

        public int HittableID => 0;

        public bool HasSnappingDistance => false;
        public bool RotateBeforeHit => false;
        public float SnappingDistance => 0;
        public float SnappingSpeedMultiplier => 10;

        private bool isInitialised;
        private ChestSave chestSave;

        private BoxCollider boxCollider;
        public BoxCollider BoxCollider => boxCollider;

        private TweenCase disableTweenCase;
        private TweenCase colliderTweenCase;

        public event ChestCallback ChestUnlocked;
        public event ChestCallback ChestOpened;

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();

            animatorEventListener = animatorRef.gameObject.GetComponent<AnimatorEventListener>();
            animatorEventListener.Initialise(this);
            animatorEventListener.CacheMethods("PlayChestOpenParticle");
        }

        public void Initialise(ChestSave save)
        {
            if (isInitialised) return;

            isInitialised = true;

            chestSave = save;

            if (!chestSave.IsOpened)
            {
                isOpened = false;

                if(unlockType == UnlockType.Unlocked)
                {
                    isUnlocked = true;

                    ChestUnlocked?.Invoke(this);
                }
                else
                {
                    isUnlocked = false;
                }

                animatorRef.Play(SPAWN_HASH, -1);

                chestUI.Initialise(this, unlockType);

                if (DistanceToggle.IsInRange(this))
                {
                    isDistanceToggleInCloseMode = true;

                    animatorRef.SetBool(PLAYER_IS_NEAR_HASH, true);

                    chestUI.Activate();
                }
            }
            else
            {
                ChestOpened?.Invoke(this);

                gameObject.SetActive(false);
            }
        }

        public void ActivateColliderWithDelay(float delay)
        {
            boxCollider.enabled = false;

            colliderTweenCase = Tween.DelayedCall(delay, () =>
            {
                boxCollider.enabled = true;
            });
        }

        public void SetUnlockType(UnlockType unlockType, CurrencyPrice unlockPrice = null)
        {
            this.unlockType = unlockType;
            this.unlockPrice = unlockPrice;
        }

        public void SetDrop(Resource[] drop, int valueOfSingleDropItem)
        {
            this.drop = drop;
            this.valueOfSingleDropItem = valueOfSingleDropItem;
        }

        public void SetDropAnimation(DropAnimation dropAnimation)
        {
            this.dropAnimation = dropAnimation;
        }

        public void OnWorldLoaded()
        {

        }

        public void OnWorldUnloaded()
        {
            disableTweenCase.KillActive();
            colliderTweenCase.KillActive();
        }

        private void OnEnable()
        {
            DistanceToggle.AddObject(this);
        }

        private void OnDisable()
        {
            DistanceToggle.RemoveObject(this);
        }

        public void PlayerEnteredZone()
        {
            isDistanceToggleInCloseMode = true;

            animatorRef.SetBool(PLAYER_IS_NEAR_HASH, true);

            if(unlockType != UnlockType.Unlocked)
            {
                if (!isUnlocked)
                {
                    chestUI.Activate();
                }
            }
        }

        public void PlayerLeavedZone()
        {
            isDistanceToggleInCloseMode = false;

            if(!isOpened)
            {
                animatorRef.SetBool(PLAYER_IS_NEAR_HASH, false);
            }

            chestUI.Disable();
        }

        public void ActivateInteractionAnimation(Interaction interactionAnimations)
        {
            interactionAnimations.Activate(InteractionAnimationType);
        }

        public void UnlockChest()
        {
            isUnlocked = true;

            ChestUnlocked?.Invoke(this);
        }

        public void GetHit(Vector3 hitSourcePosition, bool param = true, IHitter resourcePicked = null)
        {
            if (isOpened || !isUnlocked)
                return;

            isOpened = true;

            chestSave.IsOpened = true;

            animatorRef.Play(OPEN_HASH, -1);

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
                    dropObj.transform.position = GetRandomResourceSpawnPoint();

                    ResourceDropBehavior dropResource = dropObj.GetComponent<ResourceDropBehavior>();
                    dropResource.Initialise(currentValueOfDropItem).SetDropAnimation(dropAnimation).SetDisableTime(30).Throw(transform, dropObj.transform.position, hitSourcePosition);

                    if (resourcePicked != null && resourcePicked.AutoPickResources)
                    {
                        dropResource.ActivateAutoPick(0.5f, resourcePicked);
                    }
                }
            }

            ChestOpened?.Invoke(this);

            disableTweenCase = animatorRef.transform.DOScale(0, 0.3f, 2.0f).SetEasing(Ease.Type.BackIn).OnComplete(() =>
            {
                Destroy(gameObject);
            });

            AudioController.PlaySound(AudioController.AudioClips.reward);
        }

        public bool IsHittable() => true;

        private bool IsPurchaseType()
        {
            return unlockType == UnlockType.Purchase && isManuallyPlacedInWorld;
        }

        public void OnGroundOpen(bool immediately = false)
        {
            gameObject.SetActive(true);

            if (isManuallyPlacedInWorld)
                Initialise(SaveController.GetSaveObject<ChestSave>(uniqueSaveID));
        }

        public void OnGroundHidden(bool immediately = false)
        {
            gameObject.SetActive(false);
        }

        private void PlayChestOpenParticle()
        {
            openParticleSystem.Play();
        }

        private Vector3 GetRandomResourceSpawnPoint()
        {
            Vector3 halfSpawnZoneSize = spawnZone / 2;

            return resourcesSpawnPoint.position + new Vector3(Random.Range(-halfSpawnZoneSize.x, halfSpawnZoneSize.x), Random.Range(-halfSpawnZoneSize.y, halfSpawnZoneSize.y), Random.Range(-halfSpawnZoneSize.z, halfSpawnZoneSize.z));
        }

        private void OnDrawGizmosSelected()
        {
            if(resourcesSpawnPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(resourcesSpawnPoint.position, spawnZone);
            }
        }

        public delegate void ChestCallback(ChestBehavior chestBehavior);

        public enum UnlockType
        {
            Unlocked = 0,
            Purchase = 1,
            Ad = 2
        }
    }
}