using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class UIFadeAnimation
    {
        [SerializeField] CanvasGroup fadeCanvasGroup;

        private TweenCase fadeTweenCase;

        public void Show(float duration = 0.4f, float delay = 0f, bool immediately = false, SimpleCallback onCompleted = null)
        {
            fadeTweenCase.KillActive();

            if (immediately)
            {
                fadeCanvasGroup.alpha = 1f;
                onCompleted?.Invoke();

                return;
            }

            fadeCanvasGroup.alpha = 0f;
            fadeTweenCase = fadeCanvasGroup.DOFade(1, duration, delay, unscaledTime: true).OnComplete(() =>
            {
                onCompleted?.Invoke();
            });
        }

        public void Hide(float duration = 0.4f, float delay = 0f, bool immediately = false, SimpleCallback onCompleted = null)
        {
            fadeTweenCase.KillActive();

            if (immediately)
            {
                fadeCanvasGroup.alpha = 0f;
                onCompleted?.Invoke();

                return;
            }

            fadeCanvasGroup.alpha = 1f;
            fadeTweenCase = fadeCanvasGroup.DOFade(0, duration, delay, unscaledTime: true).OnComplete(() =>
            {
                onCompleted?.Invoke();
            });
        }
    }
}
