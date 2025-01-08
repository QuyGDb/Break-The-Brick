using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UnlockableToolFloatingText : FloatingTextBaseBehaviour
    {
        [SerializeField] Image iconImage;

        [Space]
        [SerializeField] Vector3 offset;
        [SerializeField] float time;
        [SerializeField] Ease.Type easing;

        [Space]
        [SerializeField] float scaleTime;
        [SerializeField] AnimationCurve scaleAnimationCurve;

        public SimpleCallback OnAnimationCompleted;
        private Vector3 defaultScale;

        private TweenCase scaleTween;
        private TweenCase moveTween;

        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        public void Initialise(UnlockableTool unlockableTool)
        {
            if(unlockableTool != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = unlockableTool.Icon;
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        public override void Activate(string text, Color color)
        {
            textRef.text = text;
            textRef.color = color;

            transform.localScale = Vector3.zero;
            scaleTween = transform.DOScale(defaultScale, scaleTime).SetCurveEasing(scaleAnimationCurve);
            moveTween = transform.DOMove(transform.position + offset, time).SetEasing(easing).OnComplete(delegate
            {
                OnAnimationCompleted?.Invoke();
                gameObject.SetActive(false);
            });
        }

        public void SetText(string text)
        {
            textRef.text = text;
        }

        public void Reset()
        {
            scaleTween.KillActive();
            moveTween.KillActive();
        }
    }
}
