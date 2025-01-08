using System.Collections.Generic;
using Watermelon.AI;

namespace Watermelon
{
    public sealed class StoreResourcesTask : BaseTask
    {
        private List<CurrencyType> availableResources;

        private ResourceStorageBuildingBehavior storageBuildingBehavior;
        public ResourceStorageBuildingBehavior StorageBuildingBehavior => storageBuildingBehavior;

        public StoreResourcesTask(ResourceStorageBuildingBehavior storageBuildingBehavior) : base(HelperTaskType.Storing, storageBuildingBehavior.Storage.ResourceTakingPoint.transform, 5, false)
        {
            this.storageBuildingBehavior = storageBuildingBehavior;

            availableResources = storageBuildingBehavior.StoredResources;
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

        public override bool Validate(HelperBehavior botBehavior)
        {
            if (!storageBuildingBehavior.IsHelperTaskActive)
                return false;

            if (!storageBuildingBehavior.gameObject.activeSelf)
                return false;

            if (storageBuildingBehavior.IsFull)
                return false;

            if (botBehavior.Inventory.HasResource(availableResources))
                return true;

            return false;
        }

        public override bool GetStateMachineState(out HelperStateMachine.State nextState)
        {
            nextState = HelperStateMachine.State.Storing;

            return true;
        }
    }
}