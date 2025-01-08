using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon
{
    public class PriceItemUI : MonoBehaviour
    {
        [SerializeField] Image currencyIcon;
        [SerializeField] TMP_Text priceText;

        public void SetData(Resource price)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                CurrenciesController cc = GameObject.Find("ScriptHolder").GetComponent<CurrenciesController>();
                Currency curr = System.Array.Find(cc.CurrenciesDatabase.Currencies, c => c.CurrencyType == price.currency);

                currencyIcon.sprite = curr.Icon;

                priceText.text = $"{price.amount}";

                PrefabUtility.RecordPrefabInstancePropertyModifications(currencyIcon);
                PrefabUtility.RecordPrefabInstancePropertyModifications(priceText);
                return;
            }
#endif
            //currencyIcon.sprite = CurrenciesController.GetCurrency(price.currency).Icon;

            priceText.text = string.Format("<sprite name={1}>{0}", price.amount, price.currency);
        }
    }
}