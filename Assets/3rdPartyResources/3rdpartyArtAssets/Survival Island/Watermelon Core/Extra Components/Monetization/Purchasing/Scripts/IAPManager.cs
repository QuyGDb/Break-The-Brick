using System.Collections.Generic;
using UnityEngine;

#if MODULE_IAP
using UnityEngine.Purchasing;
#endif

namespace Watermelon
{
    public static class IAPManager
    {
        private static Dictionary<ProductKeyType, IAPItem> productsTypeToProductLink = new Dictionary<ProductKeyType, IAPItem>();
        private static Dictionary<string, IAPItem> productsKeyToProductLink = new Dictionary<string, IAPItem>();

        private static bool isInitialised = false;
        public static bool IsInitialised => isInitialised;

        private static IAPWrapper wrapper;

        public static event SimpleCallback OnPurchaseModuleInitted;
        public static event ProductCallback OnPurchaseComplete;
        public static event ProductFailCallback OnPurchaseFailded;

        public static void Initialise(MonetizationSettings monetizationSettings)
        {
            if(isInitialised)
            {
                Debug.LogError("[IAP Manager]: Module is already initialized!");

                return;
            }

            IAPSettings settings = monetizationSettings.IAPSettings;

            GameObject messagesCanvasPrefab = settings.MessagesCanvasPrefab;
            if(messagesCanvasPrefab != null)
            {
                GameObject canvasGameObject = GameObject.Instantiate(settings.MessagesCanvasPrefab);
                canvasGameObject.transform.SetParent(Initialiser.InitialiserGameObject.transform);
                canvasGameObject.transform.localScale = Vector3.one;
                canvasGameObject.transform.localPosition = Vector3.zero;
                canvasGameObject.transform.localRotation = Quaternion.identity;
                canvasGameObject.GetComponent<IAPMessagesCanvas>().Initialise();
            }
            else
            {
                Debug.LogError("[IAP Manager]: IAP messages canvas prefab is missing!");
            }

            IAPItem[] items = settings.StoreItems;
            for (int i = 0; i < items.Length; i++)
            {
                productsTypeToProductLink.Add(items[i].ProductKeyType, items[i]);
                productsKeyToProductLink.Add(items[i].ID, items[i]);
            }

            wrapper = GetPlatformWrapper();
            wrapper.Initialise(settings);
        }

        public static IAPItem GetIAPItem(ProductKeyType productKeyType)
        {
            if (productsTypeToProductLink.ContainsKey(productKeyType))
                return productsTypeToProductLink[productKeyType];

            return null;
        }

        public static IAPItem GetIAPItem(string ID)
        {
            if (productsKeyToProductLink.ContainsKey(ID))
                return productsKeyToProductLink[ID];

            return null;
        }

#if MODULE_IAP
        public static Product GetProduct(ProductKeyType productKeyType)
        {
            IAPItem iapItem = GetIAPItem(productKeyType);
            if (iapItem != null)
            {
                return UnityIAPWrapper.Controller.products.WithID(iapItem.ID);
            }

            return null;
        }
#endif

        public static void RestorePurchases()
        {
            if (!Monetization.IsActive || !isInitialised) return;

            wrapper.RestorePurchases();
        }

        public static void SubscribeOnPurchaseModuleInitted(SimpleCallback callback)
        {
            if (isInitialised)
            {
                callback?.Invoke();
            }
            else
            {
                OnPurchaseModuleInitted += callback;
            }
        }

        public static void BuyProduct(ProductKeyType productKeyType)
        {
            if (!Monetization.IsActive || !isInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                return;
            }

            wrapper.BuyProduct(productKeyType);
        }

        public static ProductData GetProductData(ProductKeyType productKeyType)
        {
            if (!Monetization.IsActive || !isInitialised) return new ProductData();

            return wrapper.GetProductData(productKeyType);
        }

        public static bool IsSubscribed(ProductKeyType productKeyType)
        {
            if (!Monetization.IsActive || !isInitialised) return false;

            return wrapper.IsSubscribed(productKeyType);
        }

        public static string GetProductLocalPriceString(ProductKeyType productKeyType)
        {
            ProductData product = GetProductData(productKeyType);

            if (product == null)
                return string.Empty;

            return string.Format("{0} {1}", product.ISOCurrencyCode, product.Price);
        }

        public static void OnModuleInitialised()
        {
            isInitialised = true;

            OnPurchaseModuleInitted?.Invoke();

            if (Monetization.VerboseLogging)
                Debug.Log("[IAPManager]: Module is initialized!");
        }

        public static void OnPurchaseCompled(ProductKeyType productKey)
        {
            OnPurchaseComplete?.Invoke(productKey);
        }

        public static void OnPurchaseFailed(ProductKeyType productKey, Watermelon.PurchaseFailureReason failureReason)
        {
            OnPurchaseFailded?.Invoke(productKey, failureReason);
        }

        private static IAPWrapper GetPlatformWrapper()
        {
#if MODULE_IAP
            return new UnityIAPWrapper();
#else
            return new DummyIAPWrapper();
#endif
        }

        public delegate void ProductCallback(ProductKeyType productKeyType);
        public delegate void ProductFailCallback(ProductKeyType productKeyType, Watermelon.PurchaseFailureReason failureReason);
    }
}

// -----------------
// IAP Manager v 1.2.2
// -----------------

// Changelog
// v 1.2.2
// • Fixed serialization bug
// v 1.2.1
// • Added test mode
// v 1.2
// • Support of IAP version 4.11.0
// • Added Editor purchase wrapper
// v 1.1
// • Support of IAP version 4.9.3
// v 1.0.3
// • Support of IAP version 4.7.0
// v 1.0.2
// • Added quick access to the local price of IAP via GetProductLocalPriceString method
// v 1.0.1
// • Added restoration status messages
// v 1.0.0
// • Documentation added
// v 0.4
// • IAPStoreListener inheriting from MonoBehaviour
// v 0.3
// • Editor style update
// v 0.2
// • IAPManager structure changed
// • Enums from UnityEditor.Purchasing has duplicated to prevent serialization problems
// v 0.1
// • Added basic version
