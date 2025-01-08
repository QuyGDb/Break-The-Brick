using UnityEngine;

namespace Watermelon
{
    public class FishingPlaceBehavior : AbstractHitableBehavior, IGroundOpenable, IWorldElement
    {
        public int InitialisationOrder => 0;

        [Space]
        [SerializeField] int id;
        public override int HittableID => id;

        [Header("Harvest Data")]
        [SerializeField] bool isHelperTaskActive = true;
        public bool IsHelperTaskActive => isHelperTaskActive;

        [SerializeField] int hitsToDestroy = 10;
        [SerializeField] float respawnDuration = 5f;
        [SerializeField] DuoFloat lifetime;
        [SerializeField] AudioClip hitAudioClip;

        [Space]
        [SerializeField] CurrencyType dropType;
        [SerializeField] int dropRate = 1;

        [Space]
        [SerializeField] ParticleSystem visualsParticleSystem;
        [SerializeField] ParticleSystem catchParticleSystem;
        [SerializeField] Transform fishingPointTransform;

        public Vector3 Position => transform.position;

        private FishingTask fishingTask;

        private UnlockableTool unlockableTool;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        public override bool IsMutlipleObjectsHitRestricted => true;
        public float DamagePerHit { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth => 1f;

        public override Transform SnappingTransform => fishingPointTransform;

        private TweenCase respawnCase;
        private TweenCase despawnTweenCase;

        private Vector3 defaultFishingPointPosition;

        private FishingRodToolBehavior fishingRodToolBehavior;
        private Transform movingFishingPointTransform;

        private void Awake()
        {
            GameObject movingFishingPoint = new GameObject("[FISHING POINT]");
            movingFishingPoint.transform.SetParent(transform);
            movingFishingPoint.transform.position = fishingPointTransform.position;

            movingFishingPointTransform = movingFishingPoint.transform;

            defaultFishingPointPosition = fishingPointTransform.position;

            unlockableTool = UnlockableToolsController.GetUnlockableTool(interactionAnimationType);

            // Clamp drop rate
            dropRate = Mathf.Clamp(dropRate, 1, int.MaxValue);

            Health = MaxHealth;
            DamagePerHit = 1f / hitsToDestroy;

            visualsParticleSystem.Stop();
        }

        public void OnWorldLoaded()
        {

        }

        public void OnWorldUnloaded()
        {

        }

        public override void ActivateInteractionAnimation(Interaction interactionAnimations)
        {
            base.ActivateInteractionAnimation(interactionAnimations);

            ToolBehavior activeTool = interactionAnimations.ActiveToolBehavior;
            if (activeTool != null)
            {
                fishingRodToolBehavior = (FishingRodToolBehavior)activeTool;
                if (fishingRodToolBehavior != null)
                {
                    fishingRodToolBehavior.SetTargetTransform(movingFishingPointTransform);
                }
            }

            interactionAnimations.CustomEventInvoked += OnToolCustomEventInvoked;

            if (despawnTweenCase != null)
                despawnTweenCase.Reset();
        }

        private void OnToolCustomEventInvoked(string eventName)
        {
            switch (eventName)
            {
                case FishingRodToolBehavior.EVENT_ENABLE_FISHING_FLOAT:
                    catchParticleSystem.Play();
                    break;
            }
        }

        public void ForceSpawn()
        {
            IsActive = false;

            despawnTweenCase.KillActive();

            Spawn(false);
        }

        public void Spawn(bool autoDespawn = true)
        {
            if (IsActive) return;

            Health = MaxHealth;

            IsActive = true;

            InitialiseTask();

            fishingTask.Activate();

            visualsParticleSystem.Play();

            if(autoDespawn)
            {
                float randomLifetime = lifetime.Random();
                if (randomLifetime > 0)
                {
                    despawnTweenCase = Tween.DelayedCall(randomLifetime, Despawn);
                }
            }
        }

        public void Despawn()
        {
            IsActive = false;

            visualsParticleSystem.Stop();

            InitialiseTask();

            fishingTask.Disable();

            FishingController.SpawnRandomFishingPlace();
        }

        public override void GetHit(Vector3 hitSourcePosition, bool drop = true, IHitter resourcePicked = null)
        {
            if (!IsActive)
                return;

            Health -= DamagePerHit;
            if (Health < 0)
                Health = 0;

            Currency currency = CurrenciesController.GetCurrency(dropType);

            GameObject dropObj = currency.Data.DropResPool.GetPooledObject();
            ResourceDropBehavior dropResource = dropObj.GetComponent<ResourceDropBehavior>();
            dropResource.transform.position = movingFishingPointTransform.position;
            dropResource.transform.SetParent(movingFishingPointTransform);

            dropResource.Initialise(dropRate).SetDisableTime(30);

            movingFishingPointTransform.DOBezierMove(hitSourcePosition, 4f, 0f, 0f, 0.3f).OnComplete(() =>
             {
                 if (resourcePicked != null && resourcePicked.AutoPickResources)
                 {
                     dropResource.PerformPick(resourcePicked);
                 }

                 dropResource.transform.SetParent(null);

                 movingFishingPointTransform.position = defaultFishingPointPosition;
             });

            catchParticleSystem.Play();
            
            if (hitAudioClip != null)
                AudioController.PlaySound(hitAudioClip, transform.position);

            if (Health == 0)
            {
                despawnTweenCase.KillActive();

                IsActive = false;

                respawnCase = Tween.DelayedCall(respawnDuration, () =>
                {
                    FishingController.SpawnRandomFishingPlace();
                });

                visualsParticleSystem.Stop();

                fishingTask.Disable();
            }
        }

        public bool CanBeRespawn()
        {
            if (!IsActive)
            {
                if (respawnCase != null)
                    return respawnCase.IsCompleted;

                return true;
            }

            return false;
        }

        public override bool IsHittable()
        {
            if (unlockableTool != null)
                return unlockableTool.IsUnlocked;

            return true;
        }

        public void OnGroundOpen(bool immediately)
        {
            gameObject.SetActive(true);

            FishingController.AddFishingPlace(this, true);
        }

        public void OnGroundHidden(bool immediately)
        {
            gameObject.SetActive(false);

            FishingController.RemoveFishingPlace(this);
        }

        private void OnDestroy()
        {
            respawnCase.KillActive();
        }

        private void InitialiseTask()
        {
            if (fishingTask != null)
                return;

            fishingTask = new FishingTask(this);
            fishingTask.Register(LinkedWorldBehavior.TaskHandler);
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
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(fishingPointTransform.position, 1f);

            Gizmos.color = gizmoDefaultColorDev;
        }


#endif

        #endregion
    }
}