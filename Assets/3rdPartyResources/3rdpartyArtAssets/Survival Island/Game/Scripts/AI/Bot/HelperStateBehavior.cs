using UnityEngine;

namespace Watermelon
{
    public class HelperStateBehavior : StateBehavior<HelperBehavior>
    {
        protected NavMeshAgentBehaviour navMeshAgent;

        public HelperStateBehavior(HelperBehavior helperBehavior): base(helperBehavior) 
        {
            navMeshAgent = helperBehavior.NavMeshAgentBehaviour;
        }

        public override void OnStart()
        {

        }

        public override void OnEnd()
        {

        }

        public override void OnUpdate()
        {

        }
    }
}