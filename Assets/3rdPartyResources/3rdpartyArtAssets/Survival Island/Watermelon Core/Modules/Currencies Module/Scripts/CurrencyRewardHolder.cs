using UnityEngine;

namespace Watermelon
{
    public sealed class CurrencyRewardsHolder : RewardsHolder
    {
        [Group("Settings"), UniqueID]
        [SerializeField] string rewardID;

        [Group("Settings")]
        [SerializeField] UICurrencyButton currencyButton;

        [Group("Settings")]
        [SerializeField] CurrencyPrice price;

        [Group("Settings"), Space]
        [SerializeField] bool disableAfterPurchase;

        private SimpleBoolSave save;

        private void Awake()
        {
            InitialiseComponents();

            save = SaveController.GetSaveObject<SimpleBoolSave>($"CurrencyProduct_{rewardID}");

            if(disableAfterPurchase && save.Value)
            {
                // Disable offer game object
                gameObject.SetActive(false);

                return;
            }

            // Check if offer needs to be disabled
            for (int i = 0; i < rewards.Length; i++)
            {
                if (rewards[i].CheckDisableState())
                {
                    // Disable offer game object
                    gameObject.SetActive(false);

                    return;
                }
            }

            currencyButton.Init(price.Price, price.CurrencyType);
            currencyButton.Purchased += OnPurchased;
        }

        private void OnPurchased()
        {
            ApplyRewards();

            save.Value = true;

            if(disableAfterPurchase)
            {
                // Disable holder game object
                gameObject.SetActive(false);
            }

            SaveController.MarkAsSaveIsRequired();
        }
    }
}
