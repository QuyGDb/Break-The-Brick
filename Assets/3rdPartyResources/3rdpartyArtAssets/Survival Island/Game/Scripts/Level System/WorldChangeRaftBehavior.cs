using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Animator))]
    public sealed class WorldChangeRaftBehavior : WorldChangeSpecialBehavior
    {
        [SerializeField] Transform playerHolderTransform;
        [SerializeField] ParticleSystem splashParticleSystem;

        [Space]
        [SerializeField] float worldChangeEventDelay = 0.5f;

        private Animator raftAnimator;

        private void Awake()
        {
            raftAnimator = GetComponent<Animator>();
        }

        public override void OnGroundTileOpened(bool immediately)
        {
            gameObject.SetActive(true);

            if (immediately)
            {
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.localScale = Vector3.zero;
                transform.DOScale(1.0f, 0.3f);
            }
        }

        public override void OnWorldChanged(SimpleCallback worldChangeCallback)
        {
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            playerBehavior.Disable();
            playerBehavior.transform.SetParent(playerHolderTransform);
            playerBehavior.transform.ResetLocal();
            playerBehavior.PlayerGraphics.Animator.Play("Sitting", -1, 0);

            raftAnimator.Play("Move");

            splashParticleSystem.Play();

            Tween.DelayedCall(worldChangeEventDelay, worldChangeCallback);
        }
    }
}