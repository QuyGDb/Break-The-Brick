using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class InventoryUIPanel : MonoBehaviour
    {
        private const string AMOUNT_FORMAT = "{0}/{1}";
        private const string FULL = "FULL!";

        [SerializeField] TextMeshProUGUI amountText;

        private PlayerInventory inventory;

        public void Initialise(PlayerInventory inventory)
        {
            this.inventory = inventory;
        }

        public void UpdateUI(int amount)
        {
            amountText.text = string.Format(AMOUNT_FORMAT, CurrenciesHelper.Format(amount), CurrenciesHelper.Format(inventory.MaxCapacity));
        }

        public void ShowFullMessage()
        {
            amountText.text = FULL;
        }
    }
}