using UnityEngine;

namespace Watermelon
{
#if MODULE_IRONSOURCE
    public class IronSourceHandler : AdProviderHandler
    {
        private const int RETRY_ATTEMPT_DEFAULT_VALUE = 1;

        private int interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;
        private int rewardedRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

        private bool isBannerLoaded = false;

        private IronSourceListner eventsHolder;

        public IronSourceHandler(AdProvider moduleType) : base(moduleType) { }

        public override void Initialise(AdsSettings adsSettings)
        {
            this.adsSettings = adsSettings;

            eventsHolder = Initialiser.InitialiserGameObject.AddComponent<IronSourceListner>();
            eventsHolder.Initialise(this);

            if (adsSettings.RewardedVideoType == AdProvider.IronSource)
            {
                //Add AdInfo Rewarded Video Events
                IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
                IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
                IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
                IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
            }

            if (adsSettings.InterstitialType == AdProvider.IronSource)
            {
                //Add AdInfo Interstitial Events
                IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
                IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
                IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
                IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
                IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;
            }

            if (adsSettings.BannerType == AdProvider.IronSource)
            {
                //Add AdInfo Banner Events
                IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
            }

            IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;

            if (Monetization.VerboseLogging)
                Debug.Log("[AdsManager]: IronSource trying to initialise..");

            if(Monetization.DebugMode)
                IronSource.Agent.setMetaData("is_test_suite", "enable");

            IronSource.Agent.init(GetAppKey());
        }

        private void SdkInitializationCompletedEvent()
        {
            if (Monetization.VerboseLogging)
            {
                Debug.Log("[AdsManager]: IronSource is initialised!");

                IronSource.Agent.validateIntegration();
            }

            if (Monetization.DebugMode)
            {
                IronSource.Agent.launchTestSuite();
            }

            OnProviderInitialised();
        }

        #region RewardedAd callback handlers
        // The Rewarded Video ad view has opened. Your activity will loose focus.
        private void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdOpenedEvent event received");

                AdsManager.OnProviderAdDisplayed(AdProvider.IronSource, AdType.RewardedVideo);
            });
        }

        // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
        private void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdClosedEvent event received");

                AdsManager.OnProviderAdClosed(AdProvider.IronSource, AdType.RewardedVideo);
            });
        }

        // The user completed to watch the video, and should be rewarded.
        // The placement parameter will include the reward data.
        // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
        private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(true);

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdRewardedEvent event received");

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestRewardBasedVideo();
            });
        }

        // The rewarded video ad was failed to show.
        private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(false);

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: RewardedVideoOnAdShowFailedEvent event received with message: " + error);

                rewardedRetryAttempt++;
                float retryDelay = Mathf.Pow(2, rewardedRetryAttempt);

                Tween.DelayedCall(rewardedRetryAttempt, () => AdsManager.RequestRewardBasedVideo(), true, UpdateMethod.Update);
            });
        }
        #endregion

        #region Interstitial callback handlers
        /************* Interstitial AdInfo Delegates *************/
        // Invoked when the interstitial ad was loaded succesfully.
        private void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Interstitial ad loaded");

                interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                AdsManager.OnProviderAdLoaded(AdProvider.IronSource, AdType.Interstitial);
            });
        }

        // Invoked when the initialization process has failed.
        private void InterstitialOnAdLoadFailed(IronSourceError ironSourceError)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Interstitial ad failed to load an ad with error: " + ironSourceError);

                interstitialRetryAttempt++;
                float retryDelay = Mathf.Pow(2, interstitialRetryAttempt);

                Tween.DelayedCall(interstitialRetryAttempt, () => AdsManager.RequestInterstitial(), true, UpdateMethod.Update);
            });
        }

        // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
        private void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: InterstitialOnAdOpenedEvent event received");

                AdsManager.OnProviderAdDisplayed(AdProvider.IronSource, AdType.Interstitial);
            });
        }

        // Invoked when the ad failed to show.
        private void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Interstitial ad failed to load an ad with error: " + ironSourceError);

                interstitialRetryAttempt++;
                float retryDelay = Mathf.Pow(2, interstitialRetryAttempt);

                Tween.DelayedCall(interstitialRetryAttempt, () => AdsManager.RequestInterstitial(), true, UpdateMethod.Update);
            });
        }

        // Invoked when the interstitial ad closed and the user went back to the application screen.
        private void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: InterstitialOnAdClosedEvent event received");

                AdsManager.OnProviderAdClosed(AdProvider.IronSource, AdType.Interstitial);

                AdsManager.ExecuteInterstitialCallback(true);

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestInterstitial();
            });
        }
        #endregion

        #region Banner callback handlers
        //Invoked once the banner has loaded
        private void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: BannerOnAdLoadedEvent event received");

                AdsManager.OnProviderAdLoaded(AdProvider.IronSource, AdType.Banner);
            });
        }
    #endregion

        public override void DestroyBanner()
        {
            IronSource.Agent.destroyBanner();

            isBannerLoaded = false;

            AdsManager.OnProviderAdClosed(AdProvider.IronSource, AdType.Banner);
        }

        public override void HideBanner()
        {
            if(isBannerLoaded)
                IronSource.Agent.hideBanner();

            AdsManager.OnProviderAdClosed(AdProvider.IronSource, AdType.Banner);
        }

        public override void ShowBanner()
        {
            if(!isBannerLoaded)
            {
                IronSourceBannerSize ironSourceBannerSize = IronSourceBannerSize.BANNER;
                switch (adsSettings.IronSourceContainer.BannerType)
                {
                    case IronSourceContainer.BannerPlacementType.Large:
                        ironSourceBannerSize = IronSourceBannerSize.LARGE;
                        break;
                    case IronSourceContainer.BannerPlacementType.Rectangle:
                        ironSourceBannerSize = IronSourceBannerSize.RECTANGLE;
                        break;
                    case IronSourceContainer.BannerPlacementType.Smart:
                        ironSourceBannerSize = IronSourceBannerSize.SMART;
                        break;
                }

                IronSourceBannerPosition ironSourceBannerPosition = IronSourceBannerPosition.BOTTOM;
                if (adsSettings.IronSourceContainer.BannerPosition == BannerPosition.Top)
                    ironSourceBannerPosition = IronSourceBannerPosition.TOP;

                IronSource.Agent.loadBanner(ironSourceBannerSize, ironSourceBannerPosition);

                isBannerLoaded = true;
            }
            else
            {
                IronSource.Agent.displayBanner();
            }

            AdsManager.OnProviderAdDisplayed(AdProvider.IronSource, AdType.Banner);
        }

        public override void RequestInterstitial()
        {
            IronSource.Agent.loadInterstitial();
        }

        public override void ShowInterstitial(InterstitialCallback callback)
        {
            IronSource.Agent.showInterstitial();
        }

        public override void RequestRewardedVideo()
        {
            // Do nothing
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            IronSource.Agent.showRewardedVideo();
        }

        public override bool IsInterstitialLoaded()
        {
            return IronSource.Agent.isInterstitialReady();
        }

        public override bool IsRewardedVideoLoaded()
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }

        public override void SetGDPR(bool state)
        {
            IronSource.Agent.setConsent(state);
        }

        public string GetAppKey()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.IronSourceContainer.AndroidAppKey;
#elif UNITY_IOS
            return adsSettings.IronSourceContainer.IOSAppKey;
#else
            return "unexpected_platform";
#endif
        }

        private class IronSourceListner : MonoBehaviour
        {
            private IronSourceHandler ironSourceHandler;

            public void Initialise(IronSourceHandler ironSourceHandler)
            {
                this.ironSourceHandler = ironSourceHandler;
            }

            private void OnApplicationPause(bool isPaused)
            {
                IronSource.Agent.onApplicationPause(isPaused);
            }
        }
    }
#endif
}