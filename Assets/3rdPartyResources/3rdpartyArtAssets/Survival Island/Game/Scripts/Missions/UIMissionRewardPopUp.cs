using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIMissionRewardPopUp : MonoBehaviour
    {
        [SerializeField] UIFadeAnimation fadeAnimation;
        [SerializeField] UIScaleAnimation panelBackScaleAnimation;

        [Space]
        [SerializeField] TMP_Text headerText;
        [SerializeField] Image previewIcon;
        [SerializeField] TMP_Text mainText;
        [SerializeField] TMP_Text buttonText;

        [Space]
        [SerializeField] Button fadeButton;
        [SerializeField] Button continueButton;

        private void Awake()
        {
            fadeButton.onClick.AddListener(ClosePanelButton);
            continueButton.onClick.AddListener(ClosePanelButton);
        }

        public void OnMissionFinished(Mission mission)
        {
            if (mission.RewardType == MissionRewardType.Tool)
            {
                Show(mission.ToolsReward.RewardInfo);
            }
            else if (mission.RewardType == MissionRewardType.Generic)
            {
                Show(mission.GenericReward.RewardInfo);
            }
        }

        public void Show(RewardInfo data)
        {
            gameObject.SetActive(true);

            fadeAnimation.Show();
            panelBackScaleAnimation.Show();

            headerText.text = data.HeaderText;
            previewIcon.sprite = data.PreviewIcon;
            mainText.text = data.MainText;
            buttonText.text = data.ButtonText;

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Popup);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            fadeAnimation.Hide();
            panelBackScaleAnimation.Hide(onCompleted: () =>
            {
                gameObject.SetActive(false);
            });

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
        }

        private void ClosePanelButton()
        {
            MissionsController.CompleteMission();
        }

        [System.Serializable]
        public class RewardInfo
        {
            [SerializeField] string headerText = "YOU UNLOCKED";
            public string HeaderText => headerText;

            [SerializeField] Sprite previewIcon;
            public Sprite PreviewIcon => previewIcon;

            [SerializeField] string mainText;
            public string MainText => mainText;

            [SerializeField] string buttonText = "AWESOME";
            public string ButtonText => buttonText;
        }
    }
}