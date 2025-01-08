using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public class ResourceSourceBehavior : AbstractHitableBehavior, IGroundOpenable, IWorldElement
    {
        public int InitialisationOrder => 0;

        [Space]
        [BoxFoldout("Hittable", "Hittable")]
        [FormerlySerializedAs("id")]
        [SerializeField] int hittableId;
        public override int HittableID => hittableId;

        [BoxFoldout("Hittable", "Hittable")]
        [FormerlySerializedAs("isExclusive")]
        [SerializeField] bool restrictMultipleHit;

        public override bool IsMutlipleObjectsHitRestricted => restrictMultipleHit;

        [Space]
        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField] int hitsToDestroy = 10;
        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField] float respawnDuration = 5f;

        [Header("Hit Responce")]
        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField] ParticleSystem hitParticle;
        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField] AudioClip hitAudioClip;

        [Space]
        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField] bool bounceOnHit;
        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField] bool tiltOnHit;
        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField, Range(0, 1)] float tiltMultiplier;

        [Header("Harvest Data")]
        [BoxFoldout("Helpers", "Helpers")]
        [SerializeField] bool isHelperTaskActive = true;
        public bool IsHelperTaskActive => isHelperTaskActive;

        [BoxFoldout("Helpers", "Helpers")]
        [SerializeField] HelperTaskType helperTaskType;
        [BoxFoldout("Helpers", "Helpers")]
        [SerializeField] int helperTaskPriority = 0;

        public float DamagePerHit { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth => 1f;

        [Space]
        [BoxFoldout("Drop", "Drop")]
        [SerializeField] List<Resource> drop;
        public ResourcesList Drop { get; private set; }

        [BoxFoldout("Drop", "Drop")]
        [SerializeField] int dropRate = 1;
        [BoxFoldout("Drop", "Drop")]
        [SerializeField] DropAnimation dropAnimation;

        [Space]
        [SerializeField] float toolMessageSpawnHeight;
        [SerializeField] Transform visualsParent;

        private Vector3 defaultScale;

        private List<ResourceVisualStage> harvestStages = new List<ResourceVisualStage>();

        public Vector3 Position => transform.position;

        public static event SimpleCallback OnFirstTimeHit;

        private float lastTimeGotHit = 0;

        private GatheringTask geatheringTask;
        private Transform rotatingTransform;

        private GatheringUpgrade upgrade;

        private UnlockableTool unlockableTool;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        [Button]
        public void DebugTask()
        {
            Debug.Log(geatheringTask.ToString());
        }

        private void Awake()
        {
            rotatingTransform = new GameObject("RotatingParent").transform;

            rotatingTransform.SetParent(transform.parent);
            rotatingTransform.position = transform.position;
            rotatingTransform.rotation = Quaternion.identity;
            rotatingTransform.localScale = Vector3.one;

            transform.SetParent(rotatingTransform);

            Drop = new ResourcesList(drop);

            // Initialise geathering task
            geatheringTask = new GatheringTask(helperTaskType, this, helperTaskPriority);

            unlockableTool = UnlockableToolsController.GetUnlockableTool(interactionAnimationType);

            // Clamp drop rate
            dropRate = Mathf.Clamp(dropRate, 1, int.MaxValue);

            GetComponentsInChildren(harvestStages);

            harvestStages.Sort((first, second) => second.Id - first.Id);

            if (harvestStages.Count == 0)
            {
                GameObject emptyStage = new GameObject("Empty Stage");
                emptyStage.transform.SetParent(transform);
                emptyStage.transform.localPosition = Vector3.zero;
                harvestStages.Add(emptyStage.AddComponent<ResourceVisualStage>());
            }    

            PopulateDrop();

            upgrade = GlobalUpgradesController.GetUpgrade<GatheringUpgrade>(GlobalUpgradeType.Gathering);
            upgrade.OnUpgraded += CalculateDamagePerHit;

            CalculateDamagePerHit();

            Health = MaxHealth;

            IsActive = true;

            defaultScale = visualsParent.localScale;
        }

        public void OnWorldLoaded()
        {
            geatheringTask.Register(LinkedWorldBehavior.TaskHandler);
        }

        public void OnWorldUnloaded()
        {
            for(int i = 0; i < harvestStages.Count; i++)
            {
                harvestStages[i].Clear();
            }

            respawnCase.KillActive();
            hitCase.KillActive();
        }

        private void CalculateDamagePerHit()
        {
            DamagePerHit = 1f / hitsToDestroy * upgrade.GetCurrentStage().DamageMultiplier;
        }

        private void PopulateDrop()
        {
            var dropPerStage = Drop / harvestStages.Count;

            var dropLeftovers = Drop - dropPerStage * harvestStages.Count;

            for (int i = 0; i < harvestStages.Count; i++)
            {
                var stage = harvestStages[i];

                var healthThreshold = ((float)i / harvestStages.Count);
                stage.Init(healthThreshold);

                stage.Drop.AddRange(dropPerStage);

                if (!dropLeftovers.IsNullOrEmpty())
                {
                    var leftoversPerStage = new ResourcesList(dropLeftovers);
                    leftoversPerStage.SetEveryResourceAmountTo(1);

                    stage.Drop.AddRange(leftoversPerStage);

                    dropLeftovers -= leftoversPerStage;
                }
            }
        }

        TweenCase respawnCase;
        TweenCase hitCase;

        private void Respawn()
        {
            for (int i = 0; i < harvestStages.Count; i++)
            {
                var stage = harvestStages[i];
                if (!stage.IsShown)
                    stage.Show();
            }

            Health = MaxHealth;

            respawnCase = Tween.DelayedCall(0.3f, () =>
            {
                IsActive = true;

                geatheringTask.Activate();
            });
        }

        private void Update()
        {
            if (!IsActive)
                return;

            if (Health != MaxHealth && !harvestStages[^1].IsShown && Time.time - lastTimeGotHit > respawnDuration)
            {
                Respawn();
            }

            /*var distance = (transform.position - PlayerBehavior.Position).sqrMagnitude;

            if (distance <= PlayerBehavior.PickUpRadiusSqr && PlayerBehavior.IsRunning == false)
            {
                PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();
                if(playerBehavior.Tools.IsToolAvailable(requiredTool))
                {
                    if (testHitCoroutine == null)
                        testHitCoroutine = StartCoroutine(TestHitCoroutine());

                    playerBehavior.OnResourceInRange(this);
                }
                else if(lastToolMessageTime + 30 <= Time.time)
                {
                    lastToolMessageTime = Time.time;

                    var toolMessage = needToolPool.GetPooledObject(new PooledObjectSettings().SetPosition(transform.position + Vector3.up * toolMessageSpawnHeight).SetRotation(Camera.main.transform.rotation)).GetComponent<NeedToolUI>();

                    toolMessage.Init(requiredTool);
                    Tween.DelayedCall(2.5f, () => toolMessage.gameObject.SetActive(false));
                }
            }
            else
            {
                if (testHitCoroutine != null)
                {
                    StopCoroutine(testHitCoroutine);
                    testHitCoroutine = null;
                    PlayerBehavior.GetBehavior().OnResourceOutsideRangeOrCompleted(this);
                }
            }*/
        }

        public override void GetHit(Vector3 hitSourcePosition, bool drop = true, IHitter resourcePicked = null)
        {
            if (!IsActive)
                return;

            if (Health == MaxHealth)
                OnFirstTimeHit?.Invoke();

            Health -= DamagePerHit;
            if (Health < 0)
                Health = 0;

            lastTimeGotHit = Time.time;

            for (int i = harvestStages.Count - 1; i >= 0; i--)
            {
                ResourceVisualStage stage = harvestStages[i];
                if (!stage.IsShown)
                    continue;

                if (stage.HealthThreshold >= Health)
                {
                    stage.Explode();

                    for (int j = 0; j < stage.Drop.Count; j++)
                    {
                        Resource dropRes = stage.Drop[j];

                        int totalResourcesAmount = dropRes.amount;
                        int steps = Mathf.Clamp(totalResourcesAmount / dropRate, 1, int.MaxValue);
                        int amountPerStep = dropRes.amount / steps;

                        Currency currency = CurrenciesController.GetCurrency(dropRes.currency);

                        for (int d = 0; d < steps; d++)
                        {
                            totalResourcesAmount -= amountPerStep;

                            if (d == steps - 1)
                            {
                                amountPerStep += totalResourcesAmount;
                            }

                            if (drop)
                            {
                                GameObject dropObj = currency.Data.DropResPool.GetPooledObject();
                                ResourceDropBehavior dropResource = dropObj.GetComponent<ResourceDropBehavior>();
                                
                                dropObj.transform.position = stage.gameObject.transform.position.AddToY(dropResource.VerticalOffset);

                                dropResource.Initialise(amountPerStep).SetDropAnimation(dropAnimation).SetDisableTime(30).Throw(transform, stage.gameObject.transform.position, hitSourcePosition);

                                if(resourcePicked != null && resourcePicked.AutoPickResources)
                                {
                                    dropResource.ActivateAutoPick(0.5f, resourcePicked);
                                }
                            }
                        }
                    }
                }
            }

            if (Health == 0)
            {
                IsActive = false;
                respawnCase = Tween.DelayedCall(respawnDuration, Respawn);

                geatheringTask.Disable();
            }

            if (bounceOnHit)
            {
                visualsParent.localScale = defaultScale * 0.9f;
            }

            hitCase.KillActive();
            hitCase = visualsParent.DOScale(Vector3.one, 0.1f).SetEasing(Ease.Type.SineOut);

            if (hitParticle != null)
                hitParticle.Play();

            if (hitAudioClip != null)
                AudioController.PlaySound(hitAudioClip, transform.position);

            TiltOnHit();
        }

        public override bool IsHittable()
        {
            if (unlockableTool != null)
                return unlockableTool.IsUnlocked;

            return true;
        }

        private void TiltOnHit()
        {
            if (!tiltOnHit)
                return;

            var direction = (Position - PlayerBehavior.Position).SetY(0).normalized;

            var rotation = transform.rotation;

            var up = rotatingTransform.up;
            var desiredUp = Vector3.Lerp(rotatingTransform.up, direction.SetY(0.5f).normalized, tiltMultiplier).normalized;

            Tween.DoFloat(0, 1, 0.1f, (value) =>
            {
                rotatingTransform.up = Vector3.Lerp(up, desiredUp, value).normalized;
            }).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                Tween.DoFloat(0, 1, 0.15f, (value) =>
                {
                    rotatingTransform.up = Vector3.Lerp(desiredUp, up, value).normalized;
                }).SetEasing(Ease.Type.SineInOut);
            });
        }

        public void OnGroundOpen(bool immediately)
        {
            gameObject.SetActive(true);

            if (immediately)
            {
                rotatingTransform.localScale = Vector3.one;
            }
            else
            {
                rotatingTransform.localScale = Vector3.zero;
                rotatingTransform.DOScale(Vector3.one, 0.3f).SetEasing(Ease.Type.SineOut);
            }

            geatheringTask.Activate();
        }

        public void OnGroundHidden(bool immediately)
        {
            if (immediately)
            {
                gameObject.SetActive(false);
            }
            else
            {
                transform.DOScale(0, 0.3f).SetEasing(Ease.Type.SineOut).OnComplete(() => gameObject.SetActive(false));
            }

            geatheringTask.Disable();
        }

        private void OnDestroy()
        {
            upgrade.OnUpgraded -= CalculateDamagePerHit;

            respawnCase.KillActive();
            hitCase.KillActive();
        }

        private class ResourceDrop
        {
            public Currency Currency;

            public Resource Resource;
            public int Amount;

            public Vector3 StartPosition;
            public Vector3 HitSourcePosition;
        }

        #region Development
        [Button("Update Object Name")]
        private void UpdateObjectNameDev()
        {
            gameObject.name = drop[0].currency.ToString() + " Resource Source";

            RuntimeEditorUtils.SetDirty(gameObject);
        }

        [Button("Apply Random Rotation")]
        private void ApplyRandomRotationDev()
        {
            transform.eulerAngles = transform.eulerAngles.SetY(Random.Range(0f, 360f));

            RuntimeEditorUtils.SetDirty(gameObject);
        }
        #endregion
    }
}