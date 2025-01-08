using UnityEngine;

namespace Watermelon
{
    public class FlyingResourceBehavior : MonoBehaviour
    {
        [SerializeField] CurrencyType resourceType;
        public CurrencyType ResourceType => resourceType;

        public int Amount { get; private set; }

        private FlyingResourceAnimation flyingResourceAnimation;
        private TweenCaseCollection animationTweenCollection;

        private static FlyingResourceAnimation defaultFlyingAnimation;

        public void InitAtPosition(Vector3 spawnPosition, int amount)
        {
            Amount = amount;

            transform.position = spawnPosition;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.zero;

            flyingResourceAnimation = defaultFlyingAnimation;
        }

        public void SetCustomAnimation(FlyingResourceAnimation animation)
        {
            if(animation != null)
            {
                flyingResourceAnimation = animation;
            }
        }

        public TweenCase PlayAnimation(Vector3 destinationPoint, SimpleCallback onCompleted)
        {
            if(flyingResourceAnimation != null)
            {
                animationTweenCollection = flyingResourceAnimation.StartAnimation(this, destinationPoint, onCompleted);

                return animationTweenCollection.TweenCases[^1];
            }

            animationTweenCollection = new TweenCaseCollection();

            // Animating flying resource's movement to this point
            animationTweenCollection.AddTween(transform.DOMove(destinationPoint, 0.7f).SetEasing(Ease.Type.SineIn));
            animationTweenCollection.AddTween(transform.DOScale(1f, 0.2f).SetEasing(Ease.Type.SineOut));
            animationTweenCollection.AddTween(transform.DOScale(0f, 0.1f, 0.6f).SetEasing(Ease.Type.QuadIn).OnComplete(() =>
            {
                onCompleted?.Invoke();
            }));

            return animationTweenCollection.TweenCases[^1];
        }

        public void Unload()
        {
            animationTweenCollection.KillActive();
            animationTweenCollection = null;
        }

        public static void SetDefaultFlyingAnimation(FlyingResourceAnimation animation)
        {
            defaultFlyingAnimation = animation;
        }
    }
}