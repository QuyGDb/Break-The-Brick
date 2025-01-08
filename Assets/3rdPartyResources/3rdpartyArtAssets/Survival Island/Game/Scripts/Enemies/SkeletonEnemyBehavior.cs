using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class SkeletonEnemyBehavior : BaseEnemyBehavior
    {
        protected override void Awake()
        {
            base.Awake();

            animationCallbacks.Add(EnemyAnimationEventType.SpawnEnded, OnSpawnAnimationEnded);
            animationCallbacks.Add(EnemyAnimationEventType.Hit, OnHit);
            animationCallbacks.Add(EnemyAnimationEventType.HitEnded, HitEnded);
            animationCallbacks.Add(EnemyAnimationEventType.PlaySpawnParticle, PlaySpawnParticle);
            animationCallbacks.Add(EnemyAnimationEventType.PlayDeathParticle, PlayDeathAnimation);
            animationCallbacks.Add(EnemyAnimationEventType.DropResourcesOnDeath, SpawnDrop);
        }

        public void OnHit()
        {
            if (Vector3.Distance(transform.position, PlayerBehavior.Position) < 1)
            {
                PlayerBehavior.GetBehavior().TakeDamage(new DamageSource(10, this), transform.position, true);
            }
        }
    }
}
