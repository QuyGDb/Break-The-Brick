using System.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    public class DummyIAPWrapper : IAPWrapper
    {
        public override void BuyProduct(ProductKeyType productKeyType)
        {
            if (!IAPManager.IsInitialised)
            {
                IAPMessagesCanvas.ShowMessage("Network error. Please try again later");

                return;
            }

            IAPMessagesCanvas.ShowLoadingPanel();
            IAPMessagesCanvas.ChangeLoadingMessage("Payment in progress..");

            Tween.NextFrame(() =>
            {
                if (Monetization.VerboseLogging)
                    Debug.Log(string.Format("[IAPManager]: Purchasing - {0} is completed!", productKeyType));

                IAPManager.OnPurchaseCompled(productKeyType);

                IAPMessagesCanvas.ChangeLoadingMessage("Payment complete!");
                IAPMessagesCanvas.HideLoadingPanel();
            });
        }

        public override ProductData GetProductData(ProductKeyType productKeyType)
        {
            IAPItem iapItem = IAPManager.GetIAPItem(productKeyType);
            if(iapItem != null)
            {
                return new ProductData(iapItem.ProductType);
            }

            return null;
        }

        public override void Initialise(IAPSettings settings)
        {
            if (Monetization.VerboseLogging)
                Debug.LogWarning("[IAP Manager]: Dummy mode is activated. Configure the module before uploading the game to stores!");

            IAPManager.OnModuleInitialised();
        }

        public override bool IsSubscribed(ProductKeyType productKeyType)
        {
            return false;
        }

        public override void RestorePurchases()
        {
            // DO NOTHING
        }
    }
}