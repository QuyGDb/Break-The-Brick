using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
    public class ChestUIBehavior : MonoBehaviour, IGamepadInteraction
    {
        [Header("Purchase")]
        [SerializeField] TMP_Text priceText;
        [SerializeField] Button purchaseButton;

        [Header("Ads")]
        [SerializeField] Button adsButton;

        private ChestBehavior chestBehavior;

        private Canvas canvas;
        private CanvasGroup canvasGroup;

        private TweenCase stateTweenCase;

        public string Description => "Open Chest";

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            canvas = gameObject.GetComponent<Canvas>();
            canvas.enabled = false;

            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }

        public void Initialise(ChestBehavior chestBehavior, ChestBehavior.UnlockType unlockType)
        {
            this.chestBehavior = chestBehavior;

            CacheComponents();

            if (unlockType == ChestBehavior.UnlockType.Unlocked)
            {
                gameObject.SetActive(false);
            }
            else if(unlockType == ChestBehavior.UnlockType.Ad)
            {
                adsButton.gameObject.SetActive(true);
                adsButton.onClick.AddListener(OnAdButtonClicked);
            }
            else if (unlockType == ChestBehavior.UnlockType.Purchase)
            {
                purchaseButton.gameObject.SetActive(true);
                purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);

                priceText.text = chestBehavior.UnlockPrice.GetTextWithIcon();

                Vector2 priceTextSize = priceText.GetPreferredValues(priceText.text);

                RectTransform buttonRectTransform = (RectTransform)purchaseButton.transform;
                buttonRectTransform.sizeDelta = new Vector2(priceTextSize.x + 50, buttonRectTransform.sizeDelta.y);
            }
        }

        public void Activate()
        {
            canvas.enabled = true;

            stateTweenCase.KillActive();
            stateTweenCase = canvasGroup.DOFade(1.0f, 0.2f);

            GamepadIndicatorUI.Instance.AddGamepadInteractiopPoint(this);
        }

        public void Disable()
        {
            stateTweenCase.KillActive();
            stateTweenCase = canvasGroup.DOFade(0.0f, 0.2f).OnComplete(() =>
            {
                canvas.enabled = false;
            });

            GamepadIndicatorUI.Instance.RemoveGamepadInteractiopPoint(this);
        }

        private void OnAdButtonClicked()
        {
            if (chestBehavior.IsUnlocked) return;

#if MODULE_MONETIZATION
            AdsManager.ShowRewardBasedVideo((reward) =>
            {
                if(reward)
                {
                    chestBehavior.UnlockChest();

                    Disable();
                }
            });
#else
            Debug.LogWarning("Monetization module is missing!");

            chestBehavior.UnlockChest();

            Disable();
#endif

        }

        private void OnPurchaseButtonClicked()
        {
            if (chestBehavior.IsUnlocked) return;

            CurrencyPrice price = chestBehavior.UnlockPrice;
            if(price.EnoughMoneyOnBalance())
            {
                price.SubstractFromBalance();

                chestBehavior.UnlockChest();

                Disable();
            }
        }

        public void Interact()
        {
            if (adsButton.gameObject.activeInHierarchy)
            {
                adsButton.ClickButton();
            } else
            {
                purchaseButton.ClickButton();
            }
        }

        private void OnDestroy()
        {
            GamepadIndicatorUI.Instance.RemoveGamepadInteractiopPoint(this);
        }
    }
}