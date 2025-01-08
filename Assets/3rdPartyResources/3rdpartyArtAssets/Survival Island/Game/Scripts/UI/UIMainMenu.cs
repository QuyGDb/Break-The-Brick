using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIMainMenu : UIPage
    {
        [SerializeField] RectTransform safeAreaRectTransform;

        [Space]
        [SerializeField] Button playButton;
        [SerializeField] Button quitButton;

        public override void Initialise()
        {
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            playButton.onClick.AddListener(OnPlayButtonClicked);

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Destroy(quitButton.gameObject);
#else 
            quitButton.onClick.AddListener(OnQuitButtonClicked);
#endif
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        #endregion

        #region Buttons

        public void OnPlayButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            Overlay.Show(0.3f, () =>
            {
                UIController.HidePage<UIMainMenu>();

                GameController.LoadCurrentWorld();

                Overlay.Hide(0.3f, null);
            });
        }

        public void OnQuitButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        [Button]
        public void Test()
        {
            // Show fullscreen black overlay
            Overlay.Show(0.3f, () =>
            {
                // Save the current state of the game
                SaveController.Save(true);

                // Unload the current world and all the dependencies
                GameController.UnloadWorld(() =>
                {
                    UIController.HidePage<UIGame>();

                    UIController.ShowPage<UIMainMenu>();

                    // Disable fullscreen black overlay
                    Overlay.Hide(0.3f, null);
                });
            }, true);
        }

        #endregion
    }


}
