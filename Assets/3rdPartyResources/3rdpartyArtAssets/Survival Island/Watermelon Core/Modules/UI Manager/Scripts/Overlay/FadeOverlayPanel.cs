using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Canvas))]
    public class FadeOverlayPanel : MonoBehaviour, IOverlayPanel
    {
        [SerializeField] Ease.Type showEasingType;
        [SerializeField] Ease.Type hideEasingType;

        [Space]
        [SerializeField] GameObject loadingObject;

        private CanvasGroup canvasGroup;
        private TweenCase tweenCase;
        private Canvas canvas;

        public void Initialise()
        {
            canvas = gameObject.GetComponent<Canvas>();

            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.0f;
        }

        public void Show(float duration, SimpleCallback onCompleted)
        {
            tweenCase.KillActive();
            tweenCase = canvasGroup.DOFade(1.0f, duration, unscaledTime: true).SetEasing(showEasingType).OnComplete(onCompleted);
        }

        public void Hide(float duration, SimpleCallback onCompleted)
        {
            tweenCase.KillActive();
            tweenCase = canvasGroup.DOFade(0.0f, duration, unscaledTime: true).SetEasing(hideEasingType).OnComplete(onCompleted);
        }

        public void Clear()
        {
            tweenCase.KillActive();
        }

        public void SetState(bool state)
        {
            canvas.enabled = state;
        }

        public void SetLoadingState(bool state)
        {
            if (loadingObject != null)
            {
                loadingObject.gameObject.SetActive(state);
            }
        }
    }
}
