using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Watermelon.AI;

namespace Watermelon
{
    public class HelperBehavior : MonoBehaviour, INavMeshAgent, ICharacterGraphics<HelperGraphics>, IHitter, IResourceGiver, IWorldElement
    {
        public static readonly int MOVEMENT_MULTIPLIER_HASH = Animator.StringToHash("Movement Multiplier");

        public static readonly int WAITING_HASH = Animator.StringToHash("Opening");
        public static readonly int SITTING_HASH = Animator.StringToHash("Sitting");

        public int InitialisationOrder => 10;

        [UniqueID, Order(-1)]
        [SerializeField] string id;
        public string ID => id;

        [Order(-1)]
        [SerializeField] HelperGraphics defaultGraphics;

        [BoxGroup("Opening")]
        [SerializeField] Transform customRestPoint;

        [Space]
        [BoxGroup("Opening")]
        [SerializeField] bool specialOpeningLogic = false;

        [ShowIf("specialOpeningLogic")]
        [BoxGroup("Opening")]
        [SerializeField] bool disableObjectIfZoneIsLocked = false;

        [ShowIf("specialOpeningLogic")]
        [BoxGroup("Opening")]
        [SerializeField] GroundTileComplexBehavior[] linkedTiles;
        [ShowIf("specialOpeningLogic")]
        [BoxGroup("Opening")]
        [SerializeField] BuildingComplexBehavior[] linkedBuildings;

        [Space]
        [ShowIf("specialOpeningLogic")]
        [BoxGroup("Opening")]
        [SerializeField] AnimationClip openingAnimation;

        [BoxGroup("Settings")]
        [SerializeField] HelperTaskType availableTasks;
        public HelperTaskType AvailableTaskTypes => availableTasks;

        [BoxGroup("Settings")]
        [SerializeField] float tasksDistance = 0;
        public float TasksDistance => tasksDistance;

        [Space]
        [BoxGroup("Settings")]
        [SerializeField] SimpleEmoteBehavior emoteBehavior;
        public SimpleEmoteBehavior EmoteBehavior => emoteBehavior;

        [Space]
        [BoxGroup("Settings")]
        [SerializeField] HelperInventory inventory;
        public HelperInventory Inventory => inventory;

        // Components
        protected NavMeshAgentBehaviour navMeshAgentBehaviour;
        public NavMeshAgentBehaviour NavMeshAgentBehaviour => navMeshAgentBehaviour;

        protected NavMeshAgent navMeshAgent;
        protected Rigidbody characterRigidbody;
        protected Animator characterAnimator;

        protected Collider characterCollider;
        public Collider CharacterCollider => characterCollider;

        public Transform Transform => transform;
        public Transform SnappingTransform => transform;

        // Graphics
        private CharacterGraphicsHolder<HelperGraphics> graphicsHolder;
        public HelperGraphics Graphics => graphicsHolder.CharacterGraphics;

        // State machine
        private HelperStateMachine stateMachine;

        // Gathering
        private AbstractHitableBehavior targetHitableBehavior;
        public AbstractHitableBehavior TtargetHitableBehavior => targetHitableBehavior;

        private CurrencyType resourceType;
        public CurrencyType ResourceType => resourceType;

        private HelperSave helperSave;

        private bool isStoringResourcesActive;
        public bool IsStoringResourcesActive => isStoringResourcesActive;

        private BaseTask activeTask;
        public BaseTask ActiveTask => activeTask;

        private bool isRunning;
        public bool IsRunning => isRunning;

        public bool IsOpened => helperSave.IsOpened;

        public bool IsPlayer => false;

        public Vector3 FlyingResourceSpawnPosition => transform.position + new Vector3(0, 1, 0);

        public float LastTimeResourceGiven { get; protected set; }
        public bool IsResourceGivingBlocked => isRunning;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        public bool AutoPickResources => true;

        private Vector3 zoneRestPosition;

        private bool isInitialised;

        public event SimpleCallback HelperUnlocked;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            characterRigidbody = GetComponent<Rigidbody>();

            navMeshAgentBehaviour = new NavMeshAgentBehaviour();
            navMeshAgentBehaviour.Initialise(this, navMeshAgent);

            stateMachine = GetComponent<HelperStateMachine>();
            stateMachine.enabled = false;
            stateMachine.Initialise(this, navMeshAgentBehaviour);

            emoteBehavior.Initialise();
        }

        public void OnWorldLoaded()
        {
            isInitialised = true;

            graphicsHolder = new CharacterGraphicsHolder<HelperGraphics>();
            graphicsHolder.Initialise(this);
            graphicsHolder.LinkGraphics(defaultGraphics);

            inventory.Initialise(this);

            helperSave = SaveController.GetSaveObject<HelperSave>("helper_" + id);

            isStoringResourcesActive = availableTasks.IsTypeAvailable(HelperTaskType.Storing);

            zoneRestPosition = LinkedWorldBehavior.GetHelperRestPosition();
        }

        public void OnNavMeshInitialised()
        {
            if (helperSave.IsOpened)
            {
                navMeshAgentBehaviour.Warp(GetRestPosition());

                OnLinkedElementOpened();
            }
            else
            {
                if (specialOpeningLogic && (!linkedTiles.IsNullOrEmpty() || !linkedBuildings.IsNullOrEmpty()))
                {
                    if(disableObjectIfZoneIsLocked)
                        gameObject.SetActive(false);

                    ActivateWaitingAnimation();

                    if (!linkedTiles.IsNullOrEmpty())
                    {
                        foreach (GroundTileComplexBehavior linkedTile in linkedTiles)
                        {
                            if(linkedTile != null)
                            {
                                linkedTile.SubscribeOnFullyUnlocked(CheckIfLinkedElementsOpened);
                                linkedTile.InvokeOrSubscribe(CheckIfLinkedElementsOpened);
                            }
                        }
                    }

                    if (!linkedBuildings.IsNullOrEmpty())
                    {
                        foreach (BuildingComplexBehavior linkedBuilding in linkedBuildings)
                        {
                            if (linkedBuilding != null)
                            {
                                linkedBuilding.SubscribeOnFullyUnlocked(CheckIfLinkedElementsOpened);
                                linkedBuilding.InvokeOrSubscribe(CheckIfLinkedElementsOpened);
                            }
                        }
                    }

                    CheckIfLinkedElementsOpened();
                }
                else
                {
                    OnLinkedElementOpened();
                }
            }
        }

        public void OnWorldUnloaded()
        {
            navMeshAgentBehaviour.Unload();

            emoteBehavior.Unload();

            stateMachine.StopMachine();
        }

        private void CheckIfLinkedElementsOpened()
        {
            if(IsAnyLinkedElementsOpened())
            {
                OnLinkedElementOpened();
            }
        }

        private bool IsAnyLinkedElementsOpened()
        {
            if (!linkedTiles.IsNullOrEmpty())
            {
                foreach (GroundTileComplexBehavior linkedTile in linkedTiles)
                {
                    if(linkedTile != null && linkedTile.IsOpen)
                    {
                        return true;
                    }
                }
            }

            if (!linkedBuildings.IsNullOrEmpty())
            {
                foreach (BuildingComplexBehavior linkedBuildign in linkedBuildings)
                {
                    if (linkedBuildign != null && linkedBuildign.IsOpen)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnLinkedElementOpened()
        {
            if (disableObjectIfZoneIsLocked)
            {
                gameObject.SetActive(true);

                graphicsHolder.PlaySpawnAnimation();
            }

            HelperUnlocked?.Invoke();

            helperSave.IsOpened = true;

            stateMachine.enabled = true;
            stateMachine.StartMachine(HelperStateMachine.State.WaitingForTask);

            DisableWaitingAnimation();

            if (!linkedTiles.IsNullOrEmpty())
            {
                foreach (GroundTileComplexBehavior linkedTile in linkedTiles)
                {
                    if(linkedTile != null)
                    {
                        linkedTile.UnsubscribeOnFullyUnlocked(CheckIfLinkedElementsOpened);
                    }
                }
            }

            if (!linkedBuildings.IsNullOrEmpty())
            {
                foreach (BuildingComplexBehavior linkedBuilding in linkedBuildings)
                {
                    if(linkedBuilding)
                    {
                        linkedBuilding.UnsubscribeOnFullyUnlocked(CheckIfLinkedElementsOpened);
                    }
                }
            }
        }

        private void Update()
        {
            if (!isInitialised) return;

            navMeshAgentBehaviour.Update();
            emoteBehavior.Update();

            if (isRunning)
            {
                characterAnimator.SetFloat(MOVEMENT_MULTIPLIER_HASH, navMeshAgent.velocity.magnitude / navMeshAgent.speed);
            }
        }

        public void SnapToHittable(IHitable hitableTarget)
        {
            if (hitableTarget != null)
            {
                Vector3 lookAt = (hitableTarget.SnappingTransform.position - transform.position).SetY(0).normalized;

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookAt), Time.deltaTime * hitableTarget.SnappingSpeedMultiplier);
                if (hitableTarget.HasSnappingDistance)
                {
                    transform.position = Vector3.Lerp(transform.position, hitableTarget.SnappingTransform.position.SetY(transform.position.y) - lookAt * hitableTarget.SnappingDistance, Time.deltaTime * hitableTarget.SnappingSpeedMultiplier);
                }
            }
        }

        #region Graphics
        public void OnGraphicsUpdated(HelperGraphics characterGraphics)
        {
            characterGraphics.Inititalise(this);

            characterAnimator = characterGraphics.Animator;
        }

        public void OnGraphicsUnloaded(HelperGraphics currentGraphics)
        {

        }
        #endregion

        #region NavMesh
        public void OnNavMeshWaypointChanged(Vector3 targetPoint)
        {

        }

        public void OnNavMeshAgentStartedMovement(Vector3 targetPoint)
        {
            isRunning = true;

            characterAnimator.SetFloat(MOVEMENT_MULTIPLIER_HASH, navMeshAgent.velocity.magnitude / navMeshAgent.speed);
        }

        public void OnNavMeshAgentStopped()
        {
            isRunning = false;

            characterAnimator.SetFloat(MOVEMENT_MULTIPLIER_HASH, 0);
        }

        public void OnNavMeshWarpStarted()
        {

        }

        public void OnNavMeshWarpFinished()
        {

        }
        #endregion

        #region Animations
        private void ActivateWaitingAnimation()
        {
            defaultGraphics.InteractionAnimations.OverrideAnimation("Opening", openingAnimation);

            characterAnimator.SetBool(WAITING_HASH, true);
        }

        private void DisableWaitingAnimation()
        {
            characterAnimator.SetBool(WAITING_HASH, false);
        }

        public void ActivateSittingAnimation()
        {
            characterAnimator.SetBool(SITTING_HASH, true);
        }

        public void DisableSittingAnimation()
        {
            characterAnimator.SetBool(SITTING_HASH, false);
        }
        #endregion

        #region Task
        public void SetActiveTask(BaseTask task)
        {
            UnlinkActiveTask();

            activeTask = task;
            activeTask.Take(this);
        }

        public void UnlinkActiveTask()
        {
            if (activeTask == null) return;

            activeTask.Reset();
            activeTask = null;
        }

        public BaseTask FindAvailableTask()
        {
            return LinkedWorldBehavior.TaskHandler.GetAvailableTask(this);
        }
        #endregion

        #region Gathering
        public void SetTargetHitableObject(AbstractHitableBehavior hitableBehavior)
        {
            targetHitableBehavior = hitableBehavior;
        }

        public void OnResourceHit()
        {
            if (targetHitableBehavior != null)
            {
                if (isStoringResourcesActive)
                {
                    targetHitableBehavior.GetHit(transform.position, true, this);
                }
                else
                {
                    targetHitableBehavior.GetHit(transform.position, true);
                }

                if (!targetHitableBehavior.IsActive)
                    targetHitableBehavior = null;
            }
        }

        public void OnResourcePickPerformed(ResourceDropBehavior dropBehavior)
        {
            if(!inventory.IsFull)
            {
                inventory.TryToAdd(dropBehavior.CurrencyType, dropBehavior.DropAmount);
            }

            dropBehavior.OnObjectPicked(this, true);
        }

        public bool HasResource(Resource resource)
        {
            return inventory.HasResource(resource.currency, resource.amount);
        }

        public bool HasResources()
        {
            return inventory.CurrentCapacity > 0;
        }

        public int GetResourceCount(CurrencyType currencyType)
        {
            return inventory.GetResourceCount(currencyType);
        }

        public void GiveResource(Resource resource)
        {
            HelperInventory.Slot resourceSlot = inventory.GetResource(resource.currency);
            if(resourceSlot != null)
            {
                resourceSlot.Substract(resource.amount);
            }

            LastTimeResourceGiven = Time.time;
        }
        #endregion

        public Vector3 GetRestPosition()
        {
            if (customRestPoint != null)
                return customRestPoint.position;

            return zoneRestPosition;
        }

        public void SetCustomResetPoint(Transform customRestPoint)
        {
            this.customRestPoint = customRestPoint;
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;

            if (!linkedTiles.IsNullOrEmpty())
            {
                foreach (GroundTileComplexBehavior linkedTile in linkedTiles)
                {
                    if(linkedTile != null)
                    {
                        Gizmos.DrawLine(transform.position, linkedTile.transform.position);
                    }
                }
            }

            if (!linkedBuildings.IsNullOrEmpty())
            {
                foreach (BuildingComplexBehavior linkedBuilding in linkedBuildings)
                {
                    if(linkedBuilding != null)
                    {
                        Gizmos.DrawLine(transform.position, linkedBuilding.transform.position);
                    }
                }
            }
        }
    }
}