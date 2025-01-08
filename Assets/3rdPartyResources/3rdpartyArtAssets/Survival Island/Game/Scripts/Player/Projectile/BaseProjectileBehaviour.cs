using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon
{
    public abstract class BaseProjectileBehaviour : MonoBehaviour
    {
        public static bool BotAutotargetActive { get; set; } = false;
        public static bool PlayerAutotargetActive { get; set; } = false;

        protected DamageSource damageSource;
        protected float speed;
        private bool autoDisableOnHit;

        private TweenCase disableTweenCase;

        private Collider projectileCollider;
        private Collider targetCollider;

        protected ICharacter currentTarget;
        protected ICharacter shooter;

        private static List<BaseProjectileBehaviour> activeProjectiles = new List<BaseProjectileBehaviour>();

        public virtual void Initialise(DamageSource damageSource, float speed, ICharacter currentTarget, ICharacter shooter, float autoDisableTime, bool autoDisableOnHit = true)
        {
            this.damageSource = damageSource;
            this.speed = speed;
            this.autoDisableOnHit = autoDisableOnHit;

            this.currentTarget = currentTarget;
            this.shooter = shooter;

            projectileCollider = GetComponent<Collider>();
            targetCollider = damageSource.CharacterSource.CharacterCollider;

            if (currentTarget != null)
                transform.LookAt(new Vector3(currentTarget.Transform.position.x, transform.position.y, currentTarget.Transform.position.z));

            Physics.IgnoreCollision(targetCollider, projectileCollider, true);

            if (autoDisableTime > 0)
            {
                disableTweenCase = Tween.DelayedCall(autoDisableTime, delegate
                {
                    if (gameObject != null)
                    {
                        // Disable projectile
                        OnProjectileDisabled();

                        UnregisterProjectile(this);
                    }
                });
            }

            RegisterProjectile(this);
        }

        protected virtual void FixedUpdate()
        {
            if (speed != 0)
            {
                if ((BotAutotargetActive && !shooter.IsPlayer) || (PlayerAutotargetActive && shooter.IsPlayer))
                    transform.LookAt(currentTarget.Transform.position);

                transform.position += transform.forward * speed * Time.fixedDeltaTime;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_CHARACTER)
            {
                ICharacter baseCharacterBehaviour = other.GetComponent<ICharacter>();
                if (baseCharacterBehaviour != null)
                {
                    if (!baseCharacterBehaviour.IsDead)
                    {
                        if (disableTweenCase != null && !disableTweenCase.IsCompleted)
                            disableTweenCase.Kill();

                        // Deal damage to enemy
                        baseCharacterBehaviour.TakeDamage(damageSource, transform.position);

                        // Call hit callback
                        OnCharacterHitted(baseCharacterBehaviour);

                        // Disable projectile
                        if (autoDisableOnHit)
                        {
                            OnProjectileDisabled();

                            UnregisterProjectile(this);
                        }
                    }
                }
            }
            else
            {
                OnObstacleHitted();
            }
        }

        private void OnDestroy()
        {
            if (disableTweenCase != null && !disableTweenCase.IsCompleted)
                disableTweenCase.Kill();
        }

        public void DisableProjectile()
        {
            OnProjectileDisabled();
        }

        protected virtual void OnProjectileDisabled()
        {
            if (disableTweenCase != null && !disableTweenCase.IsCompleted)
                disableTweenCase.Kill();

            gameObject.SetActive(false);

            if (targetCollider != null && projectileCollider != null)
            {
                Physics.IgnoreCollision(targetCollider, projectileCollider, false);

                targetCollider = null;
                projectileCollider = null;
            }

            damageSource = null;
        }

        protected abstract void OnCharacterHitted(ICharacter characterBehaviour);

        protected virtual void OnObstacleHitted()
        {
            OnProjectileDisabled();
        }

        public static void Clear()
        {
            activeProjectiles.Clear();
        }

        public static void RegisterProjectile(BaseProjectileBehaviour projectileBehaviour)
        {
            activeProjectiles.Add(projectileBehaviour);
        }

        public static void UnregisterProjectile(BaseProjectileBehaviour projectileBehaviour)
        {
            int projectileIndex = activeProjectiles.FindIndex(x => x == projectileBehaviour);
            if (projectileIndex != -1)
            {
                activeProjectiles.RemoveAt(projectileIndex);
            }
        }

        public static void DisableActiveProjectiles()
        {
            for (int i = 0; i < activeProjectiles.Count; i++)
            {
                if (activeProjectiles[i] != null)
                    activeProjectiles[i].OnProjectileDisabled();
            }
        }
    }
}
