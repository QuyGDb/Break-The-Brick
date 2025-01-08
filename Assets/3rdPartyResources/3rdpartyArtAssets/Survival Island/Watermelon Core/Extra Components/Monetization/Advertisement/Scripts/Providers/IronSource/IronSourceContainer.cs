using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class IronSourceContainer
    {
        [SerializeField] string androidAppKey = "85460dcd";
        public string AndroidAppKey => androidAppKey;

        [SerializeField] string iOSAppKey = "8545d445";
        public string IOSAppKey => iOSAppKey;

        [Space]
        [SerializeField] BannerPosition bannerPosition;
        public BannerPosition BannerPosition => bannerPosition;

        [SerializeField] BannerPlacementType bannerType;
        public BannerPlacementType BannerType => bannerType;

        public enum BannerPlacementType
        {
            Banner = 0,
            Large = 1,
            Rectangle = 2,
            Smart = 3
        }
    }
}