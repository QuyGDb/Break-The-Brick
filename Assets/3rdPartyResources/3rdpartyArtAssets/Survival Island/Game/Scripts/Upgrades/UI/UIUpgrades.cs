using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public class UIUpgrades : UIPage, IUpgradePanel
    {
        private readonly Vector2 DEFAULT_POSITION = new Vector2(0, 95);
        private readonly Vector2 HIDE_POSITION = new Vector2(0, -2000);

        [SerializeField] VerticalLayoutGroup verticalLayoutGroup;

        [Space]
        [SerializeField] Image fadeImage;
        [SerializeField] RectTransform panelRectTransform;
        [SerializeField] RectTransform viewportRectTransform;
        [SerializeField] RectTransform contentTransform;
        public Transform ContentTransform => contentTransform;
        [SerializeField] Button closeButton;

        [Space]
        [SerializeField] Color defaultColor;
        [SerializeField] Color highlightedColor;
        public Color DefaultColor => defaultColor;
        public Color HighlightedColor => highlightedColor;

        [Header("Upgrade")]
        [SerializeField] GameObject upgradeUIPrefab;
        public GameObject UpgradeUIPrefab => upgradeUIPrefab;

        [Header("Scroll Rect")]
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] VerticalLayoutGroup content;

        private int SelectedItemId { get; set; }
        private UpgradeUIPanel SelectedItem => UpgradeUIPanels[SelectedItemId];

        private UIGame mainPage;

        // Upgrades

        private UpgradePanelHelper upgradeHelper;

        public List<UpgradeUIPanel> UpgradeUIPanels => upgradeHelper.UpgradeUIPanels;
        public Pool UpgradesUIPool => upgradeHelper.UpgradesUIPool;
        public List<IUpgrade> Upgrades => upgradeHelper.Upgrades;

        private bool showAllAfterUpgrade;
        public bool ShowAllAfterUpgrade { get => showAllAfterUpgrade; set => showAllAfterUpgrade = value; }

        private float panelHeight;
        private float viewportHeight;

        TweenCase scrollCase;

        public override void Initialise()
        {
            upgradeHelper = new UpgradePanelHelper(this);
            //upgradeHelper.AddUpgrades(new List<IUpgrade>(UpgradesController.ActiveUpgrades.Convert((upgrade) => (IUpgrade)upgrade)));

            mainPage = UIController.GetPage<UIGame>();

            closeButton.onClick.AddListener(OnCloseButtonClicked);

            panelHeight = panelRectTransform.sizeDelta.y;
            viewportHeight = panelRectTransform.sizeDelta.y + viewportRectTransform.sizeDelta.y;
        }

        public void RegisterUpgrades(List<IUpgrade> upgrades)
        {
            upgradeHelper.AddUpgrades(upgrades);
        }

        public override void PlayShowAnimation()
        {
            GlobalUpgradesEventsHandler.OnUpgraded += upgradeHelper.OnUpgraded;

            mainPage.Joystick.HideVisuals();

            fadeImage.color = fadeImage.color.SetAlpha(0.0f);
            fadeImage.DOFade(0.25f, 0.5f);

            // Reset panel position
            panelRectTransform.anchoredPosition = HIDE_POSITION;
            panelRectTransform.DOAnchoredPosition(DEFAULT_POSITION, 0.5f).SetEasing(Ease.Type.CircOut);

            upgradeHelper.Show();

            upgradeHelper.Redraw(true);

            SelectedItemId = 0;

            if (UpgradeUIPanels.Count > 0 && Control.InputType != InputType.UIJoystick)
            {
                UpgradeUIPanels[0].OnSelect();
            }

            var contentSize = (UpgradeUIPanels[0].Height + verticalLayoutGroup.spacing) * UpgradeUIPanels.Count;

            if(contentSize < viewportHeight)
            {
                panelRectTransform.sizeDelta = panelRectTransform.sizeDelta.SetY(panelHeight - viewportHeight + contentSize);
            } 
            else
            {
                panelRectTransform.sizeDelta = panelRectTransform.sizeDelta.SetY(panelHeight);
            }

            contentTransform.sizeDelta = contentTransform.sizeDelta.SetY(contentSize);

            Control.DisableMovementControl();

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Upgrades);

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            GlobalUpgradesEventsHandler.OnUpgraded -= upgradeHelper.OnUpgraded;
            
            mainPage.Joystick.ShowVisuals();
            
            fadeImage.DOFade(0, 0.5f);
            panelRectTransform.DOAnchoredPosition(HIDE_POSITION, 0.5f).SetEasing(Ease.Type.CircIn).OnComplete(delegate
            {
                UIController.OnPageClosed(this);
            });

            for(int i = 0; i < UpgradeUIPanels.Count; i++)
            {
                UpgradeUIPanels[i].Disable();
            }

            Control.EnableMovementControl();
        }

        private void Update()
        {
            if (IsPageDisplayed && Control.InputType == InputType.Gamepad)
            {
                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.B))
                {
                    closeButton.ClickButton();

                    UIGamepadButton.DisableTag(UIGamepadButtonTag.Upgrades);
                    UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
                }

                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DDown))
                {
                    if (SelectedItemId < UpgradeUIPanels.Count - 1)
                    {
                        UpgradeUIPanels[SelectedItemId].OnDeselect();

                        SelectedItemId++;

                        UpgradeUIPanels[SelectedItemId].OnSelect();

                        scrollCase.KillActive();

                        if (scrollRect.IsTargetLowerThanViewport(SelectedItem.Rect))
                        {
                            scrollCase = scrollRect.DoSnapTargetBottom(SelectedItem.Rect, 0.2f, 0, 0).SetEasing(Ease.Type.SineOut);
                        }
                    }
                }
                else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DUp))
                {
                    if (SelectedItemId > 0)
                    {
                        UpgradeUIPanels[SelectedItemId].OnDeselect();

                        SelectedItemId--;

                        UpgradeUIPanels[SelectedItemId].OnSelect();

                        scrollCase.KillActive();

                        if (scrollRect.IsTargetHigherThanViewport(SelectedItem.Rect))
                        {
                            scrollCase = scrollRect.DoSnapTargetTop(SelectedItem.Rect, 0.2f, 0, content.padding.top).SetEasing(Ease.Type.SineOut);
                        }
                    }
                }
            }
        }

        public void ResetUpgrades()
        {
            upgradeHelper.Reset();
        }

        #region Buttons
        public void OnCloseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.HidePage<UIUpgrades>();
        }
        #endregion
    }
}
