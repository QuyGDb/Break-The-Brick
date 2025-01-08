using System.Collections.Generic;
using UnityEngine;
using Watermelon.AI;

namespace Watermelon
{
    public sealed class ConstructionTask : BaseTask
    {
        private ConstructionPointBehavior constructionPointBehavior;
        public ConstructionPointBehavior ConstructionPointBehavior => constructionPointBehavior;

        private Vector3[] raycastPositions;

        public ConstructionTask(ConstructionPointBehavior constructionPointBehavior) : base(HelperTaskType.Building, constructionPointBehavior.transform, 10, false)
        {
            this.constructionPointBehavior = constructionPointBehavior; 
            
            Vector3 halfBoxColliderSize = constructionPointBehavior.BoxCollider.size / 2;

            raycastPositions = new Vector3[4]
            {
                targetTransform.position + new Vector3(halfBoxColliderSize.x, 0, 0),
                targetTransform.position + new Vector3(-halfBoxColliderSize.x, 0, 0),
                targetTransform.position + new Vector3(0, 0, halfBoxColliderSize.z),
                targetTransform.position + new Vector3(0, 0, -halfBoxColliderSize.z),
            };
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

        public override bool IsPathExists(HelperBehavior helperBehavior)
        {
            for(int i = 0; i < raycastPositions.Length; i++)
            {
                if (helperBehavior.NavMeshAgentBehaviour.PathExists(raycastPositions[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool Validate(HelperBehavior botBehavior)
        {
            if (!constructionPointBehavior.IsHelperTaskActive)
                return false;

            return constructionPointBehavior.IsActive;
        }

        public override bool GetStateMachineState(out HelperStateMachine.State nextState)
        {
            nextState = HelperStateMachine.State.Building;

            return true;
        }
    }
}