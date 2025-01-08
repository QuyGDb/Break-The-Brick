using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using System.Collections.Generic;

namespace Watermelon
{
    public class HealthbarBehaviour : MonoBehaviour
    {
        [SerializeField] Transform healthBarTransform;
        public Transform HealthBarTransform => healthBarTransform;

        [SerializeField] Vector3 healthbarOffset;
        public Vector3 HealthbarOffset => healthbarOffset;

        [Space]
        [SerializeField] CanvasGroup healthBarCanvasGroup;
        [SerializeField] Image healthFillImage;
        [SerializeField] Image maskFillImage;
        [SerializeField] TextMeshProUGUI healthText;

        [Space]
        [SerializeField] Color standartHealthbarColor;

        private IHealth targetHealth;
        private Transform parentTransform;
        private bool showAlways;

        private bool isInitialised;
        private bool isPanelActive;

        private bool isDisabled;
        public bool IsDisabled => isDisabled;

        private TweenCase maskTweenCase;
        private TweenCase panelTweenCase;
        private TweenCase fadeTweenCase;

        public void Initialise(Transform parentTransform, IHealth targetHealth, bool showAlways)
        {
            this.targetHealth = targetHealth;
            this.parentTransform = parentTransform;
            this.showAlways = showAlways;

            isDisabled = !showAlways;
            isPanelActive = false;

            // Reset bar parent
            //healthBarTransform.SetParent(null);
            healthBarTransform.localPosition = HealthbarOffset;
            healthBarTransform.gameObject.SetActive(true);

            healthFillImage.color = standartHealthbarColor;

            // Redraw health
            //RedrawHealth();

            // Show or hide healthbar
            healthBarCanvasGroup.alpha = showAlways ? 1.0f : 0.0f;

            isInitialised = true;
        }

        public void FollowUpdate()
        {
            if (isInitialised)
            {
                //healthBarTransform.position = parentTransform.position + HealthbarOffset;
                healthBarTransform.rotation = Camera.main.transform.rotation;
            }
        }

        public void OnHealthChanged()
        {
            if (isDisabled)
                return;

            if (targetHealth == null)
                return;

            healthFillImage.fillAmount = targetHealth.CurrentHealth / targetHealth.MaxHealth;

            maskTweenCase.KillActive();

            maskTweenCase = maskFillImage.DOFillAmount(healthFillImage.fillAmount, 0.3f).SetEasing(Ease.Type.QuintIn);

            if (!showAlways)
            {
                if (healthFillImage.fillAmount < 1.0f && !isPanelActive)
                {
                    isPanelActive = true;

                    panelTweenCase.KillActive();

                    panelTweenCase = healthBarCanvasGroup.DOFade(1.0f, 0.5f);
                }
                else if (healthFillImage.fillAmount >= 1.0f && isPanelActive)
                {
                    isPanelActive = false;

                    panelTweenCase.KillActive();

                    panelTweenCase = healthBarCanvasGroup.DOFade(0.0f, 0.5f);
                }
            }
        }

        public void DisableBar()
        {
            if (isDisabled)
                return;

            isDisabled = true;

            fadeTweenCase.KillActive();

            fadeTweenCase = healthBarCanvasGroup.DOFade(0.0f, 0.3f).OnComplete(delegate
            {
                healthBarTransform.gameObject.SetActive(false);
            });
        }

        public void EnableBar()
        {
            if (!isDisabled)
                return;

            isDisabled = false;

            healthBarTransform.gameObject.SetActive(true);

            fadeTweenCase.KillActive();
            fadeTweenCase = healthBarCanvasGroup.DOFade(1.0f, 0.3f);
        }

        public void RedrawHealth()
        {
            healthFillImage.fillAmount = targetHealth.CurrentHealth / targetHealth.MaxHealth;
            maskFillImage.fillAmount = healthFillImage.fillAmount;
        }

        public void ForceDisable()
        {
            isDisabled = true;

            fadeTweenCase.KillActive();

            healthBarTransform.gameObject.SetActive(false);
            healthBarCanvasGroup.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            isDisabled = true;

            Destroy(healthBarTransform.gameObject);
        }
    }

    public interface IHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
    }

}