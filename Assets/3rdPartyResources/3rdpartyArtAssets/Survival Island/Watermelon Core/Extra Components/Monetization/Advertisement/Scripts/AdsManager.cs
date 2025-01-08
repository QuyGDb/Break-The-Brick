#pragma warning disable 0649
#pragma warning disable 0162

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace Watermelon
{
    [Define("MODULE_ADMOB", "GoogleMobileAds.Editor.GoogleMobileAdsSettings", new string[] { "Assets/GoogleMobileAds/GoogleMobileAds.dll" })]
    [Define("MODULE_UNITYADS", "UnityEngine.Advertisements.Advertisement", new string[] { "Packages/com.unity.ads/Runtime/Advertisement/Advertisement.cs" })]
    [Define("MODULE_IRONSOURCE", "IronSource", new string[] { "Assets/IronSource/Scripts/IronSource.cs", "Assets/LevelPlay/Runtime/IronSource.cs" })]
    public static class AdsManager
    {
        private const int INIT_ATTEMPTS_AMOUNT = 30;

        private const string FIRST_LAUNCH_PREFS = "FIRST_LAUNCH";

        private const string GDPR_PREF_NAME = "GDPR_STATE";

        private static readonly AdProviderHandler[] AD_PROVIDERS = new AdProviderHandler[]
        {
            new AdDummyHandler(AdProvider.Dummy), 

#if MODULE_ADMOB
            new AdMobHandler(AdProvider.AdMob), 
#endif

#if MODULE_UNITYADS
            new UnityAdsLegacyHandler(AdProvider.UnityAdsLegacy), 
#endif

#if MODULE_IRONSOURCE
            new IronSourceHandler(AdProvider.IronSource),
#endif
        };

        private static bool isModuleInitialised;

        private static AdsSettings settings;
        public static AdsSettings Settings => settings;

        private static double lastInterstitialTime;

        private static AdProviderHandler.RewardedVideoCallback rewardedVideoCallback;
        private static AdProviderHandler.InterstitialCallback interstitalCallback;

        private static List<SimpleCallback> mainThreadEvents = new List<SimpleCallback>();
        private static int mainThreadEventsCount;

        private static bool isFirstAdLoaded = false;
        private static bool waitingForRewardVideoCallback;

        private static bool isBannerActive = true;

        private static Coroutine loadingCoroutine;

        private static Dictionary<AdProvider, AdProviderHandler> advertisingActiveModules = new Dictionary<AdProvider, AdProviderHandler>();

        // Events
        public static event SimpleCallback ForcedAdDisabled;

        public static event AdsModuleCallback AdProviderInitialised;
        public static event AdsEventsCallback AdLoaded;
        public static event AdsEventsCallback AdDisplayed;
        public static event AdsEventsCallback AdClosed;

        public static AdsBoolCallback InterstitialConditions;

        private static AdSave save;

        #region Initialise
        public static void Initialise(MonetizationSettings monetizationSettings)
        {
            if (isModuleInitialised)
            {
                Debug.LogWarning("[AdsManager]: Module already exists!");

                return;
            }

            isModuleInitialised = true;
            isFirstAdLoaded = false;

            settings = monetizationSettings.AdsSettings;

            save = SaveController.GetSaveObject<AdSave>("advertisement_forced_ad");

            if (settings == null)
            {
                Debug.LogError("[AdsManager]: Settings don't exist!");

                return;
            }

            if (!PlayerPrefs.HasKey(FIRST_LAUNCH_PREFS))
            {
                lastInterstitialTime = Time.time + settings.InterstitialFirstStartDelay;

                PlayerPrefs.SetInt(FIRST_LAUNCH_PREFS, 1);
            }
            else
            {
                lastInterstitialTime = Time.time + settings.InterstitialStartDelay;
            }

            Initialiser.InitialiserGameObject.AddComponent<AdsManager.AdEventExecutor>();

            advertisingActiveModules = new Dictionary<AdProvider, AdProviderHandler>();
            for (int i = 0; i < AD_PROVIDERS.Length; i++)
            {
                if (IsModuleEnabled(AD_PROVIDERS[i].ProviderType))
                {
                    advertisingActiveModules.Add(AD_PROVIDERS[i].ProviderType, AD_PROVIDERS[i]);
                }
            }

            if (Monetization.VerboseLogging)
            {
                if (settings.BannerType != AdProvider.Disable && !advertisingActiveModules.ContainsKey(settings.BannerType))
                    Debug.LogWarning("[AdsManager]: Banner type (" + settings.BannerType + ") is selected, but isn't active!");

                if (settings.InterstitialType != AdProvider.Disable && !advertisingActiveModules.ContainsKey(settings.InterstitialType))
                    Debug.LogWarning("[AdsManager]: Interstitial type (" + settings.InterstitialType + ") is selected, but isn't active!");

                if (settings.RewardedVideoType != AdProvider.Disable && !advertisingActiveModules.ContainsKey(settings.RewardedVideoType))
                    Debug.LogWarning("[AdsManager]: Rewarded Video type (" + settings.RewardedVideoType + ") is selected, but isn't active!");
            }

            // Add loading task if GDPR isn't created
            if (settings.IsGDPREnabled && !IsGDPRStateExist())
            {
                GDPRLoadingTask gdprLoadingTask = new GDPRLoadingTask();
                gdprLoadingTask.OnTaskCompleted += () =>
                {
                    InitialiseModules(settings.LoadAdsOnStart);
                };

                GameLoading.AddTask(gdprLoadingTask);

                return;
            }

            InitialiseModules(settings.LoadAdsOnStart);

#if UNITY_IOS
            if (settings.IsIDFAEnabled && !IsIDFADetermined())
            {
                if (Monetization.VerboseLogging)
                    Debug.Log("[Ads Manager]: Requesting IDFA..");

                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif
        }

        private static void InitialiseModules(bool loadAds)
        {
            foreach (var advertisingModule in advertisingActiveModules.Keys)
            {
                InitialiseModule(advertisingModule);
            }

            if (loadAds)
            {
                TryToLoadFirstAds();
            }
        }

        private static void InitialiseModule(AdProvider advertisingModule)
        {
            if (advertisingActiveModules.ContainsKey(advertisingModule))
            {
                if (!advertisingActiveModules[advertisingModule].IsInitialised())
                {
                    if (Monetization.VerboseLogging)
                        Debug.Log("[AdsManager]: Module " + advertisingModule.ToString() + " trying to initialise!");

                    advertisingActiveModules[advertisingModule].Initialise(settings);
                }
                else
                {
                    if (Monetization.VerboseLogging)
                        Debug.Log("[AdsManager]: Module " + advertisingModule.ToString() + " is already initialised!");
                }
            }
            else
            {
                if (Monetization.VerboseLogging)
                    Debug.LogWarning("[AdsManager]: Module " + advertisingModule.ToString() + " is disabled!");
            }
        }
        #endregion

        private static void Update()
        {
            if (mainThreadEventsCount > 0)
            {
                for (int i = 0; i < mainThreadEventsCount; i++)
                {
                    mainThreadEvents[i]?.Invoke();
                }

                mainThreadEvents.Clear();
                mainThreadEventsCount = 0;
            }

            if (settings.AutoShowInterstitial)
            {
                if (lastInterstitialTime < Time.time)
                {
                    ShowInterstitial(null);

                    ResetInterstitialDelayTime();
                }
            }
        }

        public static void TryToLoadFirstAds()
        {
            if (loadingCoroutine == null)
                loadingCoroutine = Tween.InvokeCoroutine(TryToLoadAdsCoroutine());
        }

        private static IEnumerator TryToLoadAdsCoroutine()
        {
            int initAttemps = 0;

            yield return new WaitForSeconds(1.0f);

            while (!isFirstAdLoaded || initAttemps > INIT_ATTEMPTS_AMOUNT)
            {
                if (LoadFirstAds())
                    break;

                yield return new WaitForSeconds(1.0f * (initAttemps + 1));

                initAttemps++;
            }

            if (Monetization.VerboseLogging)
                Debug.Log("[AdsManager]: First ads have loaded!");
        }

        private static bool LoadFirstAds()
        {
            if (isFirstAdLoaded)
                return true;

            if (settings.IsGDPREnabled && !AdsManager.IsGDPRStateExist())
                return false;

            if (settings.IsIDFAEnabled && !AdsManager.IsIDFADetermined())
                return false;

            bool isRewardedVideoModuleInititalized = AdsManager.IsModuleInititalized(AdsManager.Settings.RewardedVideoType);
            bool isInterstitialModuleInitialized = AdsManager.IsModuleInititalized(AdsManager.Settings.InterstitialType);
            bool isBannerModuleInitialized = AdsManager.IsModuleInititalized(AdsManager.Settings.BannerType);

            bool isRewardedVideoActive = AdsManager.Settings.RewardedVideoType != AdProvider.Disable;
            bool isInterstitialActive = AdsManager.Settings.InterstitialType != AdProvider.Disable;
            bool isBannerActive = AdsManager.Settings.BannerType != AdProvider.Disable;

            if ((!isRewardedVideoActive || isRewardedVideoModuleInititalized) && (!isInterstitialActive || isInterstitialModuleInitialized) && (!isBannerActive || isBannerModuleInitialized))
            {
                if (isRewardedVideoActive)
                    AdsManager.RequestRewardBasedVideo();

                bool isForcedAdEnabled = AdsManager.IsForcedAdEnabled();
                if (isInterstitialActive && isForcedAdEnabled)
                    AdsManager.RequestInterstitial();

                if (isBannerActive && isForcedAdEnabled)
                    AdsManager.ShowBanner();

                isFirstAdLoaded = true;

                return true;
            }

            return false;
        }

        public static void CallEventInMainThread(SimpleCallback callback)
        {
            if (callback != null)
            {
                mainThreadEvents.Add(callback);
                mainThreadEventsCount++;
            }
        }

        public static void ShowErrorMessage()
        {
            FloatingMessage.ShowMessage("Network error. Please try again later");
        }

        public static bool IsModuleEnabled(AdProvider advertisingModule)
        {
            if (!Monetization.IsActive || !isModuleInitialised)
                return false;

            if (advertisingModule == AdProvider.Disable)
                return false;

            return (Settings.BannerType == advertisingModule || Settings.InterstitialType == advertisingModule || Settings.RewardedVideoType == advertisingModule);
        }

        public static bool IsModuleActive(AdProvider advertisingModule)
        {
            return advertisingActiveModules.ContainsKey(advertisingModule);
        }

        public static bool IsModuleInititalized(AdProvider advertisingModule)
        {
            if (advertisingActiveModules.ContainsKey(advertisingModule))
            {
                return advertisingActiveModules[advertisingModule].IsInitialised();
            }

            return false;
        }

        #region Interstitial
        public static bool IsInterstitialLoaded()
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                return false;
            }

            AdProvider advertisingModules = settings.InterstitialType;

            if (!save.IsForcedAdEnabled || !IsModuleActive(advertisingModules))
                return false;

            return advertisingActiveModules[advertisingModules].IsInterstitialLoaded();
        }

        public static void RequestInterstitial()
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                return;
            }

            AdProvider advertisingModules = settings.InterstitialType;

            if (!save.IsForcedAdEnabled || !IsModuleActive(advertisingModules) || !advertisingActiveModules[advertisingModules].IsInitialised() || advertisingActiveModules[advertisingModules].IsInterstitialLoaded())
                return;

            advertisingActiveModules[advertisingModules].RequestInterstitial();
        }

        public static void ShowInterstitial(AdProviderHandler.InterstitialCallback callback, bool ignoreConditions = false)
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                ExecuteInterstitialCallback(false);

                return;
            }

            AdProvider advertisingModules = settings.InterstitialType;

            interstitalCallback = callback;

            if (!save.IsForcedAdEnabled || !IsModuleActive(advertisingModules) || (!ignoreConditions && (!CheckInterstitialTime() || !CheckExtraInterstitialCondition())) || !advertisingActiveModules[advertisingModules].IsInitialised() || !advertisingActiveModules[advertisingModules].IsInterstitialLoaded())
            {
                ExecuteInterstitialCallback(false);

                return;
            }

            advertisingActiveModules[advertisingModules].ShowInterstitial(callback);
        }

        public static void ExecuteInterstitialCallback(bool result)
        {
            if (interstitalCallback != null)
            {
                CallEventInMainThread(() => interstitalCallback.Invoke(result));
            }
        }

        public static void SetInterstitialDelayTime(float time)
        {
            lastInterstitialTime = Time.time + time;
        }

        public static void ResetInterstitialDelayTime()
        {
            lastInterstitialTime = Time.time + settings.InterstitialShowingDelay;
        }

        private static bool CheckInterstitialTime()
        {
            if (Monetization.VerboseLogging)
                Debug.Log("[AdsManager]: Interstitial Time: " + lastInterstitialTime + "; Time: " + Time.time);

            return lastInterstitialTime < Time.time;
        }

        public static bool CheckExtraInterstitialCondition()
        {
            if (InterstitialConditions != null)
            {
                bool state = true;

                System.Delegate[] listDelegates = InterstitialConditions.GetInvocationList();
                for (int i = 0; i < listDelegates.Length; i++)
                {
                    if (!(bool)listDelegates[i].DynamicInvoke())
                    {
                        state = false;

                        break;
                    }
                }

                if (Monetization.VerboseLogging)
                    Debug.Log("[AdsManager]: Extra condition interstitial state: " + state);

                return state;
            }

            return true;
        }
        #endregion

        #region Rewarded Video
        public static bool IsRewardBasedVideoLoaded()
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                return false;
            }

            AdProvider advertisingModule = settings.RewardedVideoType;

            if (!IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised())
                return false;

            return advertisingActiveModules[advertisingModule].IsRewardedVideoLoaded();
        }

        public static void RequestRewardBasedVideo()
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                return;
            }

            AdProvider advertisingModule = settings.RewardedVideoType;

            if (!IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised() || advertisingActiveModules[advertisingModule].IsRewardedVideoLoaded())
                return;

            advertisingActiveModules[advertisingModule].RequestRewardedVideo();
        }

        public static void ShowRewardBasedVideo(AdProviderHandler.RewardedVideoCallback callback, bool showErrorMessage = true)
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                ExecuteRewardVideoCallback(false);

                return;
            }

            AdProvider advertisingModule = settings.RewardedVideoType;

            rewardedVideoCallback = callback;
            waitingForRewardVideoCallback = true;

            if (!IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised() || !advertisingActiveModules[advertisingModule].IsRewardedVideoLoaded())
            {
                ExecuteRewardVideoCallback(false);

                if (showErrorMessage)
                    ShowErrorMessage();

                return;
            }

            advertisingActiveModules[advertisingModule].ShowRewardedVideo(callback);
        }

        public static void ExecuteRewardVideoCallback(bool result)
        {
            if (rewardedVideoCallback != null && waitingForRewardVideoCallback)
            {
                CallEventInMainThread(() => rewardedVideoCallback.Invoke(result));

                waitingForRewardVideoCallback = false;

                if (Monetization.VerboseLogging)
                {
                    Debug.Log("[AdsManager]: Reward received: " + result);
                }
            }
        }
        #endregion

        #region Banner
        public static void ShowBanner()
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                return;
            }

            if (!isBannerActive) return;

            AdProvider advertisingModule = settings.BannerType;

            if (!save.IsForcedAdEnabled || !IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised())
                return;

            advertisingActiveModules[advertisingModule].ShowBanner();
        }

        public static void DestroyBanner()
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                return;
            }

            AdProvider advertisingModule = settings.BannerType;

            if (!IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised())
                return;

            advertisingActiveModules[advertisingModule].DestroyBanner();
        }

        public static void HideBanner()
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                return;
            }

            AdProvider advertisingModule = settings.BannerType;

            if (!IsModuleActive(advertisingModule) || !advertisingActiveModules[advertisingModule].IsInitialised())
                return;

            advertisingActiveModules[advertisingModule].HideBanner();
        }

        public static void EnableBanner()
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                return;
            }

            isBannerActive = true;

            ShowBanner();
        }

        public static void DisableBanner()
        {
            if (!Monetization.IsActive || !isModuleInitialised)
            {
                Debug.LogWarning("[IAP Manager]: Mobile monetization is disabled!");

                return;
            }

            isBannerActive = false;

            HideBanner();
        }
        #endregion

        public static void OnProviderInitialised(AdProvider advertisingModule)
        {
            AdProviderInitialised?.Invoke(advertisingModule);
        }

        public static void OnProviderAdLoaded(AdProvider advertisingModule, AdType advertisingType)
        {
            AdLoaded?.Invoke(advertisingModule, advertisingType);
        }

        public static void OnProviderAdDisplayed(AdProvider advertisingModule, AdType advertisingType)
        {
            AdDisplayed?.Invoke(advertisingModule, advertisingType);

            if (advertisingType == AdType.Interstitial || advertisingType == AdType.RewardedVideo)
            {
                ResetInterstitialDelayTime();
            }
        }

        public static void OnProviderAdClosed(AdProvider advertisingModule, AdType advertisingType)
        {
            AdClosed?.Invoke(advertisingModule, advertisingType);

            if (advertisingType == AdType.Interstitial || advertisingType == AdType.RewardedVideo)
            {
                ResetInterstitialDelayTime();
            }
        }

        #region IAP
        public static bool IsForcedAdEnabled()
        {
            return save.IsForcedAdEnabled;
        }

        public static void DisableForcedAd()
        {
            Debug.Log("[Ads Manager]: Banners and interstitials are disabled!");

            save.IsForcedAdEnabled = false;

            NotchSaveArea.Refresh(true);

            ForcedAdDisabled?.Invoke();

            DestroyBanner();
        }
        #endregion

        #region GDPR
        public static void SetGDPR(bool state)
        {
            PlayerPrefs.SetInt(GDPR_PREF_NAME, state ? 1 : 0);

            foreach (AdProvider activeModule in advertisingActiveModules.Keys)
            {
                if (advertisingActiveModules[activeModule].IsInitialised())
                {
                    advertisingActiveModules[activeModule].SetGDPR(state);
                }
                else
                {
                    InitialiseModule(activeModule);
                }
            }
        }

        public static bool GetGDPRState()
        {
            return PlayerPrefs.GetInt(GDPR_PREF_NAME, 0) == 1 ? true : false;
        }

        public static bool IsGDPRStateExist()
        {
            return PlayerPrefs.HasKey(GDPR_PREF_NAME);
        }
        #endregion

        #region IDFA
        public static bool IsIDFADetermined()
        {
#if UNITY_IOS
            if(settings.IsIDFAEnabled)
            {
                return Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() != Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED;
            }
#endif

            return true;
        }
        #endregion

        public delegate void AdsModuleCallback(AdProvider advertisingModules);
        public delegate void AdsEventsCallback(AdProvider advertisingModules, AdType advertisingType);
        public delegate bool AdsBoolCallback();

        private class AdEventExecutor : MonoBehaviour
        {
            private void Update()
            {
                AdsManager.Update();
            }
        }
    }
}

// -----------------
// Advertisement v1.4.2
// -----------------

// Changelog
// v1.4.2
// • Added EnableBanner, DisableBanner methods
// v1.4.1
// • Added ironSource (Unity LevelPlay) ad provider
// v1.4
// • Admob v9.0.0 support
// • Better naming and code cleanup
// • Ads callbacks replaced with simplified ones (AdLoaded, AdDisplayed, AdClosed)
// • Removed ShowInterstitial, ShowRewardedVideo, ShowBanner methods with provider type parameter
// • Added optional bool parameter to ShowInterstitial method. Allows to show interstitial even if conditions aren't met
// v1.3
// • Admob v8.1.0 support
// • Removed IronSource provider
// v1.2.1
// • Some fixes in IronSourse provider
// • Some fixes in Admob provider
// • New interface in Admob provider
// • Added Build Preprocessing for Admob 
// v1.2
// • Added IronSource provider
// v1.1f3
// • GDPR style rework
// • Rewarded video error message
// • Removed GDPR check in AdMob module
// v1.1f2
// • GDPR init bug fixed
// v1.1
// • Added first ad loader
// • Moved IAP check to AdsManager script
// v1.0
// • Added documentation
// v0.3
// • Unity Ads fixed
// v0.2
// • Bug fix
// v0.1
// • Added basic version