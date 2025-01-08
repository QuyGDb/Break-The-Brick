using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class ResourceUI : MonoBehaviour
    {
        [SerializeField] Image resourceIconImage;
        [SerializeField] TMP_Text resourceAmountText;

        public CurrencyType CurrencyType { get; private set; }

        public void SetData(CurrencyType currencyType, string text)
        {
            CurrencyType = currencyType;

            resourceAmountText.text = text;

            Currency currency = CurrenciesController.GetCurrency(currencyType);
            if (currency != null)
            {
                resourceIconImage.sprite = currency.Icon;
            }
        }
    }
}
