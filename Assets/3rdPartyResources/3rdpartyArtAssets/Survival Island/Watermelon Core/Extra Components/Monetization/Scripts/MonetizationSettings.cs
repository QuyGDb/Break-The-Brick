using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Monetization Settings", menuName = "Core/Monetization Settings")]
    public class MonetizationSettings : ScriptableObject
    {
        [SerializeField] IAPSettings iapSettings;
        public IAPSettings IAPSettings => iapSettings;

        [SerializeField] AdsSettings adsSettings;
        public AdsSettings AdsSettings => adsSettings;

        [SerializeField] bool isModuleActive = true;
        public bool IsModuleActive => isModuleActive;

        [SerializeField] bool verboseLogging = false;
        public bool VerboseLogging => verboseLogging;

        [SerializeField] bool debugMode = false;
        public bool DebugMode => debugMode;
    }
}
