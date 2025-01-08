using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Flying Resource Bezier Animation", menuName = "Content/Flying Resource/Flying Resource Bezier Animation")]
    public class FlyingResourceBezierAnimation : FlyingResourceAnimation
    {
        [SerializeField] float moveDuration = 0.7f;
        [SerializeField] Ease.Type moveEasing = Ease.Type.SineIn;
        [SerializeField] DuoFloat moveUpOffset = new DuoFloat(1, 1);
        [SerializeField] DuoFloat moveRightOffset = new DuoFloat(0, 0);

        [Space]
        [SerializeField] float scaleInDuration = 0.2f;
        [SerializeField] Ease.Type scaleInEasing = Ease.Type.SineOut;
        [SerializeField] float scaleOutDuration = 0.1f;
        [SerializeField] Ease.Type scaleOutEasing = Ease.Type.QuadIn;

        public override TweenCaseCollection StartAnimation(FlyingResourceBehavior flyingResourceBehavior, Vector3 destinationPoint, SimpleCallback onAnimationCompleted)
        {
            TweenCaseCollection animationTweenCollection = new TweenCaseCollection();

            // Animating flying resource's movement to this point
            animationTweenCollection.AddTween(flyingResourceBehavior.transform.DOBezierMove(destinationPoint, moveUpOffset.Random(), moveRightOffset.Random(), 0f, moveDuration).SetEasing(moveEasing));
            animationTweenCollection.AddTween(flyingResourceBehavior.transform.DOScale(1f, scaleInDuration).SetEasing(scaleInEasing));
            animationTweenCollection.AddTween(flyingResourceBehavior.transform.DOScale(0f, scaleOutDuration, moveDuration - scaleOutDuration).SetEasing(scaleOutEasing).OnComplete(() =>
            {
                onAnimationCompleted?.Invoke();
            }));

            return animationTweenCollection;
        }
    }
}