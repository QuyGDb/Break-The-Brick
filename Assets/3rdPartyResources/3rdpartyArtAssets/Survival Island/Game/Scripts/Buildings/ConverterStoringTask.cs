using Watermelon.AI;

namespace Watermelon
{
    public sealed class ConverterStoringTask : BaseTask
    {
        private ResourceConverterBuildingBehavior resourceConverter;
        public ResourceConverterBuildingBehavior ResourceConverter => resourceConverter;

        public ConverterStoringTask(ResourceConverterBuildingBehavior resourceConverter) : base(HelperTaskType.ConverterStoring, resourceConverter.InStorage.ResourceTakingPoint.transform, 5, false)
        {
            this.resourceConverter = resourceConverter;
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
            if (resourceConverter == null || !resourceConverter.IsHelperTaskActive || !resourceConverter.gameObject.activeSelf || resourceConverter.InStorage.IsFull())
                return false;

            if (botBehavior.Inventory.HasResource(resourceConverter.InStorage.RequiredResources))
                return true;

            return false;
        }

        public override bool GetStateMachineState(out HelperStateMachine.State nextState)
        {
            nextState = HelperStateMachine.State.ConverterStoring;

            return true;
        }
    }
}