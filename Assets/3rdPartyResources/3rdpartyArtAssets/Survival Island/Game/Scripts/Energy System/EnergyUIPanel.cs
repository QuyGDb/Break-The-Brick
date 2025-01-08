using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class EnergyUIPanel : MonoBehaviour
    {
        [SerializeField] Image backFillImage;
        [SerializeField] Image frontFillImage;

        [Space]
        [SerializeField] Color lowEnergyColor;
        [SerializeField] DuoFloat lowEnergyColorTransitionStartEnd;
        [SerializeField] Animator backAnimatorRef;
        [SerializeField, Range(0f, 1f)] float animationStrength = 1f;
        private TweenCase maskTweenCase;
        private Color defaultColor;

        public void Initialise()
        {
            defaultColor = frontFillImage.color;

            EnergyController.OnEnergyChanged += OnEnergyChanged;

            OnEnergyChanged();
        }

        public void OnEnergyChanged()
        {
            backFillImage.fillAmount = (float)EnergyController.EnergyPoints / EnergyController.Data.MaxEnergyPoints;

            maskTweenCase.KillActive();

            maskTweenCase = frontFillImage.DOFillAmount(backFillImage.fillAmount, 0.3f).SetEasing(Ease.Type.QuintIn);

            if (backFillImage.fillAmount > lowEnergyColorTransitionStartEnd.secondValue)
            {
                frontFillImage.color = defaultColor;
                backAnimatorRef.SetLayerWeight(1, 0f * animationStrength);
            }
            else if (backFillImage.fillAmount > lowEnergyColorTransitionStartEnd.firstValue)
            {
                float lerpValue = (backFillImage.fillAmount - lowEnergyColorTransitionStartEnd.firstValue) / (lowEnergyColorTransitionStartEnd.secondValue - lowEnergyColorTransitionStartEnd.firstValue);

                frontFillImage.color = Color.Lerp(lowEnergyColor, defaultColor, lerpValue);
                backAnimatorRef.SetLayerWeight(1, (1f - lerpValue) * animationStrength);
            }
            else
            {
                frontFillImage.color = lowEnergyColor;
                backAnimatorRef.SetLayerWeight(1, 1f * animationStrength);
            }
        }

    }
}