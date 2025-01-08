using UnityEngine;
using Watermelon.AI;

namespace Watermelon
{
    public sealed class GatheringTask : BaseTask
    {
        private ResourceSourceBehavior resourceSource;
        public ResourceSourceBehavior ResourceSource => resourceSource;

        public GatheringTask(HelperTaskType type, ResourceSourceBehavior resourceSource, int defaultPriority = 0) : base(type, resourceSource.transform, defaultPriority, true)
        {
            this.resourceSource = resourceSource;
        }

        protected override void OnTaskActivated()
        {

        }

        protected override void OnTaskDisabled()
        {

        }

        protected override void OnTaskTaken(HelperBehavior helperBehavior)
        {

        }

        protected override void OnTaskReseted()
        {

        }

        public override int GetPriority(HelperBehavior helperBehavior)
        {
            int tempPriority = priority;

            if(helperBehavior.Inventory.HasResource(resourceSource.Drop))
            {
                tempPriority += 20;
            }

            tempPriority += Mathf.RoundToInt(Mathf.Lerp(10, 0, Mathf.InverseLerp(0, 50, Vector3.Distance(helperBehavior.transform.position, resourceSource.transform.position))));

            return tempPriority;
        }

        public override bool Validate(HelperBehavior helperBehavior)
        {
            if (!resourceSource.IsHelperTaskActive)
                return false;

            return !helperBehavior.Inventory.IsFull;
        }

        public override bool GetStateMachineState(out HelperStateMachine.State nextState)
        {
            nextState = HelperStateMachine.State.Gathering;

            return true;
        }
    }
}