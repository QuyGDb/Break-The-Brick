using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Watermelon
{
    public class EnemyAnimationHandler : MonoBehaviour
    {
        [SerializeField] BaseEnemyBehavior enemy;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpawnEnded() => enemy.ReceiveAnimationEvent(EnemyAnimationEventType.SpawnEnded);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Hit() => enemy.ReceiveAnimationEvent(EnemyAnimationEventType.Hit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HitEnded() => enemy.ReceiveAnimationEvent(EnemyAnimationEventType.HitEnded);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySpawnParticle() => enemy.ReceiveAnimationEvent(EnemyAnimationEventType.PlaySpawnParticle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DropResourcesOnDeath() => enemy.ReceiveAnimationEvent(EnemyAnimationEventType.DropResourcesOnDeath);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlayDeathParticle() => enemy.ReceiveAnimationEvent(EnemyAnimationEventType.PlayDeathParticle);
    }

    public enum EnemyAnimationEventType
    {
        SpawnEnded,
        Hit,
        HitEnded,
        PlaySpawnParticle,
        DropResourcesOnDeath,
        PlayDeathParticle,
    }
}
