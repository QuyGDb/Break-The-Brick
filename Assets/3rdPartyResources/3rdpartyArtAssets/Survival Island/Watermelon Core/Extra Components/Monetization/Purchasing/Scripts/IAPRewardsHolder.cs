using UnityEngine;

namespace Watermelon
{
    public sealed class IAPRewardsHolder : RewardsHolder
    {
        [Group("Settings")]
        [SerializeField] ProductKeyType productKey;

        [Space]
        [Group("Settings")]
        [SerializeField] IAPButton purchaseButton;

        private SimpleBoolSave save;
        private ProductData product;

        private void Start()
        {
            InitialiseComponents();

            // Link the product type to purchase button
            purchaseButton.Initialise(productKey);

            // Wait for IAP Manager initialisation or call method immediately if the module is already initialised
            IAPManager.SubscribeOnPurchaseModuleInitted(OnIAPManagerLoaded);
        }

        private void OnEnable()
        {
            // Subscribe to purchase callback
            IAPManager.OnPurchaseComplete += OnPurchaseComplete;
        }

        private void OnDisable()
        {
            // Unsubscribe from purchase callback
            IAPManager.OnPurchaseComplete -= OnPurchaseComplete;
        }

        private void OnIAPManagerLoaded()
        {
            // Get the product save file to check if it was previously purchased
            // This data is stored only locally so after the game reinstall it will be reset
            save = SaveController.GetSaveObject<SimpleBoolSave>($"IAPProduct_{productKey}");
            
            // Get product data wrapper
            // To acess Unity IAP product use product.Product property
            product = IAPManager.GetProductData(productKey);

            // Update button state
            // If there is problem with the internet connection or server didn't return product data loading animation appeared
            purchaseButton.UpdateState(product);

            if(product.IsPurchased || product.ProductType == ProductType.NonConsumable && save.Value)
            {
                // Disable holder if it's an one time purchase (non-consumable) product 
                if (product.ProductType == ProductType.NonConsumable)
                {
                    // Disable holder game object
                    gameObject.SetActive(false);

                    return;
                }
            }

            // Check if holder needs to be disabled
            for (int i = 0; i < rewards.Length; i++)
            {
                if (rewards[i].CheckDisableState())
                {
                    // Disable holder game object
                    gameObject.SetActive(false);

                    break;
                }
            }
        }

        private void OnPurchaseComplete(ProductKeyType key)
        {
            // Check if the purchased product type is equal to holder's product type
            if (productKey == key)
            {
                ApplyRewards();

                // Disable holder if it's an one time purchase (non-consumable) product 
                if (product.ProductType == ProductType.NonConsumable)
                {
                    // Disable holder game object
                    gameObject.SetActive(false);
                }

                // Change local save data
                save.Value = true;

                // Mark save as dirty to make sure changes will be stored
                SaveController.MarkAsSaveIsRequired();
            }
        }
    }
}
