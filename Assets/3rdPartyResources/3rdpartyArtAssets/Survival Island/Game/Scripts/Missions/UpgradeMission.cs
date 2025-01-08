using UnityEngine;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public sealed class UpgradeMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Task;

        [BoxGroup("Upgrade Mission Special", "Upgrade Mission Special")]
        public UpgradeType upgradeType;
        public UpgradeType UpgradeType => upgradeType;

        [BoxGroup("Upgrade Mission Special")]
        [SerializeField, ShowIf("IsUpgradeTypeGlobal")] GlobalUpgradeType globalUpgradeType;
        public GlobalUpgradeType GlobalUpgradeType => globalUpgradeType;

        [BoxGroup("Upgrade Mission Special")]
        [SerializeField, ShowIf("IsUpgradeTypeLocal")] AbstractLocalUpgrade localUpgrade;
        public AbstractLocalUpgrade LocalUpgrade => localUpgrade;

        [BoxGroup("Upgrade Mission Special")]
        [SerializeField] int upgradeLevel;

        [Space]
        [BoxGroup("Upgrade Mission Special")]
        [SerializeField] bool highlightInTheList;
        public bool HighlightInTheList => highlightInTheList;

        [BoxGroup("Upgrade Mission Special")]
        [SerializeField] bool highlightTheUIButton;
        public bool HighlightTheUIButton => highlightTheUIButton;

        private IUpgrade upgrade;

        private Save save;

        public override void Initialise()
        {
            base.Initialise();

            save = SaveController.GetSaveObject<Save>(GetSaveString());
            save.LinkMission(this);

            // Load mission stage
            missionStage = save.MissionStage;

            // Get upgrade
            if (upgradeType == UpgradeType.Global)
                upgrade = GlobalUpgradesController.GetUpgrade<AbstactGlobalUpgrade>(GlobalUpgradeType);
            else
                upgrade = localUpgrade;
        }

        public override void Activate()
        {
            base.Activate();

            isDirty = true;

            if (upgrade.UpgradeLevel >= upgradeLevel)
            {
                FinishMission();
            }
            else
            {
                StartMission();
            }

            if (upgradeType == UpgradeType.Global)
                GlobalUpgradesEventsHandler.OnUpgraded += OnGlobalUpgradeMade;

            if (upgradeType == UpgradeType.Local)
                localUpgrade.OnUpgraded += OnLocalUpgradeMade;

            if (highlightTheUIButton)
            {
                UIGame gameUI = UIController.GetPage<UIGame>();
                gameUI.HighlightUpgradesButton();
            }

            if (highlightInTheList)
            {
                upgrade.IsHighlighted = true;
            }
        }

        private void OnGlobalUpgradeMade(GlobalUpgradeType type, AbstactGlobalUpgrade upgrade)
        {
            if (GlobalUpgradeType == type)
            {
                if (upgrade.UpgradeLevel >= upgradeLevel)
                {
                    FinishMission();
                }
            }
        }

        private void OnLocalUpgradeMade()
        {
            if (localUpgrade.UpgradeLevel >= upgradeLevel)
            {
                FinishMission();
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (upgradeType == UpgradeType.Global)
                GlobalUpgradesEventsHandler.OnUpgraded -= OnGlobalUpgradeMade;

            if (upgradeType == UpgradeType.Local)
                localUpgrade.OnUpgraded -= OnLocalUpgradeMade;

            if (highlightTheUIButton)
            {
                UIGame gameUI = UIController.GetPage<UIGame>();
                gameUI.StopUpgradesButtonHighlight();
            }

            upgrade.IsHighlighted = false;
        }

        public override string GetFormattedProgress()
        {
            return "";
        }

        public override float GetProgress()
        {
            return upgrade.UpgradeLevel >= upgradeLevel ? 1.0f : 0.0f;
        }

        public override Vector3 GetDefaultPreviewPosition()
        {
            if (upgradeType == UpgradeType.Local)
            {
                return localUpgrade.transform.position;
            }
            else
            {
                return base.GetDefaultPreviewPosition();
            }
        }

        #region Development

        public bool ShowAutoAdjustButton()
        {
            return upgradeType == UpgradeType.Local && ShowCustomPointerFieldEditor();
        }

        [Button("Auto Adjust Pointer", "ShowAutoAdjustButton", ButtonVisibility.ShowIf)]
        public void AutoAdjustPointer()
        {
            if (CustomPointerLocation != null && upgradeType == UpgradeType.Local)
            {
                CustomPointerLocation.position = GetDefaultPreviewPosition();
                RuntimeEditorUtils.SetDirty(CustomPointerLocation);
            }
        }

        public bool IsUpgradeTypeGlobal()
        {
            return upgradeType == UpgradeType.Global;
        }

        public bool IsUpgradeTypeLocal()
        {
            return upgradeType == UpgradeType.Local;
        }

        #endregion


        [System.Serializable]
        public class Save : MissionSave
        {

        }
    }
}