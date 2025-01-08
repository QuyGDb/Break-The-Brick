using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public class PlayerBehavior : MonoBehaviour, ICharacter, ICharacterGraphics<PlayerGraphics>, IResourceGiver, IResourceTaker, IHitter
    {
        private static readonly int FULL_FLOATING_TEXT_HASH = FloatingTextController.GetHash("Floating");
        private static readonly int FULL_FLOATING_TEXT_DELAY = 2;

        public static readonly int RUN_HASH = Animator.StringToHash("Run");
        public static readonly int MOVEMENT_MULTIPLIER_HASH = Animator.StringToHash("Movement Multiplier");
        public static readonly int SWIMMING_MULTIPLIER_HASH = Animator.StringToHash("Swimming Multiplier");
        public static readonly int HARVESTING_MULTIPLIER_HASH = Animator.StringToHash("Harvesting Multiplier");
        public static readonly int TIRED_MULTIPLIER_HASH = Animator.StringToHash("Tired Multiplier");
        public static readonly int ATTACKING_HASH = Animator.StringToHash("Attacking");

        Dictionary<CurrencyType, ResourceIndicatorData> resourceIndicators = new Dictionary<CurrencyType, ResourceIndicatorData>();

        private static PlayerBehavior playerBehavior;
        public static PlayerBehavior GetBehavior() => playerBehavior;

        public static Vector3 Position { get => playerBehavior.transform.position; }
        public static Vector3 Forward { get => playerBehavior.transform.forward; }
        public static Transform InstanceTransform { get => playerBehavior.transform; }

        [BoxGroup("References", "References")]
        [SerializeField] Collider characterCollider;
        public Collider CharacterCollider => characterCollider;

        [BoxGroup("References")]
        [SerializeField] HealthBehavior healthBehavior;
        [BoxGroup("References")]
        [SerializeField] HealthBehavior swimmingEnergyBehavior;

        [BoxGroup("Settings", "Settings")]
        [SerializeField] int health = 100;
        [BoxGroup("Settings")]
        [SerializeField] int swimmingEnergy = 100;
        [BoxGroup("Settings")]
        [SerializeField] DuoInt damage;
        [BoxGroup("Settings")]
        [SerializeField] float jumpHighDifference = 0.5f;

        [BoxGroup("Triggers", "Triggers & Detectors")]
        [SerializeField] PlayerHittableTrigger hittableTrigger;
        [BoxGroup("Triggers")]
        [SerializeField] PlayerResourcesTrigger resourcesTrigger;
        [BoxGroup("Triggers")]
        [SerializeField] PlayerWaterDetector waterDetector;

        [BoxGroup("Particles", "Particles")]
        [SerializeField] ParticleSystem fallingParticle;
        [BoxGroup("Particles")]
        [SerializeField] ParticleSystem waterFallingParticle;

        [BoxGroup("Other")]
        [SerializeField] FlyingResourceAnimation customFlyingResourceAnimation;

        public bool IsPlayer => true;
        public bool IsDead { get; private set; }

        private bool isAttacking;
        private bool IsHitting { get; set; }

        public Transform Transform => transform;
        public Transform SnappingTransform => transform;

        // Inventory
        private PlayerInventory inventory;
        public PlayerInventory Inventory => inventory;

        // Player Graphics
        private CharacterGraphicsHolder<PlayerGraphics> playerGraphicsHolder;
        public PlayerGraphics PlayerGraphics => playerGraphicsHolder.CharacterGraphics;

        private Animator playerAnimator;

        // Resources
        private List<IHitable> hitableObjectsInRange = new List<IHitable>();
        private IHitable activeHitable;

        public List<CurrencyType> RequiredResources { get; private set; }
        public bool IsResourceTakingBlocked => IsRunning || inventory.CurrentCapacity + resourcesYetToReachCounter >= inventory.MaxCapacity;

        public Vector3 FlyingResourceDestination => Position + Vector3.up;

        public bool AutoPickResources => false;
        private bool inventoryWasFull = false;

        public Vector3 FlyingResourceSpawnPosition => Position + Vector3.up * 2f;

        public float LastTimeResourceGiven { get; private set; }
        public bool IsResourceGivingBlocked => IsRunning;

        private float lastFullMessageTime;
        private float lastShownResindicatorTime;

        public delegate void ResorceReceivedCallback(FlyingResourceBehavior resource);
        public static event ResorceReceivedCallback OnResourceWillBeReceived;

        // Movement
        private ICharacter targetCharacterBehaviour;

        private bool IsMovementEnabled { get; set; }

        private MovementSpeedUpgrade speedUpgrade;
        private SwimmingDurationUpgrade swimmingUpgrade;

        private int jumpCounter = 0;
        private Vector3 playerPrevPos = Vector3.zero;

        public bool IsSwimming { get; private set; } = false;

        private NavMeshAgent agent;
        private bool isRunning;
        public static bool IsRunning => playerBehavior.isRunning;
        private float speed = 0;

        private float maxSpeed;
        private float acceleration;

        private float HungrySpeedCoef => EnergyController.LowEnergySpeedCoef;

        //Health
        private HealthBehavior Health => healthBehavior;
        private HealthBehavior SwimmingEnergy => swimmingEnergyBehavior;

        private float lastTimeGotHit = 0;

        // Audio
        private Transform audioListenerTransform;

        public void Initialise()
        {
            playerBehavior = this;

            // Cache components
            agent = GetComponent<NavMeshAgent>();
            agent.enabled = false;

            // Initialise components
            inventory = new PlayerInventory();
            inventory.Initialise(this);

            // Attach audio listener to player transform
            audioListenerTransform = AudioController.AttachAudioListener(playerBehavior.transform);

            RequiredResources = new List<CurrencyType>((CurrencyType[])Enum.GetValues(typeof(CurrencyType)));
            inventoryWasFull = false;
            Tween.NextFrame(OnInventoryCapacityChanged);

            inventory.CapacityChanged += OnInventoryCapacityChanged;

            // Get upgrades
            speedUpgrade = GlobalUpgradesController.GetUpgrade<MovementSpeedUpgrade>(GlobalUpgradeType.MovementSpeed);

            GlobalUpgradesEventsHandler.OnUpgraded += OnUpgraded;

            swimmingUpgrade = GlobalUpgradesController.GetUpgrade<SwimmingDurationUpgrade>(GlobalUpgradeType.SwimmingDuration);

            // Initialise player graphics
            playerGraphicsHolder = new CharacterGraphicsHolder<PlayerGraphics>();
            playerGraphicsHolder.Initialise(this, PlayerSkinsController.GetActiveSkin().Prefab);

            // Recalculate values
            RecalculateSpeed();

            NavMeshController.InvokeOrSubscribe(delegate
            {
                OnNavMeshUpdated();
            });

            Health.Initialise(health);
            SwimmingEnergy.Initialise(swimmingEnergy);

            Health.ShowOnChange = true;
            Health.HideOnFull = true;
            SwimmingEnergy.HideOnFull = true;

            RequiredResources = new List<CurrencyType>((CurrencyType[])Enum.GetValues(typeof(CurrencyType)));

            hittableTrigger.Init(this);
            resourcesTrigger.Init(this);

            IsMovementEnabled = true;

            FoliageController.RegisterFoliageAgent(transform);

            waterDetector.IsPlayerOnWater += IsPlayerOnWater;
            waterDetector.IsPlayerSwimming += IsPlayerSwimming;
            waterDetector.OnSwimmingMultiplierChanged += OnSwimmingMultiplierChanged;

            waterDetector.IsActive = true;

            EnergyController.OnEnergyChanged += OnHungerChanged;
            PlayerSkinsController.SkinSelected += OnSkinSelected;
        }

        private void Update()
        {
            if (IsDead)
                return;

            // Fix audio listener rotation
            audioListenerTransform.rotation = Quaternion.identity;

            MovementUpdate();
            HealthRestorationUpdate();
            HittingUpdate();
        }

        #region Swimming

        public void IsPlayerOnWater(bool value)
        {
            PlayerGraphics.SetDustParticleStatus(!value);
            PlayerGraphics.SetWaterTrailStatus(value);
        }

        public void IsPlayerSwimming(bool value)
        {
            IsSwimming = value;

            if (IsSwimming)
            {
                Health.ShowOnChange = false;
                Health.Hide();

                SwimmingEnergy.ShowOnChange = true;
            }
            else
            {
                SwimmingEnergy.ShowOnChange = false;
                Health.ShowOnChange = true;

                if (!Health.IsFull)
                {
                    SwimmingEnergy.Hide();
                    Health.Show();
                }
            }
        }

        public void OnSwimmingMultiplierChanged(float value)
        {
            PlayerGraphics.SetSubmersionValue(value);
        }

        #endregion

        #region Graphics
        public void OnGraphicsUpdated(PlayerGraphics characterGraphics)
        {
            characterGraphics.Inititalise(this);

            playerAnimator = characterGraphics.Animator;
        }

        public void OnGraphicsUnloaded(PlayerGraphics currentGraphics)
        {

        }

        private void OnSkinSelected(PlayerSkinData skinData)
        {
            playerGraphicsHolder.SetGraphics(skinData.Prefab);
        }
        #endregion

        #region Upgrades
        private void OnUpgraded(GlobalUpgradeType upgradeType, AbstactGlobalUpgrade upgrade)
        {
            if (upgradeType == GlobalUpgradeType.MovementSpeed)
            {
                RecalculateSpeed();
            }
        }
        #endregion

        #region Hitting
        public void OnResourceInRange(IHitable resource)
        {
            if (hitableObjectsInRange.Contains(resource))
                return;

            if (resource.IsHittable())
            {
                hitableObjectsInRange.Add(resource);
            }
            else
            {
                if (resource.IsActive)
                {
                    UnlockableToolsController.ShowMessage(resource.InteractionAnimationType, resource.SnappingTransform.position + new Vector3(0, 2, 0), Quaternion.identity);
                }
            }
        }

        public void RunHitAnimation()
        {
            if (hitableObjectsInRange.Count == 0)
                return;

            playerAnimator.SetFloat(HARVESTING_MULTIPLIER_HASH, EnergyController.LowEnergyHittingSpeedCoef);

            IsHitting = true;
        }

        public void OnHittableOutsideRangeOrCompleted(IHitable resource)
        {
            if (!hitableObjectsInRange.Contains(resource))
                return;

            hitableObjectsInRange.Remove(resource);

            if (hitableObjectsInRange.Count <= 0)
            {
                StopHittingAnimation();
            }
        }

        private void StopHittingAnimation()
        {
            PlayerGraphics.InteractionAnimations.Disable();

            IsHitting = false;
        }

        public void AddAcceptableResoruces(List<CurrencyType> resources)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (!RequiredResources.Contains(resources[i]))
                    RequiredResources.Add(resources[i]);
            }
        }

        public void RemoveAcceptableResoruces(List<CurrencyType> resources)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (RequiredResources.Contains(resources[i]))
                    RequiredResources.Remove(resources[i]);
            }
        }

        public void OnResourceHit()
        {
            if (activeHitable != null)
            {
                if (activeHitable.IsMutlipleObjectsHitRestricted)
                {
                    activeHitable.GetHit(transform.position);
                }
                else
                {
                    for (int i = 0; i < hitableObjectsInRange.Count; i++)
                    {
                        IHitable res = hitableObjectsInRange[i];

                        if (res.HittableID == activeHitable.HittableID)
                        {
                            res.GetHit(transform.position);
                        }
                    }
                }

#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif
            }
        }

        private void HittingUpdate()
        {
            if (isRunning || IsSwimming)
            {
                if (IsHitting)
                {
                    StopHittingAnimation();
                }
            }
            else
            {
                bool hasResourceNearby = hitableObjectsInRange.Has((res) => res.IsActive);

                if (IsHitting != hasResourceNearby)
                {
                    if (IsHitting)
                    {
                        StopHittingAnimation();
                    }
                    else
                    {
                        RunHitAnimation();
                        HarvestUpdate();
                    }
                }
                else if (IsHitting)
                {
                    HarvestUpdate();
                }
            }
        }

        private void HarvestUpdate()
        {
            var prevResource = activeHitable;

            activeHitable = null;
            float smallestAngle = float.MaxValue;

            for (int i = 0; i < hitableObjectsInRange.Count; i++)
            {
                var resource = hitableObjectsInRange[i];

                if (!resource.IsActive)
                    continue;

                float angle = Quaternion.FromToRotation(transform.forward, (resource.SnappingTransform.position - transform.position).SetY(0).normalized).eulerAngles.y;
                if (angle > 180)
                    angle -= 360;
                if (angle < -180)
                    angle += 360;

                angle = Mathf.Abs(angle);

                if (angle < smallestAngle)
                {
                    smallestAngle = angle;

                    activeHitable = resource;
                }
            }

            if (activeHitable != null)
            {
                Vector3 lookAt = (activeHitable.SnappingTransform.position - transform.position).SetY(0).normalized;

                if (lookAt.sqrMagnitude >= 0.0001f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookAt), Time.deltaTime * activeHitable.SnappingSpeedMultiplier);
                }

                if (activeHitable.HasSnappingDistance)
                {
                    transform.position = Vector3.Lerp(transform.position, activeHitable.SnappingTransform.position.SetY(transform.position.y) - lookAt * activeHitable.SnappingDistance, Time.deltaTime * activeHitable.SnappingSpeedMultiplier);
                }

                if (smallestAngle > 5 && activeHitable.RotateBeforeHit)
                {
                    if (PlayerGraphics.InteractionAnimations.IsAnimationActive)
                        PlayerGraphics.InteractionAnimations.Disable();

                    return;
                }

                if (activeHitable != prevResource || !PlayerGraphics.InteractionAnimations.IsAnimationActive)
                {
                    activeHitable.ActivateInteractionAnimation(PlayerGraphics.InteractionAnimations);
                }
            }
        }

        #endregion

        #region Movement
        private void RecalculateSpeed()
        {
            MovementSpeedUpgrade.MovementSpeedStage currentStage = speedUpgrade.GetCurrentStage();

            maxSpeed = currentStage.PlayerMovementSpeed;
            acceleration = currentStage.PlayerAcceleration;

            agent.speed = maxSpeed;
            agent.acceleration = acceleration;
        }

        private void MovementUpdate()
        {
            var control = Control.CurrentControl;

            if (IsMovementEnabled && control.IsMovementInputNonZero && control.MovementInput.sqrMagnitude > 0.025f && !Health.IsDepleted)
            {
                if (!isRunning)
                {
                    isRunning = true;

                    playerAnimator.SetBool(RUN_HASH, true);

                    speed = 0;

                    if (isAttacking)
                        isAttacking = false;
                }

                float maxAlowedSpeed = control.MovementInput.magnitude * maxSpeed * HungrySpeedCoef * (IsSwimming ? 0.5f : 1);

                if (speed > maxAlowedSpeed)
                {
                    speed -= acceleration * Time.deltaTime;
                    if (speed < maxAlowedSpeed)
                    {
                        speed = maxAlowedSpeed;
                    }
                }
                else
                {
                    speed += acceleration * Time.deltaTime;
                    if (speed > maxAlowedSpeed)
                    {
                        speed = maxAlowedSpeed;
                    }
                }

                bool playerStationary = Vector3.Distance(playerPrevPos, transform.position) < maxAlowedSpeed * Time.deltaTime * 0.5f;

                if (!playerStationary)
                    jumpCounter = 0;

                playerPrevPos = transform.position;

                transform.position += control.MovementInput * Time.deltaTime * speed;

                float multiplier = speed / maxSpeed;

                playerAnimator.SetFloat(MOVEMENT_MULTIPLIER_HASH, multiplier);
                PlayerGraphics.SetMovementMultiplier(playerStationary ? 0 : multiplier);

                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(control.MovementInput.normalized), 0.2f);

                if (!IsSwimming)
                {
                    var forwardPoint = transform.position + transform.forward * 1f;
                    if (playerStationary && NavMesh.SamplePosition(forwardPoint, out var hit, 10f, NavMesh.AllAreas) && (hit.mask == 1 || hit.mask == 8 || hit.mask == 16))
                    {
                        if (Mathf.Abs(hit.position.x - forwardPoint.x) < 0.01f && Mathf.Abs(hit.position.z - forwardPoint.z) < 0.01f)
                        {
                            var difference = forwardPoint.y - hit.position.y;

                            if (difference > jumpHighDifference)
                            {
                                jumpCounter++;

                                if (jumpCounter > 3)
                                {
                                    StartCoroutine(FallingCoroutine(hit.position));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var multiplier = playerAnimator.GetFloat(MOVEMENT_MULTIPLIER_HASH) * 0.8f;
                playerAnimator.SetFloat(MOVEMENT_MULTIPLIER_HASH, multiplier);
                PlayerGraphics.SetMovementMultiplier(multiplier);

                if (isRunning)
                {
                    isRunning = false;

                    playerAnimator.SetBool(RUN_HASH, false);

                    if (targetCharacterBehaviour != null)
                    {
                        isAttacking = true;
                    }
                }
                else
                {
                    if (isAttacking)
                    {
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((targetCharacterBehaviour.Transform.position - transform.position).normalized), Time.deltaTime * 10);
                    }
                }

                jumpCounter = 0;
            }

            if (IsSwimming)
            {
                float swimmingDuration = swimmingUpgrade.GetCurrentStage().DurationinSeconds;
                if (swimmingDuration == 0)
                    swimmingDuration = 1;

                SwimmingEnergy.SubtractPercent(100 * Time.deltaTime / swimmingDuration);

                if (SwimmingEnergy.IsDepleted)
                    TakeDamage(new DamageSource(Health.MaxHealth, null), transform.position);

                var swimmingMultiplier = playerAnimator.GetFloat(SWIMMING_MULTIPLIER_HASH);
                if (swimmingMultiplier <= 0.1f)
                    swimmingMultiplier = 0.1f;
                playerAnimator.SetFloat(SWIMMING_MULTIPLIER_HASH, Mathf.Clamp01(swimmingMultiplier + Time.deltaTime * 4));
            }
            else
            {
                playerAnimator.SetFloat(SWIMMING_MULTIPLIER_HASH, Mathf.Clamp01(playerAnimator.GetFloat(SWIMMING_MULTIPLIER_HASH) - Time.deltaTime * 4));
            }
        }

        public IEnumerator FallingCoroutine(Vector3 fallPosition)
        {
            IsMovementEnabled = false;

            agent.enabled = false;

            var emission = playerGraphicsHolder.CharacterGraphics.Dust.emission;

            emission.enabled = false;

            var duration = Vector3.Distance(fallPosition, transform.position) / maxSpeed / 1.5f;

            transform.DOMoveY(fallPosition.y, duration - 0.1f, 0.1f).SetEasing(Ease.Type.SineIn);
            transform.DOMoveXZ(fallPosition.x, fallPosition.z, duration).SetEasing(Ease.Type.SineOut);

            playerGraphicsHolder.CharacterGraphics.StartFalling();

            yield return new WaitForSeconds(duration);

            if (IsSwimming)
                playerAnimator.SetFloat(SWIMMING_MULTIPLIER_HASH, 1);

            playerGraphicsHolder.CharacterGraphics.StopFalling();

            if (IsSwimming)
            {
                if (waterFallingParticle != null)
                    waterFallingParticle.Play();
            }
            else
            {
                if (fallingParticle != null)
                    fallingParticle.Play();
            }

            emission.enabled = true;

            agent.enabled = true;

            IsMovementEnabled = true;
        }
        #endregion

        #region NavMesh
        public void OnNavMeshUpdated()
        {
            agent.enabled = true;
        }

        public void Warp(Transform destination)
        {
            ParticleSystem dustParticle = playerGraphicsHolder.CharacterGraphics.Dust;
            dustParticle.Stop();

            agent.Warp(destination.position);
            transform.rotation = destination.rotation;

            dustParticle.Play();
        }

        public void Warp(Vector3 position)
        {
            ParticleSystem dustParticle = playerGraphicsHolder.CharacterGraphics.Dust;
            dustParticle.Stop();

            agent.Warp(position);

            dustParticle.Play();
        }
        #endregion

        #region Combat

        public void TakeDamage(DamageSource source, Vector3 hitPosition, bool flash = false)
        {
            if (Health.IsDepleted)
                return;

            Health.Subtract(source.Damage);
            if (!SwimmingEnergy.IsFull)
                SwimmingEnergy.Hide();

            if (Health.IsDepleted)
            {
                IsDead = true;

                StopHittingAnimation();
                hitableObjectsInRange.Clear();

                PlayerGraphics.Animator.SetTrigger("Die");
                PlayerGraphics.Dust.Stop();

                if (IsSwimming)
                {
                    AudioController.PlaySound(AudioController.AudioClips.playerDrowningSound);
                } 
                else
                {
                    AudioController.PlaySound(AudioController.AudioClips.playerDeathSound);
                }

#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_HARD);
#endif

                Overlay.Show(2.0f, () =>
                {
                    WorldController.WorldBehavior.SubworldHandler.DisableSubworld();

                    gameObject.SetActive(false);

                    agent.Warp(WorldController.WorldBehavior.SpawnPoint.position);

                    StopHittingAnimation();
                    hitableObjectsInRange.Clear();

                    PlayerGraphics.Dust.Play();
                    playerAnimator.SetFloat(SWIMMING_MULTIPLIER_HASH, Mathf.Clamp01(IsSwimming ? 1 : 0));

                    gameObject.SetActive(true);

                    Health.Restore();
                    SwimmingEnergy.Restore();

                    IsDead = false;

                    Overlay.Hide(0.3f);
                });
            }
            else
            {
                if (flash)
                    PlayerGraphics.FlashOnHit();

                lastTimeGotHit = Time.time;
                AudioController.PlaySound(AudioController.AudioClips.punch);

#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif
            }
        }

        private void HealthRestorationUpdate()
        {
            if (!Health.IsFull && Time.time > lastTimeGotHit + 5)
            {
                Health.AddPercent(100f * Time.deltaTime / 3);
            }

            if (!SwimmingEnergy.IsFull && !IsSwimming)
            {
                SwimmingEnergy.AddPercent(100f * Time.deltaTime / 3);
            }
        }

        #endregion

        #region Resource Giver
        public bool HasResource(Resource resource)
        {
            return CurrenciesController.HasAmount(resource.currency, resource.amount);
        }

        public bool HasResources()
        {
            return inventory.CurrentCapacity > 0;
        }

        public void GiveResource(Resource resource)
        {
            CurrenciesController.Substract(resource.currency, resource.amount);

            LastTimeResourceGiven = Time.time;

            inventory.RecalculateCapacity();
        }

        public int GetResourceCount(CurrencyType currencyType)
        {
            return CurrenciesController.Get(currencyType);
        }

        #endregion

        #region Resource Taker
        private int resourcesYetToReachCounter;

        public void TakeResource(FlyingResourceBehavior flyingResource, bool fromPlayer = false)
        {
            OnResourceWillBeReceived?.Invoke(flyingResource);

            flyingResource.SetCustomAnimation(customFlyingResourceAnimation);

            resourcesYetToReachCounter += flyingResource.Amount;

            var tweenCase = flyingResource.PlayAnimation(FlyingResourceDestination, () =>
            {
                flyingResource.gameObject.SetActive(false);

                resourcesYetToReachCounter -= flyingResource.Amount;

                CurrenciesController.Add(flyingResource.ResourceType, flyingResource.Amount);
                inventory.RecalculateCapacity();

                ShowResourceIndicator(flyingResource.ResourceType, flyingResource.Amount);
            });

            if (AudioController.AudioClips.resourcesPickUpFromStorageSound != null)
            {
                var gameData = GameController.Data;

                tweenCase.OnTimeReached(gameData.StorageSoundStartTime, () =>
                {
                    gameData.StorageSoundHandler.Play(AudioController.AudioClips.resourcesPickUpFromStorageSound, transform.position);
                });
            }
        }

        public int RequiredMaxAmount(CurrencyType currency)
        {
            return CurrenciesController.GetCurrency(currency).Data.UseInventory ? Inventory.MaxCapacity - Inventory.CurrentCapacity - resourcesYetToReachCounter : int.MaxValue;
        }

        public void OnInventoryCapacityChanged()
        {
            if (inventory.IsFull())
            {
                if (!inventoryWasFull)
                {
                    RequiredResources.RemoveAll((resource) => CurrenciesController.GetCurrency(resource).Data.UseInventory);
                }
                inventoryWasFull = true;
            }
            else if (inventoryWasFull)
            {
                var inventoryResources = new List<CurrencyType>((CurrencyType[])Enum.GetValues(typeof(CurrencyType))).FindAll((resource) => CurrenciesController.GetCurrency(resource).Data.UseInventory);

                for (int i = 0; i < inventoryResources.Count; i++)
                {
                    if (!RequiredResources.Contains(inventoryResources[i]))
                    {
                        RequiredResources.Add(inventoryResources[i]);
                    }
                }

                inventoryWasFull = false;
            }
        }

        #endregion

        #region Drop
        public void ShowFullFloatingText(bool ignoreDelay = false)
        {
            if (ignoreDelay || Time.time > lastFullMessageTime)
            {
                FloatingTextController.SpawnFloatingText(FULL_FLOATING_TEXT_HASH, "FULL!", transform.position + new Vector3(0, 2, 0), Quaternion.identity, Color.red);

                lastFullMessageTime = Time.time + FULL_FLOATING_TEXT_DELAY;
            }
        }

        private void ShowResourceIndicator(CurrencyType resourceType, int amount)
        {
            if (resourceIndicators.ContainsKey(resourceType))
            {
                var data = resourceIndicators[resourceType];

                data.amount += amount;
                data.floatingText.SetText($"+{data.amount} <sprite name={resourceType}>");
            }
            else if (!EnergyController.IsFoorResource(resourceType))
            {
                if (Time.time - lastShownResindicatorTime < 0.4f)
                {
                    float waitingTime = Time.time - lastShownResindicatorTime + 0.05f;
                    if (waitingTime <= 0.05f) waitingTime = Time.deltaTime * 2;

                    Tween.DelayedCall(waitingTime + 0.05f, () => {
                        ShowResourceIndicator(resourceType, amount);
                    });
                } else
                {
                    var data = new ResourceIndicatorData();

                    data.amount = amount;
                    data.floatingText = FloatingTextController.SpawnFloatingText(FULL_FLOATING_TEXT_HASH, $"+{data.amount} <sprite name={resourceType}>", InstanceTransform.position.AddToY(2f), Quaternion.identity, Color.white).GetComponent<FloatingTextBehaviour>();

                    resourceIndicators.Add(resourceType, data);

                    data.floatingText.AddOnTimeReached(0.5f, () =>
                    {
                        resourceIndicators.Remove(resourceType);
                    });

                    lastShownResindicatorTime = Time.time;
                }
            }
        }

        public void OnResourcePickPerformed(ResourceDropBehavior dropBehavior)
        {
            bool playSound = false;

            Currency currency = CurrenciesController.GetCurrency(dropBehavior.CurrencyType);
            if (currency.Data.UseInventory)
            {
                if (!inventory.IsFull())
                {
                    int resourceAmount = dropBehavior.DropAmount;

                    inventory.TryToAdd(dropBehavior.CurrencyType, ref resourceAmount, false);

                    dropBehavior.OnObjectPicked(this, true);

                    ShowResourceIndicator(dropBehavior.CurrencyType, dropBehavior.DropAmount);

                    playSound = true;
                }
                else
                {
                    ShowFullFloatingText();
                }
            }
            else
            {
                CurrenciesController.Add(currency.CurrencyType, dropBehavior.DropAmount);

                dropBehavior.OnObjectPicked(this, true);

                if (!EnergyController.IsFoorResource(currency.CurrencyType))
                {
                    ShowResourceIndicator(dropBehavior.CurrencyType, dropBehavior.DropAmount);
                }

                playSound = true;
            }

            if (playSound)
            {
                if (currency.Data.PickUpSound != null)
                {
                    if (AudioController.AudioClips.resourcesPickUpFromStorageSound != null)
                    {
                        var gameData = GameController.Data;

                        gameData.StorageSoundHandler.Play(currency.Data.PickUpSound, transform.position);

                    }
                }
            }
        }

        public virtual void Rejected()
        {
            if (inventory.IsFull())
            {
                ShowFullFloatingText();
            }
        }
        #endregion

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(Transform other) => (other.position - Position).sqrMagnitude;

        private void OnHungerChanged()
        {
            PlayerGraphics.Animator.SetFloat(TIRED_MULTIPLIER_HASH, EnergyController.EnergyPoints > 0.001f ? 0 : 1);
        }

        #endregion

        #region Unload

        public void Disable()
        {
            enabled = false;
            agent.enabled = false;
        }

        public void Unload()
        {
            AudioController.ResetAudioLisenerParent();

            inventory.Unload();

            inventory.CapacityChanged -= OnInventoryCapacityChanged;

            GlobalUpgradesEventsHandler.OnUpgraded -= OnUpgraded;
        }

        private void OnDestroy()
        {
            FoliageController.RemoveFoliageAgent(transform);

            EnergyController.OnEnergyChanged -= OnHungerChanged;
        }

        #endregion

        private class ResourceIndicatorData
        {
            public FloatingTextBehaviour floatingText;
            public int amount;
        }
    }
}