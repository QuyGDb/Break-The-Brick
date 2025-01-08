using UnityEngine;

namespace Watermelon
{
    public class SimleProjectileBehaviour : BaseProjectileBehaviour
    {
        [SerializeField] string trailParticleName;
        [SerializeField] string hitParticleName;

        private ParticleCase trailParticleCase;

        public override void Initialise(DamageSource damageSource, float speed, ICharacter currentTarget, ICharacter shooter, float autoDisableTime, bool autoDisableOnHit = true)
        {
            base.Initialise(damageSource, speed, currentTarget, shooter, autoDisableTime, autoDisableOnHit);

            trailParticleCase = ParticlesController.PlayParticle(trailParticleName).SetTarget(transform, Vector3.zero);
        }

        protected override void OnCharacterHitted(ICharacter characterBehaviour)
        {
            if (!string.IsNullOrEmpty(hitParticleName))
                ParticlesController.PlayParticle(hitParticleName).SetPosition(transform.position).SetDuration(1.0f);
        }

        protected override void OnProjectileDisabled()
        {
            if (trailParticleCase != null)
            {
                trailParticleCase.SetTarget(null, transform.position);
                trailParticleCase.ForceDisable();

                trailParticleCase = null;
            }

            base.OnProjectileDisabled();
        }
    }
}