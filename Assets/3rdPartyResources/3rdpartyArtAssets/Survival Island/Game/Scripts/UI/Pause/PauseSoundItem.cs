using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class PauseSoundItem : PauseItem
    {
        [SerializeField] Image imageRef;

        [Space]
        [SerializeField] Sprite activeSprite;
        [SerializeField] Sprite disableSprite;

        private bool isActive = true;

        private void Start()
        {
            isActive = AudioController.GetVolume(AudioType.Sound) != 0;

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

                AudioController.SetVolume(AudioType.Sound, 1f);
                AudioController.SetVolume(AudioType.Music, 1f);
            }
            else
            {
                imageRef.sprite = disableSprite;

                AudioController.SetVolume(AudioType.Sound, 0f);
                AudioController.SetVolume(AudioType.Music, 0f);
            }

            // Play button sound
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }
    }
}
