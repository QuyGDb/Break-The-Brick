using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.Enemy.Skeleton
{
    public class SkeletonIdleState: StateBehavior<SkeletonEnemyBehavior>
    {
        public SkeletonIdleState(SkeletonEnemyBehavior skeleton) : base(skeleton)
        {
            
        }

        public override void OnUpdate()
        {
            if (Vector3.Distance(Position, Target.SpawnPoint.position) > 0.2f)
            {
                if (Time.frameCount % 10 == 1)
                {
                    Target.MoveToSpawn();
                }
            }
            else
            {
                Target.transform.rotation = Quaternion.Lerp(Target.transform.rotation, Target.SpawnPoint.rotation, Time.deltaTime * 5);
            }
        }
        
    }

    public class SkeletonAttackState : StateBehavior<SkeletonEnemyBehavior>
    {
        public bool IsAttacking { get; private set; }
        
        public SkeletonAttackState(SkeletonEnemyBehavior skeleton) : base(skeleton)
        {

        }

        public override void OnStart()
        {
            IsAttacking = false;
        }

        public override void OnUpdate()
        {
            if (IsAttacking)
            {
                Target.transform.rotation = Quaternion.Lerp(Target.transform.rotation, Quaternion.LookRotation((PlayerBehavior.Position - Position).normalized), Time.deltaTime * 5);
                return;
            }

            if (PlayerBehavior.GetBehavior() == null) return;

            if(Vector3.Distance(Position, PlayerBehavior.Position) > 1)
            {
                if(Time.frameCount % 10 == 2)
                {
                    Target.MoveToPlayer();
                }
            } else if(!PlayerBehavior.GetBehavior().IsDead)
            {
                Target.Attack();

                IsAttacking = true;

                Target.OnHitEnded += OnHitEnded;
            }
        }

        private void OnHitEnded()
        {
            Tween.DelayedCall(0.5f, () => IsAttacking = false);
            Target.OnHitEnded -= OnHitEnded;
        }
    }
}