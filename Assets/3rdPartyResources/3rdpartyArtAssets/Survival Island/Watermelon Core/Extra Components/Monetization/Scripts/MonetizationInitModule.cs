using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Monetization")]
    public class MonetizationInitModule : InitModule
    {
        public override string ModuleName => "Monetization"; 

        [SerializeField] MonetizationSettings settings;

        public override void CreateComponent(GameObject holderObject)
        {
            Monetization.IsActive = settings.IsModuleActive;
            Monetization.VerboseLogging = settings.VerboseLogging;
            Monetization.DebugMode = settings.DebugMode;

            AdsManager.Initialise(settings);
            IAPManager.Initialise(settings);
        }
    }
}