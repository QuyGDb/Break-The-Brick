using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.AI
{
    public class HelperStateMachine : AbstractStateMachine<HelperStateMachine.State>
    {
        private const float TASK_UPDATE_DELAY = 3.0f;
        
        private HelperBehavior helperBehavior;
        private NavMeshAgentBehaviour navMeshAgentBehaviour;

        private float taskWaitingDelay;

        public void Initialise(HelperBehavior helperBehavior, NavMeshAgentBehaviour navMeshAgentBehaviour)
        {
            this.helperBehavior = helperBehavior;
            this.navMeshAgentBehaviour = navMeshAgentBehaviour;

            StateCase waitingForTaskStateCase = new StateCase();
            waitingForTaskStateCase.state = new WaitingForTaskState(helperBehavior);
            waitingForTaskStateCase.transitions = new List<StateTransition<State>>
            {
                new StateTransition<State>(WaitForTaskStateTransition, transitionType: StateTransitionType.Independent),
            };

            StateCase gatheringStateCase = new StateCase();
            gatheringStateCase.state = new GatheringState(helperBehavior);
            gatheringStateCase.transitions = new List<StateTransition<State>>
            {
                new StateTransition<State>(TaskFinish, transitionType: StateTransitionType.OnFinish),
            };

            StateCase storingStateCase = new StateCase();
            storingStateCase.state = new StoringState(helperBehavior);
            storingStateCase.transitions = new List<StateTransition<State>>
            {
                new StateTransition<State>(TaskFinish, transitionType: StateTransitionType.OnFinish),
            };

            StateCase buildingStateCase = new StateCase();
            buildingStateCase.state = new BuildingState(helperBehavior);
            buildingStateCase.transitions = new List<StateTransition<State>>
            {
                new StateTransition<State>(TaskFinish, transitionType: StateTransitionType.OnFinish),
            };

            StateCase converterStoringStateCase = new StateCase();
            converterStoringStateCase.state = new ConverterStoringState(helperBehavior);
            converterStoringStateCase.transitions = new List<StateTransition<State>>
            {
                new StateTransition<State>(TaskFinish, transitionType: StateTransitionType.OnFinish),
            };

            StateCase fishingStateCase = new StateCase();
            fishingStateCase.state = new FishingState(helperBehavior);
            fishingStateCase.transitions = new List<StateTransition<State>>
            {
                new StateTransition<State>(TaskFinish, transitionType: StateTransitionType.OnFinish),
            };

            states.Add(State.WaitingForTask, waitingForTaskStateCase);
            states.Add(State.Gathering, gatheringStateCase);
            states.Add(State.Storing, storingStateCase);
            states.Add(State.Building, buildingStateCase);
            states.Add(State.ConverterStoring, converterStoringStateCase);
            states.Add(State.Fishing, fishingStateCase);

            startState = State.WaitingForTask;
        }

        public void StartMachine(State initialState)
        {
            startState = initialState;

            StartMachine();
        }

        private bool WaitForTaskStateTransition(out State nextState)
        {
            if (helperBehavior.ActiveTask != null && helperBehavior.ActiveTask.IsActive && helperBehavior.ActiveTask.Validate(helperBehavior))
            {
                if (helperBehavior.ActiveTask.GetStateMachineState(out nextState))
                {
                    return true;
                }
            }

            if (Time.time > taskWaitingDelay)
            {
                taskWaitingDelay = Time.time + TASK_UPDATE_DELAY;

                if(!EnergyController.IsEnergySystemEnabled || EnergyController.EnergyPoints > 0)
                {
                    BaseTask task = helperBehavior.FindAvailableTask();
                    if (task != null)
                    {
                        helperBehavior.SetActiveTask(task);

                        if (task.GetStateMachineState(out nextState))
                        {
                            return true;
                        }
                    }
                }
            }

            nextState = State.WaitingForTask;

            return false;
        }

        private bool TaskFinish(out State nextState)
        {
            nextState = State.WaitingForTask;

            return true;
        }

        public enum State
        {
            WaitingForTask = 0,
            Idle = 1,
            Gathering = 2,
            Storing = 3,
            Building = 4,
            ConverterStoring = 5,
            Fishing = 6,
        }
    }

    public class WaitingForTaskState : HelperStateBehavior
    {
        private bool isSitting;

        public WaitingForTaskState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            isSitting = false;

            if (target != null)
            {
                navMeshAgent.Stop();
                navMeshAgent.SetWaypoints(target.GetRestPosition());
                navMeshAgent.PathFinished += RestPositionReached;
            }
        }

        private void RestPositionReached()
        {
            isSitting = true;

            target.ActivateSittingAnimation();
        }

        public override void OnUpdate()
        {
            if(isSitting)
            {
                if (EnergyController.IsEnergySystemEnabled && EnergyController.EnergyPoints == 0)
                {
                    target.EmoteBehavior.Show(SimpleEmoteBehavior.EmoteType.Hunger);
                }
                else
                {
                    target.EmoteBehavior.Show(SimpleEmoteBehavior.EmoteType.StorageIsFull);
                }
            }
        }

        public override void OnEnd()
        {
            isSitting = false;

            target.DisableSittingAnimation();

            target.EmoteBehavior.Hide();
        }
    }

    public class FishingState : HelperStateBehavior
    {
        private FishingPlaceBehavior fishingPlace;

        private bool isGathering;

        public FishingState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            isGathering = false;
            navMeshAgent.Stop();

            BaseTask task = target.ActiveTask;
            if (task != null)
            {
                FishingTask fishingTask = (FishingTask)task;
                if (fishingTask != null)
                {
                    fishingPlace = fishingTask.FishingPlaceBehavior;

                    target.SetTargetHitableObject(fishingPlace);

                    Vector3 direction = (target.transform.position - fishingPlace.transform.position).normalized;

                    navMeshAgent.SetWaypoints(fishingPlace.transform.position + direction);
                    navMeshAgent.PathFinished += OnResourceReached;
                }
                else
                {
                    InvokeOnFinished();
                }
            }
            else
            {
                InvokeOnFinished();
            }
        }

        private void OnResourceReached()
        {
            if (fishingPlace.Health > 0 && Vector3.Distance(target.transform.position, fishingPlace.transform.position) <= 2.0f)
            {
                isGathering = true;

                navMeshAgent.Stop();

                fishingPlace.ActivateInteractionAnimation(target.Graphics.InteractionAnimations);
            }
            else
            {
                InvokeOnFinished();
            }
        }

        public override void OnUpdate()
        {
            if (fishingPlace != null && fishingPlace.Health <= 0)
            {
                InvokeOnFinished();
            }

            if (!isGathering && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();
            }

            if (isGathering)
            {
                target.SnapToHittable(fishingPlace);
            }
        }

        public override void OnEnd()
        {
            target.Graphics.InteractionAnimations.Disable();

            navMeshAgent.Stop();

            isGathering = false;

            target.UnlinkActiveTask();
        }
    }

    public class GatheringState : HelperStateBehavior
    {
        private ResourceSourceBehavior targetResource;

        private bool isGathering;

        public GatheringState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            isGathering = false;
            navMeshAgent.Stop();

            BaseTask task = target.ActiveTask;
            if(task != null)
            {
                GatheringTask geatheringTask = (GatheringTask)task;
                if(geatheringTask != null)
                {
                    targetResource = geatheringTask.ResourceSource;

                    target.SetTargetHitableObject(targetResource);

                    Vector3 direction = (target.transform.position - targetResource.transform.position).normalized;

                    navMeshAgent.SetWaypoints(targetResource.transform.position + direction);
                    navMeshAgent.PathFinished += OnResourceReached;
                }
                else
                {
                    InvokeOnFinished();
                }
            }
            else
            {
                InvokeOnFinished();
            }
        }

        private void OnResourceReached()
        {
            if(targetResource.Health > 0 && Vector3.Distance(target.transform.position, targetResource.transform.position) <= 2.0f)
            {
                isGathering = true;

                navMeshAgent.Stop();

                targetResource.ActivateInteractionAnimation(target.Graphics.InteractionAnimations);
            }
            else
            {
                InvokeOnFinished();
            }
        }

        public override void OnUpdate()
        {
            if (targetResource != null && targetResource.Health <= 0)
            {
                InvokeOnFinished();
            }

            if(!isGathering && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();
            }

            if(isGathering)
            {
                target.SnapToHittable(targetResource);
            }
        }

        public override void OnEnd()
        {
            target.Graphics.InteractionAnimations.Disable();

            navMeshAgent.Stop();

            isGathering = false;

            target.UnlinkActiveTask();
        }
    }

    public class StoringState : HelperStateBehavior
    {
        private ResourceStorageBuildingBehavior targetStorage;

        private bool storageReached;

        public StoringState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            navMeshAgent.Stop();

            BaseTask task = target.ActiveTask;
            if (task != null)
            {
                StoreResourcesTask storeResourcesTask = (StoreResourcesTask)task;
                if (storeResourcesTask != null)
                {
                    targetStorage = storeResourcesTask.StorageBuildingBehavior;

                    Transform targetTransform = targetStorage.Storage.ResourceTakingPoint.transform;

                    storageReached = false;

                    Vector3 randomOffset = UnityEngine.Random.insideUnitSphere;
                    randomOffset.y = 0;

                    navMeshAgent.SetWaypoints(targetTransform.transform.position + randomOffset);
                    navMeshAgent.PathFinished += OnStorageReached;
                }
                else
                {
                    InvokeOnFinished();
                }
            }
            else
            {
                InvokeOnFinished();
            }
        }

        private void OnStorageReached()
        {
            storageReached = true;
        }

        public override void OnUpdate()
        {
            if(targetStorage.IsFull)
            {
                InvokeOnFinished();

                return;
            }

            if(!target.Inventory.HasResource(targetStorage.StoredResources))
            {
                InvokeOnFinished();

                return;
            }

            if(!storageReached && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();
            }
        }

        public override void OnEnd()
        {
            navMeshAgent.Stop();

            target.UnlinkActiveTask();
        }
    }

    public class ConverterStoringState : HelperStateBehavior
    {
        private ResourceConverterBuildingBehavior targetStorage;

        private bool storageReached;

        public ConverterStoringState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            navMeshAgent.Stop();

            BaseTask task = target.ActiveTask;
            if (task != null)
            {
                ConverterStoringTask converterStoringTask = (ConverterStoringTask)task;
                if (converterStoringTask != null)
                {
                    targetStorage = converterStoringTask.ResourceConverter;

                    Transform targetTransform = targetStorage.InStorage.ResourceTakingPoint.transform;

                    storageReached = false;

                    Vector3 randomOffset = UnityEngine.Random.insideUnitSphere;
                    randomOffset.y = 0;

                    navMeshAgent.SetWaypoints(targetTransform.transform.position + randomOffset);
                    navMeshAgent.PathFinished += OnStorageReached;
                }
                else
                {
                    InvokeOnFinished();
                }
            }
            else
            {
                InvokeOnFinished();
            }
        }

        private void OnStorageReached()
        {
            storageReached = true;
        }

        public override void OnUpdate()
        {
            if (targetStorage.InStorage.IsFull())
            {
                InvokeOnFinished();

                return;
            }

            if (!target.Inventory.HasResource(targetStorage.InStorage.RequiredResources))
            {
                InvokeOnFinished();

                return;
            }

            if (!storageReached && !navMeshAgent.IsMoving)
            {
                InvokeOnFinished();
            }
        }

        public override void OnEnd()
        {
            navMeshAgent.Stop();

            target.UnlinkActiveTask();
        }
    }

    public class BuildingState : HelperStateBehavior
    {
        private ConstructionPointBehavior targetConstructionPoint;

        public BuildingState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            navMeshAgent.Stop();

            BaseTask task = target.ActiveTask;
            if (task != null)
            {
                ConstructionTask constructionTask = (ConstructionTask)task;
                if (constructionTask != null)
                {
                    targetConstructionPoint = constructionTask.ConstructionPointBehavior;

                    Vector3 boxColliderSize = targetConstructionPoint.BoxCollider.size;
                    float width = boxColliderSize.x;
                    float height = boxColliderSize.z;

                    float radius = Mathf.Sqrt(width * width + height * height) / 2;

                    target.SetTargetHitableObject(targetConstructionPoint);

                    Vector3 direction = (target.transform.position - targetConstructionPoint.transform.position).normalized;

                    navMeshAgent.SetWaypoints(targetConstructionPoint.transform.position + (direction * radius));
                    navMeshAgent.PathFinished += OnBuildingReached;
                }
                else
                {
                    InvokeOnFinished();
                }
            }
            else
            {
                InvokeOnFinished();
            }
        }

        private void OnBuildingReached()
        {
            if (targetConstructionPoint != null)
            {
                if(!targetConstructionPoint.IsBuilt)
                {
                    targetConstructionPoint.ActivateInteractionAnimation(target.Graphics.InteractionAnimations);
                }

                Vector3 lookAt = (targetConstructionPoint.transform.position - target.transform.position).SetY(0).normalized;

                target.transform.rotation = Quaternion.LookRotation(lookAt);
            }
        }

        public override void OnUpdate()
        {
            if(Target.ActiveTask == null || !Target.ActiveTask.IsActive)
            {
                InvokeOnFinished();
            }

            if (targetConstructionPoint != null && targetConstructionPoint.IsBuilt)
            {
                InvokeOnFinished();
            }
        }

        public override void OnEnd()
        {
            navMeshAgent.Stop();

            target.Graphics.InteractionAnimations.Disable();

            target.UnlinkActiveTask();
        }
    }

    public class WaitingState : HelperStateBehavior
    {
        public WaitingState(HelperBehavior helperBehavior) : base(helperBehavior)
        {

        }

        public override void OnStart()
        {
            navMeshAgent.Stop();
        }

        public override void OnUpdate()
        {

        }

        public override void OnEnd()
        {

        }
    }
}
