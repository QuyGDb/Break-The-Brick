#pragma warning disable 0649
#pragma warning disable 0162

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace Watermelon
{
    public class AdSave : ISaveObject
    {
        public bool IsForcedAdEnabled = true;

        public void Flush()
        {

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