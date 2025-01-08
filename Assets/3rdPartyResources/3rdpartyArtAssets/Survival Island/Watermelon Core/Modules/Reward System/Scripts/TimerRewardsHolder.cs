using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.IAPStore
{
    public sealed class TimerRewardsHolder : RewardsHolder
    {
        private const string DEFAULT_BUTTON_TEXT = "Get!";

        [Group("Settings"), UniqueID]
        [SerializeField] string rewardID;

        [Group("Settings"), Space]
        [SerializeField] Button button;

        [Group("Settings")]
        [SerializeField] TMP_Text timerText;
        [Group("Settings")]
        [SerializeField] int timerDurationInMinutes;

        private SimpleLongSave save;
        private DateTime timerStartTime;

        private StringBuilder sb;

        private void Awake()
        {
            InitialiseComponents();

            save = SaveController.GetSaveObject<SimpleLongSave>($"TimerProduct_{rewardID}");

            // Check if rewards needs to be disabled
            for (int i = 0; i < rewards.Length; i++)
            {
                if (rewards[i].CheckDisableState())
                {
                    // Disable holder game object
                    gameObject.SetActive(false);

                    return;
                }
            }

            sb = new StringBuilder();

            button.onClick.AddListener(OnButtonClicked);
        }

        private string FormatTimer(TimeSpan timeSpan)
        {
            sb.Clear();

            if(timeSpan.Hours > 0)
            {
                sb.Append(timeSpan.Hours);
                sb.Append(':');
            }

            sb.Append(timeSpan.Minutes);
            sb.Append(':');

            sb.Append(timeSpan.Seconds);

            return sb.ToString();
        }

        private void Update()
        {
            TimeSpan timer = DateTime.Now - timerStartTime;
            TimeSpan duration = TimeSpan.FromMinutes(timerDurationInMinutes);
            if (timer > duration)
            {
                button.enabled = true;

                timerText.text = DEFAULT_BUTTON_TEXT;
            }
            else
            {
                button.enabled = false;

                timerText.text = FormatTimer(duration - timer);

                float prefferedWidth = timerText.preferredWidth;
                if (prefferedWidth < 270) prefferedWidth = 270;

                timerText.rectTransform.sizeDelta = timerText.rectTransform.sizeDelta.SetX(prefferedWidth + 5);
                button.image.rectTransform.sizeDelta = button.image.rectTransform.sizeDelta.SetX(prefferedWidth + 10);
            }
        }

        private void OnButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            save.Value = DateTime.Now.ToBinary();
            timerStartTime = DateTime.Now;

            ApplyRewards();

            SaveController.MarkAsSaveIsRequired();
        }
    }
}