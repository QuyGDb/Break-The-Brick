using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIPause : UIPage
    {
        [SerializeField] RectTransform panelRect;
        [SerializeField] float panelHiddenY;
        [SerializeField] float panelShownY;
        [SerializeField] VerticalLayoutGroup content;

        [Space]
        [SerializeField] Image blackFade;
        [SerializeField] Button closeButton;

        [SerializeField] List<PauseItem> items;

        private int SelectedItemId { get; set; }
        private PauseItem SelectedItem => items[SelectedItemId];

        public override void Initialise()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);

            Tween.NextFrame(() =>
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] == null)
                    {
                        items.RemoveAt(i);
                        i--;
                    }
                }

                Tween.NextFrame(() => {
                    var contentRect = content.transform as RectTransform;
                    panelRect.sizeDelta = panelRect.sizeDelta.SetY(contentRect.sizeDelta.y + 165);
                });
            }, 2);
        }

        public override void PlayHideAnimation()
        {
            Tween.DoFloat(0, 1, 0.3f, (value) => { Time.timeScale = Mathf.Clamp01(value); }, unscaledTime: true);

            blackFade.DOFade(0, 0.3f, unscaledTime: true);

            panelRect.DOAnchoredPosition(Vector2.up * panelHiddenY, 0.3f, unscaledTime: true).SetEasing(Ease.Type.SineIn).OnComplete(() =>
            {
                UIController.OnPageClosed(this);

                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] != null) items[i].Deselect();
                }

                Control.EnableMovementControl();
            });

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
        }

        public override void PlayShowAnimation()
        {
            Tween.DoFloat(1, 0, 0.3f, (value) => { Time.timeScale = Mathf.Clamp01(value); }, unscaledTime: true);

            blackFade.DOFade(0.6f, 0.3f, unscaledTime: true);

            panelRect.anchoredPosition = Vector2.up * panelHiddenY;

            panelRect.DOAnchoredPosition(Vector2.up * panelShownY, 0.3f, unscaledTime: true).SetEasing(Ease.Type.SineOut);

            SelectedItemId = 0;
            SelectedItem.Select();

            UIController.OnPageOpened(this);

            Control.DisableMovementControl();

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Popup);


        }

        private void Update()
        {
            if (IsPageDisplayed && Control.InputType == InputType.Gamepad)
            {
                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.B))
                {
                    Time.timeScale = 0.1f;
                    closeButton.ClickButton();

                    UIGamepadButton.DisableTag(UIGamepadButtonTag.Popup);
                    UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
                }

                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DDown))
                {
                    if (SelectedItemId < items.Count - 1)
                    {
                        SelectedItem.Deselect();

                        SelectedItemId++;
                        SelectedItem.Select();
                    }
                }
                else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DUp))
                {
                    if (SelectedItemId > 0)
                    {
                        SelectedItem.Deselect();

                        SelectedItemId--;
                        SelectedItem.Select();
                    }
                }
            }
        }

        private void OnCloseButtonClicked()
        {
            UIController.HidePage<UIPause>();

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}
