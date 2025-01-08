namespace Watermelon
{
    public class PauseRestorePurchasesItem : PauseItem
    {
        protected override void Awake()
        {
            base.Awake();

#if !((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
            Destroy(this.gameObject);
#endif
        }

        protected override void Click()
        {
#if MODULE_MONETIZATION
            IAPManager.RestorePurchases();
#endif

            // Play button sound
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}
