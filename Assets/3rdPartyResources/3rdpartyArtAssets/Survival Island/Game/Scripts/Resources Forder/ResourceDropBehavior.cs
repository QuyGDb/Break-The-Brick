using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class ResourceDropBehavior : MonoBehaviour, IWorldItemCollector
    {
        [SerializeField] CurrencyType currencyType;
        public CurrencyType CurrencyType => currencyType;

        [SerializeField] float verticalOffset;
        public float VerticalOffset => verticalOffset;

        protected DropAnimation dropAnimation;

        protected WorldItemCollectorCase collectorCase;

        protected bool isFlying;
        public bool IsFlying => isFlying;

        protected bool isPicked;
        public bool IsPicked => isPicked;

        private bool isAutoPickupActive;
        private float autoPickupDelay;
        private IResourcePicker autoPickupTarget;
        private TweenCase autoPickupTweenCase;

        private Coroutine animationCoroutine;

        private int dropAmount;
        public int DropAmount => dropAmount;

        public event SimpleCallback ResourcePicked;

        public virtual ResourceDropBehavior Initialise(int dropAmount)
        {
            this.dropAmount = dropAmount;

            isFlying = false;
            isPicked = false;

            isAutoPickupActive = false;
            autoPickupTarget = null;

            dropAnimation = null;

            transform.localScale = Vector3.one;

            return this;
        }

        public ResourceDropBehavior SetDisableTime(float disableTime)
        {
            if (collectorCase != null)
                collectorCase.MarkAsDisabled();

            collectorCase = WorldItemCollector.RegisterGameObject(this, disableTime);

            return this;
        }

        public ResourceDropBehavior SetDropAnimation(DropAnimation dropAnimation)
        {
            this.dropAnimation = dropAnimation;

            return this;
        }

        public virtual ResourceDropBehavior ActivateAutoPick(float delay, IResourcePicker target)
        {
            isAutoPickupActive = target != null;

            autoPickupDelay = delay;
            autoPickupTarget = target;

            return this;
        }

        public virtual ResourceDropBehavior Throw(Transform dropParent, Vector3 startPosition, Vector3 hitPosition)
        {
            if (isFlying) return this;

            isFlying = true;

            transform.position = startPosition;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            SimpleCallback onAnimationFinished = () =>
            {
                isFlying = false;

                if (AudioController.AudioClips.resourcesDropInStorageSound != null)
                {
                    var gameData = GameController.Data;

                    gameData.StorageSoundHandler.Play(AudioController.AudioClips.resourcesDropInStorageSound, transform.position);
                }

                if (isAutoPickupActive)
                {
                    autoPickupTweenCase = new DropTweenCase(transform, autoPickupTarget.SnappingTransform, new Vector3(0, 0.5f, 0), transform.localScale);
                    autoPickupTweenCase.SetEasing(Ease.Type.SineIn);
                    autoPickupTweenCase.SetDuration(0.2f);
                    autoPickupTweenCase.SetDelay(autoPickupDelay);
                    autoPickupTweenCase.OnComplete(() =>
                    {
                        PerformPick(autoPickupTarget);
                    });
                    autoPickupTweenCase.StartTween();
                }
            };

            DropAnimation.Data dropAnimationData = new DropAnimation.Data()
            {
                DropTransform = transform,
                VerticalOffset = verticalOffset,
                HitPosition = hitPosition,
                DropParentTransform = dropParent,
                StartPosition = startPosition
            };

            IEnumerator dropEnumerator = null;
            if (dropAnimation != null)
            {
                dropEnumerator = dropAnimation.AnimationEnumerator(dropAnimationData, onAnimationFinished);
            }
            else
            {
                dropEnumerator = DefaultDropAnimation(dropAnimationData, onAnimationFinished);
            }

            animationCoroutine = StartCoroutine(dropEnumerator);

            return this;
        }

        public void PerformPick(IResourcePicker target)
        {
            if (isPicked) return;

            if(target != null)
            {
                target.OnResourcePickPerformed(this);
            }
        }

        public virtual void OnObjectPicked(IResourcePicker target, bool playAnimation = false)
        {
            if (isPicked) return;

            isPicked = true;

            autoPickupTweenCase.KillActive();

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);

                animationCoroutine = null;
            }

            if (AudioController.AudioClips.resourcesPickUpFromStorageSound != null)
            {
                var gameData = GameController.Data;

                gameData.StorageSoundHandler.Play(AudioController.AudioClips.resourcesPickUpFromStorageSound, transform.position);
            }

            ResourcePicked?.Invoke();
            ResourcePicked = null;

            if (playAnimation)
            {
                DropTweenCase dropTweenCase = new DropTweenCase(transform, target.SnappingTransform, new Vector3(0, 1, 0), Vector3.zero);
                dropTweenCase.SetEasing(Ease.Type.SineIn);
                dropTweenCase.SetDuration(0.2f);
                dropTweenCase.OnComplete(() => 
                {
                    Unload();
                });
                dropTweenCase.StartTween();
            }
            else
            {
                Unload();
            }
        }

        public void OnWorldItemCollected()
        {
            collectorCase = null;

            Unload();
        }

        public virtual void Unload()
        {
            autoPickupTweenCase.KillActive();

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);

                animationCoroutine = null;
            }

            if (collectorCase != null)
            {
                collectorCase.MarkAsDisabled();
                collectorCase = null;
            }

            gameObject.SetActive(false);
        }

        private IEnumerator DefaultDropAnimation(DropAnimation.Data animationData, SimpleCallback onAnimationFinished)
        {
            int counter = 0;
            Vector3 endPos;

            Transform dropParent = animationData.DropParentTransform;
            Vector3 startPosition = animationData.StartPosition;

            float terrainHeight = dropParent.position.y + 100;

            do
            {
                endPos = dropParent.position.GetRandomPositionAroundObject(Random.Range(1.0f, 1.2f)).AddToY(animationData.VerticalOffset);
                counter++;

                if (NavMesh.SamplePosition(endPos, out var hit, 0.1f, NavMesh.AllAreas) && Mathf.Abs(hit.position.y - endPos.y) < 0.5f)
                {
                    terrainHeight = hit.position.y;
                }
            }
            while (Mathf.Abs(terrainHeight - dropParent.position.y) >= 0.5f && counter < 20);

            Vector3 keyPos = ((startPosition + endPos) / 2).SetY(startPosition.y + (startPosition.y - endPos.y) * 0.5f);

            float duration = 0.5f;
            float time = 0f;

            while (time < duration)
            {
                yield return null;

                time += Time.deltaTime;

                transform.position = Bezier.EvaluateQuadratic(startPosition, keyPos, endPos, time / duration);
            }

            transform.position = endPos;

            onAnimationFinished?.Invoke();
        }

        private class DropTweenCase : TweenCase
        {
            private Transform dropTransform;
            private Transform targetTransform;

            private Vector3 offset;

            private Vector3 startPosition;

            private Vector3 startScale;
            private Vector3 targetScale;

            public DropTweenCase(Transform transform, Transform targetTransform, Vector3 targetOffset, Vector3 targetScale)
            {
                this.targetTransform = targetTransform;
                this.targetScale = targetScale;

                parentObject = transform.gameObject;

                dropTransform = transform;

                startPosition = dropTransform.position;
                startScale = dropTransform.localScale;

                offset = targetOffset;
            }

            public override void DefaultComplete()
            {
                dropTransform.position = targetTransform.position + offset;
                dropTransform.localScale = targetScale;
            }

            public override void Invoke(float deltaTime)
            {
                float interpolatedState = Interpolate(state);

                dropTransform.position = Vector3.Lerp(startPosition, targetTransform.position + offset, interpolatedState);
                dropTransform.localScale = Vector3.Lerp(startScale, targetScale, interpolatedState);
            }

            public override bool Validate()
            {
                return parentObject != null;
            }
        }
    }
}