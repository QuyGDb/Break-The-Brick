using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class IAPItem
    {
        [SerializeField] string androidID;
        [SerializeField] string iOSID;
        [SerializeField] ProductKeyType productKeyType;
        [SerializeField] ProductType productType;

        public string ID
        {
            get
            {
#if UNITY_ANDROID
                return androidID;
#elif UNITY_IOS
                return iOSID;
#else
                return string.Format("unknown_platform_{0}", productKeyType);
#endif
            }
        }

        public ProductType ProductType { get => productType; set => productType = value; }
        public ProductKeyType ProductKeyType { get => productKeyType; set => productKeyType = value; }

        public IAPItem()
        {
        }

        public IAPItem(string id, ProductKeyType productKeyType, ProductType productType)
        {
            this.androidID = id;
            this.iOSID = id;
            this.productKeyType = productKeyType;
            this.productType = productType;
        }

        public IAPItem(string androidID, string iOSID, ProductKeyType productKeyType, ProductType productType)
        {
            this.androidID = androidID;
            this.iOSID = iOSID;
            this.productKeyType = productKeyType;
            this.productType = productType;
        }
    }
}
