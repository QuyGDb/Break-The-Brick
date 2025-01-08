using UnityEngine;

namespace Watermelon
{
    public class NoAdsReward : Reward
    {
        [SerializeField] bool disableOfferIfNoAdsPurchased;

        private void OnEnable()
        {
            AdsManager.ForcedAdDisabled += OnForcedAdDisabled;    
        }

        private void OnDisable()
        {
            AdsManager.ForcedAdDisabled -= OnForcedAdDisabled;
        }

        public override void ApplyReward()
        {
            AdsManager.DisableForcedAd();
        }

        public override bool CheckDisableState()
        {
            if (disableOfferIfNoAdsPurchased)
                return !AdsManager.IsForcedAdEnabled();

            return false;
        }

        private void OnForcedAdDisabled()
        {
            if (disableOfferIfNoAdsPurchased)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
