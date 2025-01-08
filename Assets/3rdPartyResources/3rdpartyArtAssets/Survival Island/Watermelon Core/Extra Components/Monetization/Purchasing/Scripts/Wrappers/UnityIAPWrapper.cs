using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if MODULE_IAP
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
#endif

namespace Watermelon
{
    public class UnityIAPWrapper : IAPWrapper
#if MODULE_IAP
        , IDetailedStoreListener
#endif
    {

#if MODULE_IAP
        private static IStoreController controller;
        public static IStoreController Controller => controller;

        private static IExtensionProvider extensions;
        public static IExtensionProvider Extensions => extensions;
#endif

        public override async void Initialise(IAPSettings settings)
        {
#if MODULE_IAP
            try
            {
                var options = new InitializationOptions().SetEnvironmentName("production");

                await UnityServices.InitializeAsync(options);

                StandardPurchasingModule purchasingModule = StandardPurchasingModule.Instance();

                if (Monetization.DebugMode)
                {
                    purchasingModule.useFakeStoreAlways = true;
                    purchasingModule.useFakeStoreUIMode = FakeStoreUIMode.DeveloperUser;
                }

                // Init products
                ConfigurationBuilder builder = ConfigurationBuilder.Instance(purchasingModule);

                IAPItem[] items = settings.StoreItems;
                for (int i = 0; i < items.Length; i++)
                {
                    builder.AddProduct(items[i].ID, (UnityEngine.Purchasing.ProductType)items[i].ProductType);
                }

                UnityPurchasing.Initialize(this, builder);
            }
            catch (System.Exception exception)
            {
                Debug.LogError(exception.Message);
            }
#else
            await Task.Run(() => Debug.Log("[IAP Manager]: Define MODULE_IAP is disabled!"));
#endif
        }

#if MODULE_IAP
        /// <summary>
        /// Called when Unity IAP is ready to make purchases.
        /// </summary>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            UnityIAPWrapper.controller = controller;
            UnityIAPWrapper.extensions = extensions;

            IAPManager.OnModuleInitialised();
        }

        /// <summary>
        /// Called when Unity IAP encounters an unrecoverable initialization error.
        ///
        /// Note that this will not be called if Internet is unavailable; Unity IAP
        /// will attempt initialization until it becomes available.
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            if (Monetization.VerboseLogging)
                Debug.Log("[IAPManager]: Module initialization is failed!");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            if (Monetization.VerboseLogging)
                Debug.Log(string.Format("[IAPManager]: Module initialization is failed! {0}", message));
        }

        /// <summary>
        /// Called when a purchase completes.
        ///
        /// May be called at any time after OnInitialized().
        /// </summary>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            if (Monetization.VerboseLogging)
                Debug.Log("[IAPManager]: Purchasing - " + e.purchasedProduct.definition.id + " is completed!");

            IAPItem item = IAPManager.GetIAPItem(e.purchasedProduct.definition.id);
            if (item != null)
            {
                IAPManager.OnPurchaseCompled(item.ProductKeyType);
            }
            else
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[IAPManager]: Product - " + e.purchasedProduct.definition.id + " can't be found!");
            }

            IAPMessagesCanvas.ChangeLoadingMessage("Payment complete!");
            IAPMessagesCanvas.HideLoadingPanel();

            return PurchaseProcessingResult.Complete;
        }

        /// <summary>
        /// Called when a purchase fails.
        /// </summary>
        public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, UnityEngine.Purchasing.PurchaseFailureReason failureReason)
        {
            if (Monetization.VerboseLogging)
            {
                Debug.Log("[IAPManager]: Purchasing - " + product.definition.id + " is failed!");
                Debug.Log("[IAPManager]: Fail reason - " + failureReason.ToString());
            }

            IAPItem item = IAPManager.GetIAPItem(product.definition.id);
            if (item != null)
            {
                IAPManager.OnPurchaseFailed(item.ProductKeyType, (Watermelon.PurchaseFailureReason)failureReason);
            }
            else
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[IAPManager]: Product - " + product.definition.id + " can't be found!");
            }

            IAPMessagesCanvas.ChangeLoadingMessage("Payment failed!");
            IAPMessagesCanvas.HideLoadingPanel();
        }

        public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureDescription failureDescription)
        {
            if (Monetization.VerboseLogging)
            {
                Debug.Log("[IAPManager]: Purchasing - " + product.definition.id + " is failed!");
                Debug.Log("[IAPManager]: Fail reason - " + failureDescription.message);
            }

            IAPItem item = IAPManager.GetIAPItem(product.definition.id);
            if (item != null)
            {
                IAPManager.OnPurchaseFailed(item.ProductKeyType, (Watermelon.PurchaseFailureReason)failureDescription.reason);
            }
            else
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[IAPManager]: Product - " + product.definition.id + " can't be found!");
            }

            IAPMessagesCanvas.ChangeLoadingMessage("Payment failed!");
            IAPMessagesCanvas.HideLoadingPanel();
        }
#endif

        public override void RestorePurchases()
        {
#if MODULE_IAP
            if (!IAPManager.IsInitialised)
            {
                IAPMessagesCanvas.ShowMessage("Network error. Please try again later");

                return;
            }

            IAPMessagesCanvas.ShowLoadingPanel();
            IAPMessagesCanvas.ChangeLoadingMessage("Restoring purchased products..");

            extensions.GetExtension<IAppleExtensions>().RestoreTransactions((result, message) =>
            {
                if (result)
                {
                    // This does not mean anything was restored,
                    // merely that the restoration process succeeded.
                    IAPMessagesCanvas.ChangeLoadingMessage("Restoration completed!");
                }
                else
                {
                    // Restoration failed.
                    IAPMessagesCanvas.ChangeLoadingMessage("Restoration failed!");
                }

                IAPMessagesCanvas.HideLoadingPanel();
            });
#endif
        }

        public override void BuyProduct(ProductKeyType productKeyType)
        {
#if MODULE_IAP
            if (!IAPManager.IsInitialised)
            {
                IAPMessagesCanvas.ShowMessage("Network error. Please try again later");

                return;
            }

            IAPMessagesCanvas.ShowLoadingPanel();
            IAPMessagesCanvas.ChangeLoadingMessage("Payment in progress..");

            IAPItem item = IAPManager.GetIAPItem(productKeyType);
            if(item != null)
            {
                controller.InitiatePurchase(item.ID);
            }
#else
            FloatingMessage.ShowMessage("Network error.");
#endif
        }

        public override ProductData GetProductData(ProductKeyType productKeyType)
        {
            if (!IAPManager.IsInitialised)
                return null;

#if MODULE_IAP
            IAPItem item = IAPManager.GetIAPItem(productKeyType);
            if (item != null)
            {
                return new ProductData(controller.products.WithID(item.ID));
            }
#endif

            return null;
        }

        public override bool IsSubscribed(ProductKeyType productKeyType)
        {
#if MODULE_IAP
            IAPItem item = IAPManager.GetIAPItem(productKeyType);
            if (item != null)
            {
                Product product = controller.products.WithID(item.ID);
                if (product != null)
                {
                    // If the product doesn't have a receipt, then it wasn't purchased and the user is therefore not subscribed.
                    if (product.receipt == null)
                        return false;

                    //The intro_json parameter is optional and is only used for the App Store to get introductory information.
                    SubscriptionManager subscriptionManager = new SubscriptionManager(product, null);

                    // The SubscriptionInfo contains all of the information about the subscription.
                    // Find out more: https://docs.unity3d.com/Packages/com.unity.purchasing@3.1/manual/UnityIAPSubscriptionProducts.html
                    SubscriptionInfo info = subscriptionManager.getSubscriptionInfo();

                    return info.isSubscribed() == Result.True;
                }
            }
#endif

            return false;
        }
    }
}
