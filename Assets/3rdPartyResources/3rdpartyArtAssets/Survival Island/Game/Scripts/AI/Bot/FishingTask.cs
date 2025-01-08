using Watermelon.AI;

namespace Watermelon
{
    public sealed class FishingTask : BaseTask
    {
        private FishingPlaceBehavior fishingPlaceBehavior;
        public FishingPlaceBehavior FishingPlaceBehavior => fishingPlaceBehavior;

        public FishingTask(FishingPlaceBehavior fishingPlaceBehavior) : base(HelperTaskType.Fishing, fishingPlaceBehavior.transform)
        {
            this.fishingPlaceBehavior = fishingPlaceBehavior;
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

        public override bool Validate(HelperBehavior helperBehavior)
        {
            if (!fishingPlaceBehavior.IsHelperTaskActive)
                return false;

            return !helperBehavior.Inventory.IsFull;
        }

        public override bool GetStateMachineState(out HelperStateMachine.State nextState)
        {
            nextState = HelperStateMachine.State.Fishing;

            return true;
        }
    }
}