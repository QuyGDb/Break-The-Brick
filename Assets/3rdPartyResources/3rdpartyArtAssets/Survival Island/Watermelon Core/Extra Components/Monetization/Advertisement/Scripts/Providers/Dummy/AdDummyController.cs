#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    public class AdDummyController : MonoBehaviour
    {
        [SerializeField] GameObject bannerObject;

        [Space]
        [SerializeField] GameObject interstitialObject;

        [Space]
        [SerializeField] GameObject rewardedVideoObject;

        private RectTransform bannerRectTransform;

        private void Awake()
        {
            bannerRectTransform = (RectTransform)bannerObject.transform;

            // Toggle editor visibility
            bannerObject.ToggleVisibility(true);
            bannerObject.TogglePicking(true);

            DontDestroyOnLoad(gameObject);
        }

        public void Initialise(AdsSettings settings)
        {
            switch (settings.DummyContainer.BannerPosition)
            {
                case BannerPosition.Bottom:
                    bannerRectTransform.pivot = new Vector2(0.5f, 0.0f);

                    bannerRectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                    bannerRectTransform.anchorMax = new Vector2(1.0f, 0.0f);

                    bannerRectTransform.anchoredPosition = Vector2.zero;
                    break;
                case BannerPosition.Top:
                    bannerRectTransform.pivot = new Vector2(0.5f, 1.0f);

                    bannerRectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                    bannerRectTransform.anchorMax = new Vector2(1.0f, 1.0f);

                    bannerRectTransform.anchoredPosition = Vector2.zero;
                    break;
            }
        }

        public void ShowBanner()
        {
            bannerObject.SetActive(true);
        }

        public void HideBanner()
        {
            bannerObject.SetActive(false);
        }

        public void ShowInterstitial()
        {
            interstitialObject.SetActive(true);
        }

        public void CloseInterstitial()
        {
            interstitialObject.SetActive(false);

            AdsManager.OnProviderAdClosed(AdProvider.Dummy, AdType.Interstitial);
        }

        public void ShowRewardedVideo()
        {
            rewardedVideoObject.SetActive(true);
        }

        public void CloseRewardedVideo()
        {
            rewardedVideoObject.SetActive(false);

            AdsManager.OnProviderAdClosed(AdProvider.Dummy, AdType.RewardedVideo);
        }

        #region Buttons
        public void CloseInterstitialButton()
        {
            AdsManager.ExecuteInterstitialCallback(true);

            CloseInterstitial();
        }

        public void CloseRewardedVideoButton()
        {
            AdsManager.ExecuteRewardVideoCallback(false);

            CloseRewardedVideo();
        }

        public void GetRewardButton()
        {
            AdsManager.ExecuteRewardVideoCallback(true);

            CloseRewardedVideo();
        }
        #endregion
    }
}