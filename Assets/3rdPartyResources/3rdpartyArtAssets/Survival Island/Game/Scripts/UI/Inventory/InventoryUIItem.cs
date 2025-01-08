using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public class InventoryUIItem : MonoBehaviour
    {
        [SerializeField] Image currencyIcon;

        [SerializeField] TMP_Text currencyCount;
        [SerializeField] TMP_Text moneyCount;

        [Space]
        [SerializeField] Button sellButton;

        [Space]
        [SerializeField] Slider countSlider;

        [Space]
        [SerializeField] Image outline;

        public CurrencyType CurrencyType { get; private set; }
        public Currency Currency { get; private set; }

        public int CurrencyCount { get; private set; }
        public int MoneyCount { get; private set; }

        public bool IsSelected { get; private set; }

        public RectTransform Rect { get; private set; }

        protected void Awake()
        {
            sellButton.onClick.AddListener(OnSellButtonClicked);
            countSlider.onValueChanged.AddListener(OnSliderValueChanged);

            outline.enabled = false;

            Rect = GetComponent<RectTransform>();
        }

        public void Init(Currency currency)
        {
            CurrencyType = currency.CurrencyType;
            Currency = currency;

            currencyIcon.sprite = Currency.Icon;

            countSlider.normalizedValue = 0f;
            OnSliderValueChanged(0);
        }

        public void OnSelect()
        {
            IsSelected = true;

            if(Control.InputType == InputType.Gamepad)
            {
                outline.enabled = true;
            }

            Control.OnInputChanged += OnControlChanged;
        }

        public void OnDeselect()
        {
            IsSelected = false;

            outline.enabled = false;

            Control.OnInputChanged -= OnControlChanged;
        }

        private void OnControlChanged(InputType type)
        {
            outline.enabled = type == InputType.Gamepad && IsSelected;
        }

        private void Update()
        {
            if(IsSelected && Control.InputType == InputType.Gamepad)
            {
                // Slider
                float sliderShift = GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DLeft) ? -1 : 0;
                sliderShift += GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DRight) ? 1 : 0;

                sliderShift += GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.LB) ? -10 : 0;
                sliderShift += GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.RB) ? 10 : 0;

                float dLeftTime = GamepadControl.GetButtonPressedTime(GamepadButtonType.DLeft);
                float dRightTime = GamepadControl.GetButtonPressedTime(GamepadButtonType.DRight);
                float lbTime = GamepadControl.GetButtonPressedTime(GamepadButtonType.LB);
                float rbTime = GamepadControl.GetButtonPressedTime(GamepadButtonType.RB);

                if (dLeftTime > 0 && Time.time - dLeftTime > 0.3f) sliderShift -= Mathf.Clamp(Currency.Amount * Time.deltaTime / 10, 1, float.MaxValue);
                if (dRightTime > 0 && Time.time - dRightTime > 0.3f) sliderShift += Mathf.Clamp(Currency.Amount * Time.deltaTime / 10, 1, float.MaxValue);

                if (lbTime > 0 && Time.time - lbTime > 0.3f) sliderShift -= Mathf.Clamp(Currency.Amount * Time.deltaTime, 1, float.MaxValue);
                if (rbTime > 0 && Time.time - rbTime > 0.3f) sliderShift += Mathf.Clamp(Currency.Amount * Time.deltaTime, 1, float.MaxValue);

                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.L3)) sliderShift = -Currency.Amount;
                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.R3)) sliderShift = Currency.Amount;

                var sliderValue = Mathf.Clamp01(countSlider.value + 1f / Currency.Amount * sliderShift);

                // Selling
                
                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.A))
                {
                    sellButton.ClickButton();
                }

                countSlider.value = sliderValue;
            }
        }

        private void OnSellButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            CurrenciesController.Add(CurrencyType.Coins, MoneyCount);
            CurrenciesController.Substract(CurrencyType, CurrencyCount);

            if (CurrenciesController.HasAmount(CurrencyType, 1))
            {
                countSlider.value = 0;
            }
            else
            {
                gameObject.SetActive(false);

                InventoryUIPage.OnCurrencyPanelDisabled(this);
            }
        }

        private void OnSliderValueChanged(float value)
        {
            CurrencyCount = Mathf.RoundToInt(Mathf.Lerp(1, Currency.Amount, value));

            MoneyCount = CurrencyCount * Currency.Data.MoneyConversionRate;

            currencyCount.text = $"{CurrencyCount}/{Currency.Amount}";
            moneyCount.text = $"{MoneyCount}";
        }

        private void OnDisable()
        {
            if(IsSelected) OnDeselect();
        }
    }
}