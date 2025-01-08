#pragma warning disable 0414

using UnityEngine;

#if MODULE_UNITYADS
using UnityEngine.Advertisements;
#endif

namespace Watermelon
{
#if MODULE_UNITYADS
    public class UnityAdsLegacyHandler : AdProviderHandler
    {
        private const int INIT_CHECK_MAX_ATTEMPT_AMOUNT = 5;

        private static string placementBannerID;
        private static string placementInterstitialID;
        private static string placementRewardedVideoID;
        private static string appId;

        private bool interstitialIsLoaded;
        private bool rewardVideoIsLoaded;

        private int initializationAttemptCount = 0;

        private UnityAdvertismentListener unityAdvertisment;

        public UnityAdsLegacyHandler(AdProvider moduleType) : base(moduleType) { }

        public override void Initialise(AdsSettings adsSettings)
        {
            this.adsSettings = adsSettings;

            if (isInitialised || Advertisement.isInitialized)
            {
                Debug.LogError("[AdsManager]: Unity Ads Legacy is already initialized!");

                return;
            }

            if (!Advertisement.isSupported)
            {
                Debug.LogError("[AdsManager]: Unity Ads Legacy isn't supported!");

                return;
            }

            placementBannerID = GetBannerID();
            placementInterstitialID = GetInterstitialID();
            placementRewardedVideoID = GetRewardedVideoID();
            appId = GetUnityAdsAppID();

            unityAdvertisment = Initialiser.InitialiserGameObject.AddComponent<UnityAdvertismentListener>();
            unityAdvertisment.Init(adsSettings, this);

            Advertisement.Initialize(appId, Monetization.DebugMode, unityAdvertisment);

            Advertisement.Banner.SetPosition((UnityEngine.Advertisements.BannerPosition)adsSettings.UnityAdsContainer.BannerPosition);

            InitGDPR(AdsManager.GetGDPRState());

            if (Monetization.VerboseLogging)
            {
                Debug.Log("[AdsManager]: Unity Ads initialized: " + Advertisement.isInitialized);
                Debug.Log("[AdsManager]: Unity Ads is supported: " + Advertisement.isSupported);
                Debug.Log("[AdsManager]: Unity Ads test mode enabled: " + Advertisement.debugMode);
                Debug.Log("[AdsManager]: Unity Ads version: " + Advertisement.version);
            }

            OnProviderInitialised();
        }

        public override void DestroyBanner()
        {
            Advertisement.Banner.Hide(true);
        }

        public override void HideBanner()
        {
            Advertisement.Banner.Hide(false);
        }

        public override void RequestInterstitial()
        {
            Advertisement.Load(placementInterstitialID, unityAdvertisment);
        }

        public override void RequestRewardedVideo()
        {
            Advertisement.Load(placementRewardedVideoID, unityAdvertisment);
        }

        public override void ShowBanner()
        {
            Advertisement.Banner.Show(placementBannerID);
        }

        public override void ShowInterstitial(InterstitialCallback callback)
        {
            Advertisement.Show(placementInterstitialID, unityAdvertisment);
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            Advertisement.Show(placementRewardedVideoID, unityAdvertisment);
        }

        public override bool IsInterstitialLoaded()
        {
            return interstitialIsLoaded;
        }

        public override bool IsRewardedVideoLoaded()
        {
            return rewardVideoIsLoaded;
        }

        public string GetUnityAdsAppID()
        {
#if UNITY_ANDROID
            return adsSettings.UnityAdsContainer.AndroidAppID;
#elif UNITY_IOS
            return adsSettings.UnityAdsSettings.IOSAppID;
#else
            return string.Empty;
#endif
        }

        public override void SetGDPR(bool state)
        {
            InitGDPR(state);
        }

        private void InitGDPR(bool state)
        {
            string gdprState = state ? "true" : "false";

            MetaData gdprMetaData = new MetaData("gdpr");
            gdprMetaData.Set("consent", gdprState);
            Advertisement.SetMetaData(gdprMetaData);

            MetaData privacyMetaData = new MetaData("privacy");
            privacyMetaData.Set("consent", gdprState);
            Advertisement.SetMetaData(privacyMetaData);
        }

        public string GetBannerID()
        {
#if UNITY_ANDROID
            return adsSettings.UnityAdsContainer.AndroidBannerID;
#elif UNITY_IOS
            return adsSettings.UnityAdsSettings.IOSBannerID;
#else
            return string.Empty;
#endif
        }

        public string GetInterstitialID()
        {
#if UNITY_ANDROID
            return adsSettings.UnityAdsContainer.AndroidInterstitialID;
#elif UNITY_IOS
            return adsSettings.UnityAdsSettings.IOSInterstitialID;
#else
            return string.Empty;
#endif
        }

        public string GetRewardedVideoID()
        {
#if UNITY_ANDROID
            return adsSettings.UnityAdsContainer.AndroidRewardedVideoID;
#elif UNITY_IOS
            return adsSettings.UnityAdsSettings.IOSRewardedVideoID;
#else
            return string.Empty;
#endif
        }

        private class UnityAdvertismentListener : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener, IUnityAdsInitializationListener
        {
            private UnityAdsLegacyHandler adsHandler;
            private AdsSettings adsSettings;

            public void Init(AdsSettings adsSettings, UnityAdsLegacyHandler adsHandler)
            {
                this.adsSettings = adsSettings;
                this.adsHandler = adsHandler;
            }

            public void OnInitializationComplete()
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Unity Ads is initialized!");
            }

            public void OnInitializationFailed(UnityAdsInitializationError error, string message)
            {
                adsHandler.initializationAttemptCount++;

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: OnInitializationFailed event error:" + error.ToString() + "    message: " + message);

                if (adsHandler.initializationAttemptCount <= INIT_CHECK_MAX_ATTEMPT_AMOUNT)
                {
                    Advertisement.Initialize(appId, Monetization.DebugMode, this);
                }
                else
                {
                    if (Monetization.VerboseLogging)
                        Debug.Log("[AdsManager]: OnInitializationFailed in every attempt");
                }
            }

            public void OnUnityAdsAdLoaded(string placementId)
            {
                if (placementId.Equals(placementBannerID))
                {
                    Advertisement.Banner.Show(placementBannerID);

                    if (Monetization.VerboseLogging)
                        Debug.Log("[AdsManager]: OnUnityAdsAdLoaded - banner loaded");
                }
                else if (placementId.Equals(placementInterstitialID))
                {
                    adsHandler.interstitialIsLoaded = true;

                    if (Monetization.VerboseLogging)
                        Debug.Log("[AdsManager]: OnUnityAdsAdLoaded - interstitial loaded");
                }
                else if (placementId.Equals(placementRewardedVideoID))
                {
                    adsHandler.rewardVideoIsLoaded = true;

                    if (Monetization.VerboseLogging)
                        Debug.Log("[AdsManager]: OnUnityAdsAdLoaded - rewardVideo loaded");
                }
            }

            public void OnUnityAdsDidError(string message)
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: OnUnityAdsDidError - " + message);
            }

            public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
            {
                if (placementId == placementInterstitialID)
                {
                    AdsManager.OnProviderAdClosed(AdProvider.UnityAdsLegacy, AdType.Interstitial);

                    AdsManager.ExecuteInterstitialCallback(showResult == ShowResult.Finished);

                    AdsManager.ResetInterstitialDelayTime();
                }
                else if (placementId == placementRewardedVideoID)
                {
                    bool state = showResult == ShowResult.Finished;

                    // Reward the player
                    AdsManager.ExecuteRewardVideoCallback(state);

                    AdsManager.OnProviderAdClosed(AdProvider.UnityAdsLegacy, AdType.RewardedVideo);

                    AdsManager.ResetInterstitialDelayTime();
                }

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: OnUnityAdsDidFinish - " + placementId + ". Result - " + showResult);
            }

            public void OnUnityAdsDidStart(string placementId)
            {
                if (placementId == placementInterstitialID)
                {
                    AdsManager.OnProviderAdDisplayed(AdProvider.UnityAdsLegacy, AdType.Interstitial);
                }
                else if (placementId == placementRewardedVideoID)
                {
                    AdsManager.OnProviderAdDisplayed(AdProvider.UnityAdsLegacy, AdType.RewardedVideo);
                }

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: OnUnityAdsDidStart - " + placementId);
            }

            public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
            {
                if (placementId == placementInterstitialID)
                {
                    adsHandler.interstitialIsLoaded = false;
                }
                else if (placementId == placementRewardedVideoID)
                {
                    adsHandler.rewardVideoIsLoaded = false;
                }

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: OnUnityAdsFailedToLoad - " + placementId + ". Error - " + error + " .Message: " + message );
            }

            public void OnUnityAdsReady(string placementId)
            {
                if (placementId == placementBannerID)
                {
                    AdsManager.OnProviderAdLoaded(AdProvider.UnityAdsLegacy, AdType.Banner);
                }
                else if (placementId == placementInterstitialID)
                {
                    AdsManager.OnProviderAdLoaded(AdProvider.UnityAdsLegacy, AdType.Interstitial);
                }
                else if (placementId == placementRewardedVideoID)
                {
                    AdsManager.OnProviderAdLoaded(AdProvider.UnityAdsLegacy, AdType.RewardedVideo);
                }

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: OnUnityAdsReady - " + placementId);
            }

            public void OnUnityAdsShowClick(string placementId)
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: OnUnityAdsFailedToLoad - " + placementId);
            }

            public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
            {
                if (placementId == placementInterstitialID)
                {
                    adsHandler.interstitialIsLoaded = false;

                    AdsManager.OnProviderAdClosed(AdProvider.UnityAdsLegacy, AdType.Interstitial);

                    AdsManager.ExecuteInterstitialCallback(showCompletionState == UnityAdsShowCompletionState.COMPLETED);

                    AdsManager.ResetInterstitialDelayTime();

                    AdsManager.RequestInterstitial();
                }
                else if (placementId == placementRewardedVideoID)
                {
                    adsHandler.rewardVideoIsLoaded = false;

                    bool state = showCompletionState == UnityAdsShowCompletionState.COMPLETED;

                    // Reward the player
                    AdsManager.ExecuteRewardVideoCallback(state);

                    AdsManager.OnProviderAdClosed(AdProvider.UnityAdsLegacy, AdType.RewardedVideo);

                    AdsManager.ResetInterstitialDelayTime();

                    AdsManager.RequestRewardBasedVideo();
                }

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: OnUnityAdsDidFinish - " + placementId + ". Result - " + showCompletionState);
            }

            public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: OnUnityAdsShowFailure - " + placementId + " - " + message);

                if(error == UnityAdsShowError.NOT_READY)
                {
                    if (placementId == placementInterstitialID)
                    {
                        AdsManager.RequestInterstitial();
                    }
                    else if (placementId == placementRewardedVideoID)
                    {
                        AdsManager.RequestRewardBasedVideo();
                    }
                }
            }

            public void OnUnityAdsShowStart(string placementId)
            {
                if (placementId == placementInterstitialID)
                {
                    AdsManager.OnProviderAdLoaded(AdProvider.UnityAdsLegacy, AdType.Interstitial);
                }
                else if (placementId == placementRewardedVideoID)
                {
                    AdsManager.OnProviderAdLoaded(AdProvider.UnityAdsLegacy, AdType.RewardedVideo);
                }

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: OnUnityAdsShowStart - " + placementId);
            }
        }
    }
#endif
}