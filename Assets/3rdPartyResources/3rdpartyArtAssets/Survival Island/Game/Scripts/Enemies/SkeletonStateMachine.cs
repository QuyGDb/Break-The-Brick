using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.Enemy.Skeleton;

namespace Watermelon
{
    public class SkeletonStateMachine : AbstractStateMachine<SkeletonStateMachine.State>
    {
        private SkeletonEnemyBehavior enemy;

        private void Awake()
        {
            enemy = GetComponent<SkeletonEnemyBehavior>();

            var idleStateCase = new StateCase();
            idleStateCase.state = new SkeletonIdleState(enemy);
            idleStateCase.transitions = new List<StateTransition<State>>
            {
                new StateTransition<State>(IdleStateTransition, StateTransitionType.Independent)
            };

            var attackingStateCase = new StateCase();
            attackingStateCase.state = new SkeletonAttackState(enemy);
            attackingStateCase.transitions = new List<StateTransition<State>>
            {
                new StateTransition<State> (AttackingStateTransition, StateTransitionType.Independent),
            };

            states.Add(State.Idle, idleStateCase);
            states.Add(State.Attacking, attackingStateCase);
        }

        private bool IdleStateTransition(out State nextState)
        {
            nextState = State.Attacking;

            if (PlayerBehavior.GetBehavior() == null) return false;

            return Vector3.Distance(PlayerBehavior.Position, enemy.transform.position) < 5f;
        }
        private bool AttackingStateTransition(out State nextState)
        {
            nextState = State.Idle;

            if (PlayerBehavior.GetBehavior() == null) return false;

            return Vector3.Distance(PlayerBehavior.Position, enemy.transform.position) > 10f;
        }

        public enum State
        {
            Idle,
            Attacking,
        }
    }
}