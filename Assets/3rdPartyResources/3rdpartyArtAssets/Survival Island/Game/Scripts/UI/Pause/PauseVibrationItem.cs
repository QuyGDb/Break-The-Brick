using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class PauseVibrationItem : PauseItem
    {
        [SerializeField] Image imageRef;

        [Space]
        [SerializeField] Sprite activeSprite;
        [SerializeField] Sprite disableSprite;

        private bool isActive = true;

        private void Start()
        {
#if MODULE_HAPTIC
            isActive = Haptic.IsActive;
#else
            isActive = false;
#endif

            if (isActive)
                imageRef.sprite = activeSprite;
            else
                imageRef.sprite = disableSprite;
        }

        protected override void Click()
        {
            isActive = !isActive;

            if (isActive)
            {
                imageRef.sprite = activeSprite;

#if MODULE_HAPTIC
                Haptic.IsActive = true;
#endif
            }
            else
            {
                imageRef.sprite = disableSprite;

#if MODULE_HAPTIC
                Haptic.IsActive = false;
#endif
            }

            // Play button sound
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}
