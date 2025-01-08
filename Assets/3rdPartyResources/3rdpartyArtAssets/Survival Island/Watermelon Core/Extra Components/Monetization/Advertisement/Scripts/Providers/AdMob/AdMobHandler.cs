using UnityEngine;
using System.Collections.Generic;

#if MODULE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Watermelon
{
#if MODULE_ADMOB
    public class AdMobHandler : AdProviderHandler
    {
        private const int RETRY_ATTEMPT_DEFAULT_VALUE = 1;

        private int interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;
        private int rewardedRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardedAd rewardBasedVideo;

        public AdMobHandler(AdProvider providerType) : base(providerType) 
        {

        }

        public override void Initialise(AdsSettings adsSettings)
        {
            this.adsSettings = adsSettings;

            if (Monetization.VerboseLogging)
                Debug.Log("[AdsManager]: AdMob is trying to initialize!");

            MobileAds.SetiOSAppPauseOnBackground(true);

            RequestConfiguration requestConfiguration = new RequestConfiguration()
            {
                TagForChildDirectedTreatment = TagForChildDirectedTreatment.Unspecified,
                TestDeviceIds = adsSettings.AdMobContainer.TestDevicesIDs
            };

            MobileAds.SetRequestConfiguration(requestConfiguration);

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(InitCompleteAction);
        }

        private void InitCompleteAction(InitializationStatus initStatus)
        {
            GoogleMobileAds.Common.MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                OnProviderInitialised();
            });
        }

        public override void DestroyBanner()
        {
            if (bannerView != null)
                bannerView.Destroy();
        }

        public override void HideBanner()
        {
            if (bannerView != null)
                bannerView.Hide();
        }

        public override void RequestInterstitial()
        {
            // Clean up interstitial ad before creating a new one.
            if (interstitial != null)
            {
                interstitial.Destroy();
            }

            InterstitialAd.Load(GetInterstitialID(), GetAdRequest(), (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    if (Monetization.VerboseLogging)
                        Debug.Log("[AdsManager]: Interstitial ad failed to load an ad with error: " + error);

                    interstitialRetryAttempt++;
                    float retryDelay = Mathf.Pow(2, interstitialRetryAttempt);

                    Tween.DelayedCall(interstitialRetryAttempt, () => AdsManager.RequestInterstitial(), true, UpdateMethod.Update);

                    return;
                }

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Interstitial ad loaded with response: " + ad.GetResponseInfo());

                interstitial = ad;

                interstitialRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                AdsManager.OnProviderAdLoaded(providerType, AdType.Interstitial);

                // Register for ad events.
                interstitial.OnAdFullScreenContentOpened += HandleInterstitialOpened;
                interstitial.OnAdFullScreenContentClosed += HandleInterstitialClosed;
                interstitial.OnAdClicked += HandleInterstitialClicked;
            });
        }

        public override void RequestRewardedVideo()
        {
            RewardedAd.Load(GetRewardedVideoID(), GetAdRequest(), (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    AdsManager.ExecuteRewardVideoCallback(false);

                    if (Monetization.VerboseLogging)
                        Debug.Log("[AdsManager]: HandleRewardBasedVideoFailedToLoad event received with message: " + error);

                    rewardedRetryAttempt++;
                    float retryDelay = Mathf.Pow(2, rewardedRetryAttempt);

                    Tween.DelayedCall(rewardedRetryAttempt, () => AdsManager.RequestRewardBasedVideo(), true, UpdateMethod.Update);
                    
                    return;
                }

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Rewarded ad loaded with response: " + ad.GetResponseInfo());

                rewardedRetryAttempt = RETRY_ATTEMPT_DEFAULT_VALUE;

                AdsManager.OnProviderAdLoaded(providerType, AdType.RewardedVideo);

                rewardBasedVideo = ad;
                rewardBasedVideo.OnAdFullScreenContentFailed += HandleRewardBasedVideoFailedToShow;
                rewardBasedVideo.OnAdFullScreenContentOpened += HandleRewardBasedVideoOpened;
                rewardBasedVideo.OnAdFullScreenContentClosed += HandleRewardBasedVideoClosed;
                rewardBasedVideo.OnAdClicked += HandleRewardBasedVideoClicked;
            });
        }

        private void RequestBanner()
        {
            // Clean up banner before reusing
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            AdSize adSize = AdSize.Banner;

            switch (adsSettings.AdMobContainer.BannerType)
            {
                case AdMobContainer.BannerPlacementType.Banner:
                    adSize = AdSize.Banner;
                    break;
                case AdMobContainer.BannerPlacementType.MediumRectangle:
                    adSize = AdSize.MediumRectangle;
                    break;
                case AdMobContainer.BannerPlacementType.IABBanner:
                    adSize = AdSize.IABBanner;
                    break;
                case AdMobContainer.BannerPlacementType.Leaderboard:
                    adSize = AdSize.Leaderboard;
                    break;
            }

            AdPosition adPosition = AdPosition.Bottom;
            switch (adsSettings.AdMobContainer.BannerPosition)
            {
                case BannerPosition.Bottom:
                    adPosition = AdPosition.Bottom;
                    break;
                case BannerPosition.Top:
                    adPosition = AdPosition.Top;
                    break;
            }

            bannerView = new BannerView(GetBannerID(), adSize, adPosition);

            // Register for ad events.
            bannerView.OnBannerAdLoaded += HandleAdLoaded;
            bannerView.OnBannerAdLoadFailed += HandleAdFailedToLoad;
            bannerView.OnAdPaid += HandleAdPaid;
            bannerView.OnAdClicked += HandleAdClicked;
            bannerView.OnAdFullScreenContentClosed += HandleAdClosed;

            // Load a banner ad.
            bannerView.LoadAd(GetAdRequest());
        }

        public override void ShowBanner()
        {
            if (bannerView == null)
                RequestBanner();

            if(bannerView != null)
                bannerView.Show();
        }

        public override void ShowInterstitial(InterstitialCallback callback)
        {
            interstitial.Show();
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            rewardBasedVideo.Show((GoogleMobileAds.Api.Reward reward) =>
            {
                AdsManager.CallEventInMainThread(delegate
                {
                    AdsManager.OnProviderAdDisplayed(providerType, AdType.RewardedVideo);

                    AdsManager.ExecuteRewardVideoCallback(true);

                    if (Monetization.VerboseLogging)
                        Debug.Log("[AdsManager]: HandleRewardBasedVideoRewarded event received");

                    AdsManager.ResetInterstitialDelayTime();
                    AdsManager.RequestRewardBasedVideo();
                });
            });
        }

        public override bool IsInterstitialLoaded()
        {
            return interstitial != null && interstitial.CanShowAd();
        }

        public override bool IsRewardedVideoLoaded()
        {
            return rewardBasedVideo != null && rewardBasedVideo.CanShowAd();
        }

        public AdRequest GetAdRequest()
        {
            return new AdRequest()
            {
                Extras = new Dictionary<string, string>()
                {
                    { "npa", AdsManager.GetGDPRState() ? "1" : "0" }
                }
            };
        }

    #region Banner Callbacks
        public void HandleAdLoaded()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleAdLoaded event received");

                AdsManager.OnProviderAdLoaded(providerType, AdType.Banner);
            });
        }

        public void HandleAdFailedToLoad(LoadAdError error)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleFailedToReceiveAd event received with message: " + error.GetMessage());
            });
        }

        private void HandleAdPaid(AdValue adValue)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleAdPaid event received");
            });
        }

        public void HandleAdClicked()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleAdClicked event received");
            });
        }

        public void HandleAdClosed()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleAdClosed event received");

                AdsManager.OnProviderAdClosed(providerType, AdType.Banner);
            });
        }
    #endregion

    #region Interstitial Callback
        public void HandleInterstitialOpened()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleInterstitialOpened event received");

                AdsManager.OnProviderAdDisplayed(providerType, AdType.Interstitial);
            });
        }

        public void HandleInterstitialClosed()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleInterstitialClosed event received");

                AdsManager.OnProviderAdClosed(providerType, AdType.Interstitial);

                AdsManager.ExecuteInterstitialCallback(true);

                AdsManager.ResetInterstitialDelayTime();
                AdsManager.RequestInterstitial();
            });
        }

        private void HandleInterstitialClicked()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleInterstitialClicked event received");
            });
        }
    #endregion

    #region RewardedVideo Callback
        private void HandleRewardBasedVideoFailedToShow(AdError error)
        {
            AdsManager.CallEventInMainThread(delegate
            {
                AdsManager.ExecuteRewardVideoCallback(false);

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoFailedToShow event received with message: " + error);

                rewardedRetryAttempt++;
                float retryDelay = Mathf.Pow(2, rewardedRetryAttempt);

                Tween.DelayedCall(rewardedRetryAttempt, () => AdsManager.RequestRewardBasedVideo(), true, UpdateMethod.Update);
            });
        }

        public void HandleRewardBasedVideoOpened()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoOpened event received");

#if UNITY_EDITOR
                //fix that helps display ads over store
                UnityEngine.Object[] canvases = GameObject.FindObjectsOfType(typeof(Canvas), false);
                var regex = new System.Text.RegularExpressions.Regex("[0-9]{3,4}x[0-9]{3,4}\\(Clone\\)");

                for (int i = 0; i < canvases.Length; i++)
                {
                    if (regex.IsMatch(canvases[i].name))
                    {
                        ((Canvas)canvases[i]).sortingOrder = 9999;

                        break;
                    }
                }
#endif
            });
        }

        public void HandleRewardBasedVideoClosed()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoClosed event received");

                AdsManager.OnProviderAdClosed(providerType, AdType.RewardedVideo);
            });
        }

        private void HandleRewardBasedVideoClicked()
        {
            AdsManager.CallEventInMainThread(delegate
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: HandleRewardBasedVideoClicked event received");
            });
        }
        #endregion

        public string GetBannerID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.AdMobContainer.AndroidBannerID;
#elif UNITY_IOS
            return adsSettings.AdMobContainer.IOSBannerID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetInterstitialID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.AdMobContainer.AndroidInterstitialID;
#elif UNITY_IOS
            return adsSettings.AdMobContainer.IOSInterstitialID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetRewardedVideoID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.AdMobContainer.AndroidRewardedVideoID;
#elif UNITY_IOS
            return adsSettings.AdMobContainer.IOSRewardedVideoID;
#else
            return "unexpected_platform";
#endif
        }
    }
#endif
}