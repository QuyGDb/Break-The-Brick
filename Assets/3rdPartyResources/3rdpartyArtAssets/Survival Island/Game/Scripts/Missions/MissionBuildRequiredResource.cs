using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class MissionBuildRequiredResource : MonoBehaviour
    {
        [SerializeField] Image iconImage;

        [SerializeField] TextMeshProUGUI amountText;
        public TextMeshProUGUI AmountText => amountText;

        [SerializeField] GameObject markObject;

        private Currency currency;
        private int currentAmount;

        public void Initialise(CurrencyType currencyType, int amount)
        {
            currency = CurrenciesController.GetCurrency(currencyType);

            iconImage.sprite = currency.Icon;

            UpdateAmount(amount);
        }

        public void UpdateAmount(int amount)
        {
            if (amount > 0)
            {
                amountText.gameObject.SetActive(true);
                markObject.SetActive(false);

                amountText.text = amount.ToString();
            }
            else
            {
                markObject.SetActive(true);
                amountText.gameObject.SetActive(false);
            }

            currentAmount = amount;
        }

        public bool HasEnoughCurrency()
        {
            return currency.Amount >= currentAmount;
        }
    }
}