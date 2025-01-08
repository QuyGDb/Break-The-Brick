using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [System.Serializable]
    public class UIWorldChangePopUp
    {
        [SerializeField] GameObject panelObject;
        [SerializeField] UIFadeAnimation fadeAnimation;
        [SerializeField] UIScaleAnimation panelBackScaleAnimation;

        [SerializeField] Button continueButton;
        [SerializeField] Button exitButton;
        [SerializeField] Button bigExitButton;

        private SimpleCallback callback;

        public void Initialise()
        {
            continueButton.onClick.AddListener(ContinueButton);
            exitButton.onClick.AddListener(Hide);
            bigExitButton.onClick.AddListener(Hide);
        }

        public void Show(SimpleCallback callback)
        {
            this.callback = callback;

            panelObject.SetActive(true);
            fadeAnimation.Show();
            panelBackScaleAnimation.Show();

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Popup);
        }

        public void Hide()
        {
            fadeAnimation.Hide();
            panelBackScaleAnimation.Hide(onCompleted: () =>
            {
                panelObject.SetActive(false);

                UIGamepadButton.DisableAllTags();
                UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
            });
        }

        public void ContinueButton()
        {
            panelObject.SetActive(false);
            fadeAnimation.Hide(immediately: true);
            panelBackScaleAnimation.Hide(immediately: true);

            callback?.Invoke();
        }
    }
}