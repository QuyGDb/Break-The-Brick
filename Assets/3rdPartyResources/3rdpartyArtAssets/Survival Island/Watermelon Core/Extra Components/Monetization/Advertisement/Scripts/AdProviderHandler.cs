using UnityEngine;

namespace Watermelon
{
    public abstract class AdProviderHandler
    {
        protected AdProvider providerType;
        public AdProvider ProviderType => providerType;

        protected AdsSettings adsSettings;

        protected bool isInitialised = false;

        public AdProviderHandler(AdProvider providerType)
        {
            this.providerType = providerType;
        }

        public bool IsInitialised()
        {
            return isInitialised;
        }

        protected void OnProviderInitialised()
        {
            isInitialised = true;

            AdsManager.OnProviderInitialised(providerType);

            if (Monetization.VerboseLogging)
                Debug.Log(string.Format("[AdsManager]: {0} is initialized!", providerType));
        }

        public abstract void Initialise(AdsSettings adsSettings);

        public abstract void ShowBanner();
        public abstract void HideBanner();
        public abstract void DestroyBanner();

        public abstract void RequestInterstitial();
        public abstract void ShowInterstitial(InterstitialCallback callback);
        public abstract bool IsInterstitialLoaded();

        public abstract void RequestRewardedVideo();
        public abstract void ShowRewardedVideo(RewardedVideoCallback callback);
        public abstract bool IsRewardedVideoLoaded();

        public virtual void SetGDPR(bool state) { }

        public delegate void RewardedVideoCallback(bool hasReward);
        public delegate void InterstitialCallback(bool isDisplayed);
    }
}