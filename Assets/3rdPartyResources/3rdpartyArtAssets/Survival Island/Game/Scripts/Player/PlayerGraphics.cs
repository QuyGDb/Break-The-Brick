using UnityEngine;

namespace Watermelon
{
    public class PlayerGraphics : CharacterGraphics
    {
        private static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");

        [BoxGroup("References")]
        [SerializeField] Renderer bodyRenderer;

        [BoxFoldout("Particles", label: "Particles")]
        [SerializeField] ParticleSystem dust;
        public ParticleSystem Dust => dust;
        [BoxFoldout("Particles", label: "Particles")]
        [SerializeField] AnimationCurve dustEmissionCurve;
        private float maxDustEmmision;

        [BoxFoldout("Particles", label: "Particles")]
        [SerializeField] ParticleSystem waterTrail;

        [BoxGroup("Submersion")]
        [SerializeField] AnimationCurve nearShoreSubmersionCurve;
        private float submersionTarget;

        [BoxGroup("Flash On Hit")]
        [SerializeField] protected bool flashOnHit;
        [BoxGroup("Flash On Hit")]
        [SerializeField, ColorUsage(false, true), ShowIf("flashOnHit")] protected Color flashEmissionColor = Color.white * 0.5f;
        [BoxGroup("Flash On Hit")]
        [SerializeField, ShowIf("flashOnHit")] protected float flashDuration = 0.2f;

        private PlayerBehavior playerBehavior;

        // Required components
        private PlayerAnimationHandler playerAnimationHandler;

        public void Inititalise(PlayerBehavior playerBehavior)
        {
            this.playerBehavior = playerBehavior;

            // Cache animator and create override animator controller
            PrepareComponents();

            // Get animator event handler component
            playerAnimationHandler = animator.GetComponent<PlayerAnimationHandler>();
            playerAnimationHandler.Inititalise(this, playerBehavior);

            interactionAnimations.Initialise(animator);

            maxDustEmmision = dust.emission.rateOverDistanceMultiplier;
        }

        public void SetWaterTrailStatus(bool value)
        {
            if (value)
            {
                waterTrail.Play();
            }
            else
            {
                waterTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public void SetDustParticleStatus(bool value)
        {
            if (value)
            {
                dust.Play();
            }
            else
            {
                dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public void SetMovementMultiplier(float multiplier)
        {
            var emission = dust.emission;
            emission.rateOverDistanceMultiplier = dustEmissionCurve.Evaluate(multiplier) * maxDustEmmision;
        }

        public void StartFalling()
        {
            animator.SetBool("Falling", true);
        }

        public void StopFalling()
        {
            animator.SetBool("Falling", false);
        }

        public virtual void FlashOnHit()
        {
            if (!flashOnHit)
                return;

            bodyRenderer.material.DOColor(_EmissionColor, flashEmissionColor, flashDuration / 2f).OnComplete(() =>
            {
                bodyRenderer.material.DOColor(_EmissionColor, Color.black, flashDuration / 2f);
            });
        }

        public void SetSubmersionValue(float value)
        {
            submersionTarget = nearShoreSubmersionCurve.Evaluate(value);
        }

        private void Update()
        {
            var localPos = transform.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, submersionTarget, Time.deltaTime * 10);
            transform.localPosition = localPos;
        }

        public InteractionAnimationData GetInteractionData(InteractionAnimationType type)
        {
            return interactionAnimations.GetAnimationData(type);
        }

        public void Step()
        {
            if (!AudioController.AudioClips.stepSounds.IsNullOrEmpty())
            {
                var data = GameController.Data;
                var playerSpeednormalized = animator.GetFloat("Movement Multiplier");

                if (playerSpeednormalized < data.MinSpeedToTriggerSteps) return;

                var volume = data.StepsVolumeRange.Lerp(Mathf.InverseLerp(data.MinSpeedToTriggerSteps, 1, playerSpeednormalized));
                AudioController.PlaySound(AudioController.AudioClips.stepSounds.GetRandomItem(), volume);
            }
        }

        public void RunWakeUpAnimation()
        {
            animator.SetTrigger("Wake Up");
        }
    }
}