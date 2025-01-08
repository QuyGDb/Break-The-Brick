using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class CurrencyReward : Reward
    {
        [SerializeField] CurrencyData[] currencies;

        [SerializeField] bool spawnCurrencyCloud;

        [ShowIf("spawnCurrencyCloud")]
        [SerializeField] CurrencyType currencyCloudType;
        [ShowIf("spawnCurrencyCloud")]
        [SerializeField] int cloudElementsAmount = 10;
        [ShowIf("spawnCurrencyCloud")]
        [SerializeField] RectTransform currencyCloudSpawnPoint;
        [ShowIf("spawnCurrencyCloud")]
        [SerializeField] RectTransform currencyCloudTargetPoint;

        public override void Initialise()
        {
            foreach (CurrencyData currencyData in currencies)
            {
                Currency currency = CurrenciesController.GetCurrency(currencyData.CurrencyType);

                if (currencyData.CurrencyImage != null)
                    currencyData.CurrencyImage.sprite = currency.Icon;

                if (currencyData.AmountText != null)
                {
                    string numberText = currencyData.FormatTheNumber ? CurrenciesHelper.Format(currencyData.Amount) : currencyData.Amount.ToString();
                    currencyData.AmountText.text = string.Format(currencyData.TextFormating == "" ? "{0}" : currencyData.TextFormating, numberText);
                }
            }
        }

        public override void ApplyReward()
        {
            void ApplyCurrency()
            {
                foreach (CurrencyData currencyData in currencies)
                {
                    CurrenciesController.Add(currencyData.CurrencyType, currencyData.Amount);
                }
            }

            if(spawnCurrencyCloud)
            {
                FloatingCloud.SpawnCurrency(currencyCloudType.ToString(), currencyCloudSpawnPoint, currencyCloudTargetPoint, cloudElementsAmount, "", ApplyCurrency);
            }
            else
            {
                ApplyCurrency();
            }
        }

        [System.Serializable]
        public class CurrencyData
        {
            [SerializeField] CurrencyType currencyType;
            public CurrencyType CurrencyType => currencyType;

            [SerializeField] int amount;
            public int Amount => amount;

            [Space]
            [SerializeField] Image currencyImage;
            public Image CurrencyImage => currencyImage;

            [SerializeField] TextMeshProUGUI amountText;
            public TextMeshProUGUI AmountText => amountText;

            [SerializeField] string textFormating = "x{0}";
            public string TextFormating => textFormating;

            [SerializeField] bool formatTheNumber;
            public bool FormatTheNumber => formatTheNumber;
        }
    }
}
