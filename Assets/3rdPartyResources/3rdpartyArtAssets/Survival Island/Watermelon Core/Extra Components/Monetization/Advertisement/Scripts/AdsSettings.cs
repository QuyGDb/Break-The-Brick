#pragma warning disable 0414

using UnityEngine;

namespace Watermelon
{
    [SetupTab("Advertising", texture = "icon_ads")]
    [HelpURL("https://www.notion.so/wmelongames/Advertisement-221053e32d4047bb880275027daba9f0?pvs=4")]
    public class AdsSettings : ScriptableObject
    {
        [SerializeField] AdProvider bannerType = AdProvider.Dummy;
        public AdProvider BannerType => bannerType;

        [SerializeField] AdProvider interstitialType = AdProvider.Dummy;
        public AdProvider InterstitialType => interstitialType;

        [SerializeField] AdProvider rewardedVideoType = AdProvider.Dummy;
        public AdProvider RewardedVideoType => rewardedVideoType;

        // Providers
        [SerializeField] AdMobContainer adMobContainer;
        public AdMobContainer AdMobContainer => adMobContainer;

        [SerializeField] UnityAdsLegacyContainer unityAdsContainer;
        public UnityAdsLegacyContainer UnityAdsContainer => unityAdsContainer;

        [SerializeField] IronSourceContainer ironSourceContainer;
        public IronSourceContainer IronSourceContainer => ironSourceContainer;

        // Dummy
        [SerializeField] AdDummyContainer dummyContainer;
        public AdDummyContainer DummyContainer => dummyContainer;

        [Group("Settings")]
        [SerializeField] bool loadAdsOnStart = true;
        public bool LoadAdsOnStart => loadAdsOnStart;

        [Space]
        [Group("Settings")]
        [Tooltip("Delay in seconds before interstitial appearings on first game launch.")]
        [SerializeField] float interstitialFirstStartDelay = 40f;
        public float InterstitialFirstStartDelay => interstitialFirstStartDelay;

        [Group("Settings")]
        [Tooltip("Delay in seconds before interstitial appearings.")]
        [SerializeField] float interstitialStartDelay = 40f;
        public float InterstitialStartDelay => interstitialStartDelay;

        [Group("Settings")]
        [Tooltip("Delay in seconds between interstitial appearings.")]
        [SerializeField] float interstitialShowingDelay = 30f;
        public float InterstitialShowingDelay => interstitialShowingDelay;

        [Group("Settings")]
        [SerializeField] bool autoShowInterstitial;
        public bool AutoShowInterstitial => autoShowInterstitial;

        [Group("Privacy")]
        [SerializeField] bool isGDPREnabled = false;
        public bool IsGDPREnabled => isGDPREnabled;

        [Group("Privacy")]
        [SerializeField] GameObject gdprPanelPrefab;
        public GameObject GDPRPanelPrefab => gdprPanelPrefab;

        [Space]
        [Group("Privacy")]
        [SerializeField] bool isIDFAEnabled = false;
        public bool IsIDFAEnabled => isIDFAEnabled;

        [Group("Privacy")]
        [SerializeField] string trackingDescription = "Your data will be used to deliver personalized ads to you.";
        public string TrackingDescription => trackingDescription;

        [Space]
        [Group("Privacy")]
        [SerializeField] string privacyLink = "https://mywebsite.com/privacy";
        public string PrivacyLink => privacyLink;

        [Group("Privacy")]
        [SerializeField] string termsOfUseLink = "https://mywebsite.com/terms";
        public string TermsOfUseLink => termsOfUseLink;

        public bool IsDummyEnabled()
        {
            if (bannerType == AdProvider.Dummy)
                return true;

            if (interstitialType == AdProvider.Dummy)
                return true;

            if (rewardedVideoType == AdProvider.Dummy)
                return true;

            return false;
        }
    }

    public enum BannerPosition
    {
        Bottom = 0,
        Top = 1,
    }
}