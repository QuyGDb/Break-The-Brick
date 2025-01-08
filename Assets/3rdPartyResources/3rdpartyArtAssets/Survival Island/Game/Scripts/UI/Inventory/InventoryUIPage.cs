using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public class InventoryUIPage : UIPage
    {
        [SerializeField] RectTransform panelRect;
        [SerializeField] float panelHiddenY;
        [SerializeField] float panelShownY;
        [SerializeField] GameObject tutorialObject;
        [SerializeField] GameObject emptyInventoryPanel;

        [Header("Item")]
        [SerializeField] GameObject itemPrefab;
        [SerializeField] RectTransform itemsParent;

        private PoolGeneric<InventoryUIItem> itemsPool;

        [Header("Buttons")]
        [SerializeField] Button closeButton;
        [SerializeField] Button sellAllButton;
        [SerializeField] Button sellAllAdButton;

        [Header("Scroll Rect")]
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] VerticalLayoutGroup content;

        private Currency[] currencies;

        private List<InventoryUIItem> items;
        private static InventoryUIPage instance;

        private int SelectedItemId { get; set; }
        private InventoryUIItem SelectedItem => items[SelectedItemId];

        private TweenCase scrollCase;

        public override void Initialise()
        {
            instance = this;

            closeButton.onClick.AddListener(OnCloseButtonClicked);
            sellAllButton.onClick.AddListener(OnSellAllButtonClicked);
            sellAllAdButton.onClick.AddListener(OnSellAllAdButtonClicked);

            itemsPool = new PoolGeneric<InventoryUIItem>(new PoolSettings
            {
                name = "Inventory UI Item",
                singlePoolPrefab = itemPrefab,
                size = 6,
                objectsContainer = itemsParent,
                autoSizeIncrement = true,
                type = Pool.PoolType.Single
            });

            currencies = System.Array.FindAll(CurrenciesController.Currencies, x => x.Data.UseInventory);
        }

        public override void PlayHideAnimation()
        {
            panelRect.DOAnchoredPosition(Vector2.up * panelHiddenY, 0.3f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
            {
                UIController.OnPageClosed(this);

                for (int i = 0; i < items.Count; i++)
                {
                    items[i].OnDeselect();
                    items[i].gameObject.SetActive(false);
                }

                items.Clear();

                Control.EnableMovementControl();
            });
        }

        public override void PlayShowAnimation()
        {
            InitData();

            CheckIfEmpty();

            Show();

            UIController.OnPageOpened(this);

            Control.DisableMovementControl();

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Inventory);
        }

        private void Show()
        {
            panelRect.anchoredPosition = Vector2.up * panelHiddenY;

            panelRect.DOAnchoredPosition(Vector2.up * panelShownY, 0.3f).SetEasing(Ease.Type.SineOut);
        }

        private void Update()
        {
            if (IsPageDisplayed && Control.InputType == InputType.Gamepad)
            {
                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.B))
                {
                    closeButton.ClickButton();

                    UIGamepadButton.DisableTag(UIGamepadButtonTag.Inventory);
                    UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
                }

                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DDown))
                {
                    if(SelectedItemId < items.Count - 1)
                    {
                        items[SelectedItemId].OnDeselect();

                        SelectedItemId++;

                        items[SelectedItemId].OnSelect();

                        scrollCase.KillActive();

                        if (scrollRect.IsTargetLowerThanViewport(SelectedItem.Rect))
                        {
                            scrollCase = scrollRect.DoSnapTargetBottom(SelectedItem.Rect, 0.2f, 0, 0).SetEasing(Ease.Type.SineOut);
                        }
                    }
                } else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DUp))
                {
                    if (SelectedItemId > 0)
                    {
                        items[SelectedItemId].OnDeselect();

                        SelectedItemId--;

                        items[SelectedItemId].OnSelect();

                        scrollCase.KillActive();

                        if (scrollRect.IsTargetHigherThanViewport(SelectedItem.Rect))
                        {
                            scrollCase = scrollRect.DoSnapTargetTop(SelectedItem.Rect, 0.2f, 0, content.padding.top).SetEasing(Ease.Type.SineOut);
                        }
                    }
                }
            }
        }

        public void ActivateTutorial(bool state)
        {
            tutorialObject.SetActive(state);
        }

        private void InitData()
        {
            items = new List<InventoryUIItem>();

            for (int i = 0; i < currencies.Length; i++)
            {
                if (currencies[i].Amount > 0)
                {
                    var item = itemsPool.GetPooledComponent();

                    item.Init(currencies[i]);

                    items.Add(item);
                }
            }

            if(items.Count > 0 && Control.InputType != InputType.UIJoystick)
            {
                items[0].OnSelect();
            }
        }

        private void OnCloseButtonClicked()
        {
            UIController.HidePage<InventoryUIPage>();

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnSellAllButtonClicked()
        {
            tutorialObject.SetActive(false);

            if (items.Count == 0)
                return;

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            CurrenciesController.Add(CurrencyType.Coins, CalculateAllMoney());

            RemoveAllCurrencies();
        }

        private void OnSellAllAdButtonClicked()
        {
            if (items.Count == 0)
                return;

#if MODULE_MONETIZATION
            AdsManager.ShowRewardBasedVideo((success) =>
            {
                if (success)
                {
                    CurrenciesController.Add(CurrencyType.Coins, CalculateAllMoney() * 3);

                    RemoveAllCurrencies();
                }
            });
#else
            Debug.LogWarning("Monetization module is missing!");

            CurrenciesController.Add(CurrencyType.Coins, CalculateAllMoney() * 3);

            RemoveAllCurrencies();
#endif
        }

        private int CalculateAllMoney()
        {
            int count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (!item.gameObject.activeSelf)
                {
                    items.RemoveAt(i);
                    i--;
                    continue;
                }

                count += CurrenciesController.Get(item.CurrencyType) * item.Currency.Data.MoneyConversionRate;
            }

            return count;
        }

        private void RemoveAllCurrencies()
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                CurrenciesController.Set(item.CurrencyType, 0);

                item.gameObject.SetActive(false);
            }

            items.Clear();

            CheckIfEmpty();
        }

        public void CheckIfEmpty()
        {
            bool isEmpty = true;

            for (int i = 0; i < items.Count; i++)
            {
                // active panel means - panel is not empty
                if (items[i].gameObject.activeSelf)
                {
                    isEmpty = false;
                    break;
                }
            }

            if (isEmpty)
            {
                emptyInventoryPanel.SetActive(true);
                sellAllButton.gameObject.SetActive(false);
            }
            else
            {
                emptyInventoryPanel.SetActive(false);
                sellAllButton.gameObject.SetActive(true);
            }
        }

        public static void OnCurrencyPanelDisabled(InventoryUIItem panel)
        {
            instance.items.Remove(panel);
            if(instance.items.Count > 0)
            {
                if (instance.SelectedItemId > instance.items.Count - 1) instance.SelectedItemId = instance.items.Count - 1;

                instance.SelectedItem.OnSelect();
            }

            instance.CheckIfEmpty();
        }
    }
}