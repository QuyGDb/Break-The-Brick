using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public static class UITweenCases
    {
        #region Extensions
        /// <summary>
        /// Change text font size
        /// </summary>
        public static TweenCase DOPreferredHeight(this LayoutElement tweenObject, float resultValue, float time, float delay = 0, bool unscaledTime = false, UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new LayoutElementPrefferedHeight(tweenObject, resultValue).SetDelay(delay).SetDuration(time).SetUnscaledMode(unscaledTime).SetUpdateMethod(updateMethod).StartTween();
        }

        /// <summary>
        /// Change alpha value of canvas group
        /// </summary>
        public static TweenCase DOFade(this CanvasGroup tweenObject, float resultValue, float time, float delay = 0, bool unscaledTime = false, UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new CanvasGroupFade(tweenObject, resultValue).SetDelay(delay).SetDuration(time).SetUnscaledMode(unscaledTime).SetUpdateMethod(updateMethod).StartTween();
        }
        #endregion

        public class LayoutElementPrefferedHeight : TweenCaseFunction<LayoutElement, float>
        {
            public LayoutElementPrefferedHeight(LayoutElement tweenObject, float resultValue) : base(tweenObject, resultValue)
            {
                parentObject = tweenObject.gameObject;

                startValue = tweenObject.preferredHeight;
            }

            public override bool Validate()
            {
                return parentObject != null;
            }

            public override void DefaultComplete()
            {
                tweenObject.preferredHeight = resultValue;
            }

            public override void Invoke(float deltaTime)
            {
                tweenObject.preferredHeight = Mathf.LerpUnclamped(startValue, resultValue, Interpolate(state));
            }
        }

        public class CanvasGroupFade : TweenCaseFunction<CanvasGroup, float>
        {
            public CanvasGroupFade(CanvasGroup tweenObject, float resultValue) : base(tweenObject, resultValue)
            {
                parentObject = tweenObject.gameObject;

                startValue = tweenObject.alpha;
            }

            public override bool Validate()
            {
                return parentObject != null;
            }

            public override void DefaultComplete()
            {
                tweenObject.alpha = resultValue;
            }

            public override void Invoke(float deltaTime)
            {
                tweenObject.alpha = Mathf.LerpUnclamped(startValue, resultValue, Interpolate(state));
            }
        }
    }
}
