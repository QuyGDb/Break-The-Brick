using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class BaseEnemyBehavior : MonoBehaviour, ICharacter, IHitable
    {
        private static readonly int MOVEMENT_NULTIPLIER_HASH = Animator.StringToHash("Movement Multiplier");
        private static readonly int ATTACK_TRIGGER = Animator.StringToHash("Attack");
        private static readonly int DIE_TRIGGER = Animator.StringToHash("Die");

        private static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");

        protected Dictionary<EnemyAnimationEventType, SimpleCallback> animationCallbacks;

        public bool IsPlayer => false;
        public bool IsDead { get; private set; }

        public Transform SnappingTransform => transform;
        public Transform Transform => transform;

        [BoxFoldout("Refs", label: "References")]
        [SerializeField] Collider characterCollider;
        public Collider CharacterCollider => characterCollider;
        [BoxFoldout("Refs", label: "References")]
        [SerializeField] Animator animator;
        [BoxFoldout("Refs", label: "References")]
        [SerializeField] NavMeshAgent agent;
        [BoxFoldout("Refs", label: "References")]
        [SerializeField] Renderer bodyRenderer;
        [BoxFoldout("Refs", label: "References")]
        [SerializeField] AudioClip hitSound;

        [BoxFoldout("Drop", label: "Drop")]
        [SerializeField] protected bool dropsResOnDeath = false;

        [BoxFoldout("Drop", label: "Drop")]
        [SerializeField, ShowIf("dropsResOnDeath")] protected int dropRate = 1;
        [BoxFoldout("Drop", label: "Drop")]
        [SerializeField, ShowIf("dropsResOnDeath")] protected DropAnimation dropAnimation;

        [BoxFoldout("Drop", label: "Drop")]
        [SerializeField, ShowIf("dropsResOnDeath")] protected List<Resource> drop;
        public ResourcesList Drop { get; private set; }

        [BoxFoldout("Flash", label: "Response On Hit")]
        [SerializeField] protected bool flashOnHit;
        [BoxFoldout("Flash", label: "Response On Hit")]
        [SerializeField, ColorUsage(false, true), ShowIf("flashOnHit")] protected Color flashEmissionColor = Color.white * 0.5f;
        [BoxFoldout("Flash", label: "Response On Hit")]
        [SerializeField, ShowIf("flashOnHit")] protected float flashDuration = 0.2f;

        [BoxFoldout("Particles", label: "Particles")]
        [SerializeField] ParticleSystem spawnParticle;
        [BoxFoldout("Particles", label: "Particles")]
        [SerializeField] ParticleSystem deathParticle;
        [BoxFoldout("Particles", label: "Particles")]
        [SerializeField] ParticleSystem gotHitParticle;

        protected IStateMachine stateMachine;

        [SerializeField] float health;

        public Transform SpawnPoint { get; private set; }

        private HealthBehavior Health { get; set; }

        #region Hittable

        public InteractionAnimationType InteractionAnimationType => InteractionAnimationType.Chopping;

        public bool IsActive => !IsDead;

        public bool IsMutlipleObjectsHitRestricted => true;

        public int HittableID => -10;

        public bool HasSnappingDistance => false;

        public float SnappingDistance => 0;

        public float SnappingSpeedMultiplier => 10;

        public bool RotateBeforeHit => true;

        #endregion

        public event SimpleCallback OnDeath;
        public event SimpleCallback OnHitEnded;

        protected virtual void Awake()
        {
            stateMachine = GetComponent<IStateMachine>();

            Drop = new ResourcesList(drop);

            animationCallbacks = new Dictionary<EnemyAnimationEventType, SimpleCallback>();

            Health = GetComponent<HealthBehavior>();
            Health.Initialise(health);
        }

        private void Update()
        {
            animator.SetFloat(MOVEMENT_NULTIPLIER_HASH, agent.velocity.magnitude / agent.speed);
        }

        public void Spawn(Transform spawnPoint)
        {
            SpawnPoint = spawnPoint;

            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            transform.localScale = Vector3.one;

            Health.Restore();
            Health.Show();

            agent.Warp(spawnPoint.position);

            IsDead = false;
        }

        public void TakeDamage(DamageSource source, Vector3 position, bool shouldFlash = false)
        {
            Health.Subtract(source.Damage);

            if (Health.IsDepleted)
            {
                IsDead = true;

                stateMachine.StopMachine();

                PlayerBehavior.GetBehavior().OnHittableOutsideRangeOrCompleted(this);

                animator.SetTrigger(DIE_TRIGGER);

                Tween.DelayedCall(2f, () =>
                {
                    gameObject.SetActive(false);
                    PlayerBehavior.GetBehavior().OnHittableOutsideRangeOrCompleted(this);
                    OnDeath?.Invoke();
                });

            }
            else
            {
                if (shouldFlash)
                    FlashOnHit();
            }

            if (gotHitParticle != null)
                gotHitParticle.Play();

            if (hitSound != null)
                AudioController.PlaySound(hitSound, 0.75f);
        }

        public void MoveToSpawn()
        {
            agent.SetDestination(SpawnPoint.position);
            agent.stoppingDistance = 0;
        }

        public void MoveToPlayer()
        {
            agent.SetDestination(PlayerBehavior.Position);
            agent.stoppingDistance = 1;
        }

        public void Attack()
        {
            animator.SetTrigger(ATTACK_TRIGGER);
        }

        #region Animation Callbacks

        public virtual void ReceiveAnimationEvent(EnemyAnimationEventType type)
        {
            if (animationCallbacks.ContainsKey(type))
                animationCallbacks[type]?.Invoke();
        }

        protected virtual void OnSpawnAnimationEnded()
        {
            stateMachine.StartMachine();
        }

        protected virtual void HitEnded()
        {
            OnHitEnded?.Invoke();
        }

        protected virtual void PlaySpawnParticle()
        {
            if (spawnParticle != null)
                spawnParticle.Play();
        }

        protected virtual void PlayDeathAnimation()
        {
            if (deathParticle != null)
                deathParticle.Play();
        }

        protected virtual void SpawnDrop()
        {
            if (!dropsResOnDeath)
                return;

            for (int i = 0; i < Drop.Count; i++)
            {
                Resource dropRes = Drop[i];

                int totalResourcesAmount = dropRes.amount;
                int steps = Mathf.Clamp(totalResourcesAmount / dropRate, 1, int.MaxValue);
                int amountPerStep = dropRes.amount / steps;

                Currency currency = CurrenciesController.GetCurrency(dropRes.currency);

                for (int d = 0; d < steps; d++)
                {
                    totalResourcesAmount -= amountPerStep;

                    if (d == steps - 1)
                    {
                        amountPerStep += totalResourcesAmount;
                    }

                    GameObject dropObj = currency.Data.DropResPool.GetPooledObject();
                    ResourceDropBehavior dropResource = dropObj.GetComponent<ResourceDropBehavior>();

                    dropObj.transform.position = transform.position.AddToY(dropResource.VerticalOffset);

                    dropResource.Initialise(amountPerStep).SetDropAnimation(dropAnimation).SetDisableTime(30).Throw(transform, transform.position, lastHitSourcePosition);

                    if (lastHitter != null && lastHitter.AutoPickResources)
                    {
                        dropResource.ActivateAutoPick(0.5f, lastHitter);
                    }
                }
            }
        }

        #endregion

        private Vector3 lastHitSourcePosition;
        private IResourcePicker lastHitter;

        public void GetHit(Vector3 hitSourcePosition, bool drop = true, IHitter hitter = null)
        {
            lastHitSourcePosition = hitSourcePosition;
            lastHitter = hitter;
            TakeDamage(new DamageSource(1, PlayerBehavior.GetBehavior()), PlayerBehavior.Position, true);
        }

        protected virtual void FlashOnHit()
        {
            if (!flashOnHit)
                return;

            bodyRenderer.material.DOColor(_EmissionColor, flashEmissionColor, flashDuration / 2f).OnComplete(() =>
            {
                bodyRenderer.material.DOColor(_EmissionColor, Color.black, flashDuration / 2f);
            });
        }

        public bool IsHittable() => true;

        public virtual void ActivateInteractionAnimation(Interaction interactionAnimations)
        {
            interactionAnimations.Activate(InteractionAnimationType);
        }

        public void Unload()
        {
            Health.ForceHide();

            stateMachine.StopMachine();
            gameObject.SetActive(false);
        }
    }
}