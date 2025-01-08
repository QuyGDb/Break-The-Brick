using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CurrencyPrice
    {
        private const string TEXT_FORMAT = "<sprite name={0}>{1}";

        [SerializeField] CurrencyType currencyType;
        public CurrencyType CurrencyType => currencyType;

        [SerializeField] int price;
        public int Price => price;

        public Currency Currency => CurrenciesController.GetCurrency(currencyType);
        public string FormattedPrice => CurrenciesHelper.Format(price);

        public CurrencyPrice()
        {

        }

        public CurrencyPrice(CurrencyType currencyType, int price)
        {
            this.currencyType = currencyType;
            this.price = price;
        }

        public bool EnoughMoneyOnBalance()
        {
            return CurrenciesController.HasAmount(currencyType, price);
        }

        public void SubstractFromBalance()
        {
            CurrenciesController.Substract(currencyType, price);
        }

        public string GetTextWithIcon()
        {
            return string.Format(TEXT_FORMAT, currencyType, price);
        }
    }
}
