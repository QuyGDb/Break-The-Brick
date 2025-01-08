using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Watermelon
{
    public class Mission : MonoBehaviour
    {
        protected bool isFinished;
        public bool IsFinished => isFinished;

        [SerializeField, UniqueID, Order(-2)]
        protected string id;
        public string ID => id;

        [SerializeField, Order(-1)]
        protected string title;
        public string Title => title;

        [BoxGroup("Reward", "Reward")]
        [SerializeField] protected MissionRewardType rewardType;
        public MissionRewardType RewardType => rewardType;

        [BoxGroup("Reward")]
        [SerializeField, ShowIf("SelectedRewardTypeIsResources")] protected ResourceRewardData resourceReward;
        public ResourceRewardData ResourceReward => resourceReward;

        [BoxGroup("Reward")]
        [SerializeField, ShowIf("SelectedRewardTypeIsTools")] protected ToolRewardData toolsReward;
        public ToolRewardData ToolsReward => toolsReward;

        [BoxGroup("Reward")]
        [SerializeField, ShowIf("SelectedRewardTypeIsGeneric")] protected GenericRewardData genericReward;
        public GenericRewardData GenericReward => genericReward;

        [BoxGroup("Preview and Navigation", "Preview and Navigation")]
        [SerializeField] protected bool useCustomTargetPosition;
        [BoxGroup("Preview and Navigation"), InlineButton("Select", "SelectCustomPointerTransform")]
        [SerializeField, ShowIf("useCustomTargetPosition")] protected Transform customTargetPosition;
        protected Transform CustomPointerLocation => customTargetPosition;

        [Space(5)]
        [BoxGroup("Preview and Navigation")]
        [SerializeField] protected bool cameraPreviewAtStart;

        [Space(5)]
        [BoxGroup("Preview and Navigation")]
        [SerializeField] protected bool showPositionPointer;
        [BoxGroup("Preview and Navigation")]
        [SerializeField, ShowIf("showPositionPointer")] protected Vector3 pointerPositionOffset = new Vector3(0, 4, 0);

        [Space(5)]
        [BoxGroup("Preview and Navigation")]
        [SerializeField] protected bool showGuidingLine;
        [BoxGroup("Preview and Navigation")]
        [SerializeField, ShowIf("showGuidingLine")] protected Vector3 guidingLineTargetOffset = new Vector3(0, 0, 0);

        [Space(5)]
        [BoxGroup("Preview and Navigation")]
        [SerializeField] protected bool showGuidingArrow;
        [BoxGroup("Preview and Navigation")]
        [SerializeField, ShowIf("showGuidingArrow")] protected Vector3 guidingArrowTargetOffset = new Vector3(0, 0, 0);

        [BoxGroup("Subworld", "Subworld")]
        [SerializeField] protected bool isTargetInSubworld;
        [BoxGroup("Subworld")]
        [SerializeField, ShowIf("isTargetInSubworld")] protected SubworldEntrance subworldEntrance;
        [BoxGroup("Subworld")]
        [SerializeField, ShowIf("isTargetInSubworld")] protected SubworldBehavior subworldBehavior;

        [BoxFoldout("Callbacks", order: 10)]
        [SerializeField] UnityEvent onMissionStarted;
        [BoxFoldout("Callbacks", order: 10)]
        [SerializeField] UnityEvent onMissionCompleted;

        protected Stage missionStage;
        public Stage MissionStage => missionStage;

        protected bool isDirty;
        public bool IsDirty => isDirty;

        public virtual MissionUICase.Type MissionUIType => MissionUICase.Type.Collect;

        protected PositionPointerCase positionPointerCase;
        protected ArrowLinePointerCase guidingLineCase;
        protected ArrowPointerCase guidingArrowCase;

        public event OnStageChangedCallback OnStageChanged;

        public void ApplyReward(float multiplier = 1.0f)
        {
            if (rewardType == MissionRewardType.Resources)
            {
                CurrenciesController.Add(resourceReward.CurrencyType, Mathf.RoundToInt(resourceReward.Amount * multiplier));
            }
            else if (rewardType == MissionRewardType.Tool)
            {
                UnlockableToolsController.UnlockTool(toolsReward.InteractionToUnlock);
            }

            SetStage(Stage.Collected);

            onMissionCompleted?.Invoke();

            AudioController.PlaySound(AudioController.AudioClips.reward, 0.7f);
        }

        public void FinishMission()
        {
            SetStage(Stage.Finished);

            Deactivate();

            MissionsController.MissionFinished();

            if (MissionsController.AutoCompleteMissions)
                MissionsController.AutoCompleteMission(5.0f);
        }

        public void StartMission()
        {
            SetStage(Stage.Active);

            if (cameraPreviewAtStart)
                DoCameraPreview();

            ActivatePreviews();

            WorldController.WorldBehavior.SubworldHandler.OnSubworldEnetered += OnSubworldEnteredOrLeft;
            WorldController.WorldBehavior.SubworldHandler.OnSubworldLeft += OnSubworldEnteredOrLeft;

            onMissionStarted?.Invoke();
        }

        private void ActivatePreviews()
        {
            if (MissionStage == Stage.Finished || MissionStage == Stage.Collected)
                return;

            if (showPositionPointer)
                ActivatePositionPointer();

            if (showGuidingLine)
                ActivateGuidingLine();

            if (showGuidingArrow)
                ActivateGuidingArrow();
        }


        private void OnSubworldEnteredOrLeft()
        {
            // we need to update preview arrows depening on where player is located
            DisablePreviews();

            ActivatePreviews();
        }

        private void DisablePreviews()
        {
            if (positionPointerCase != null)
            {
                positionPointerCase.Disable();
                positionPointerCase = null;
            }

            if (guidingLineCase != null)
            {
                guidingLineCase.Disable();
                guidingLineCase = null;
            }

            if (guidingArrowCase != null)
            {
                guidingArrowCase.Disable();
                guidingArrowCase = null;
            }
        }

        public void SetStage(Stage stage)
        {
            Stage previousStage = missionStage;

            missionStage = stage;

            StageChanged(stage);

            OnStageChanged?.Invoke(previousStage, stage);
        }

        protected virtual void StageChanged(Stage stage)
        {

        }

        protected virtual void ActivatePositionPointer()
        {
            if (positionPointerCase != null)
                return;

            positionPointerCase = NavigationHelper.CreatePositionPointer(GetPreviewPosition() + pointerPositionOffset);
            positionPointerCase.Show();
        }


        protected virtual void ActivateGuidingLine()
        {
            if (guidingLineCase != null)
                return;

            guidingLineCase = NavigationHelper.CreateGuidingLine(GetPreviewPosition() + guidingLineTargetOffset);
        }

        protected virtual void ActivateGuidingArrow()
        {
            if (guidingArrowCase != null)
                return;

            guidingArrowCase = NavigationHelper.CreateGuidingArrow(GetPreviewPosition() + guidingArrowTargetOffset);
        }

        public void ForceComplete()
        {
            if (missionStage == Stage.Active)
            {
                FinishMission();
            }
        }

        public void DoCameraPreview()
        {
            if (!PreviewCamera.IsActive)
            {
                PreviewCamera.Focus(GetPreviewPosition(), 1.5f);
            }
        }

        public virtual void Initialise()
        {
        }

        public virtual void Activate()
        {

        }

        public virtual void Deactivate()
        {
            WorldController.WorldBehavior.SubworldHandler.OnSubworldEnetered -= OnSubworldEnteredOrLeft;
            WorldController.WorldBehavior.SubworldHandler.OnSubworldLeft -= OnSubworldEnteredOrLeft;

            DisablePreviews();
        }

        public virtual float GetProgress()
        {
            return 0;
        }

        public virtual string GetFormattedProgress()
        {
            return string.Empty;
        }

        public Vector3 GetPreviewPosition()
        {
            SubworldBehavior activeSubworld = WorldController.WorldBehavior.SubworldHandler.ActiveSubworld;

            // target is in the subworld
            if (isTargetInSubworld)
            {
                // if player on the main world
                if (activeSubworld == null)
                {
                    // show the entrance
                    return subworldEntrance.transform.position;
                }
                // if player on another subworld
                else if (activeSubworld != subworldBehavior)
                {
                    // show the exit
                    return activeSubworld.Exits[0].transform.position;
                }
            }
            // if target is in the main world
            else
            {
                // but player is in the subworld
                if (activeSubworld != null)
                {
                    // show the exit
                    return activeSubworld.Exits[0].transform.position;
                }
            }

            if (useCustomTargetPosition && customTargetPosition != null)
            {
                return customTargetPosition.position;
            }
            else
            {
                return GetDefaultPreviewPosition();
            }
        }

        public virtual Vector3 GetDefaultPreviewPosition()
        {
            return Vector3.zero;
        }

        public void OnUIUpdated()
        {
            isDirty = false;
        }

        public string GetSaveString()
        {
            return string.Format("world{0} mission{1}{2}", WorldController.CurrentWorld.ID, GetType(), id);
        }

        public enum Stage
        {
            Uninitialised = 0,
            Active = 1,
            Finished = 2,
            Collected = 3
        }

        public delegate void OnStageChangedCallback(Stage previousStage, Stage currentStage);

        protected void SelectCustomPointerTransform()
        {
#if UNITY_EDITOR
            if (customTargetPosition != null)
                Selection.activeObject = customTargetPosition;
#endif
        }

        #region Editor

        protected virtual bool ShowCustomPointerFieldEditor()
        {
            return showPositionPointer && useCustomTargetPosition;
        }

        protected bool SelectedRewardTypeIsResources()
        {
            return rewardType == MissionRewardType.Resources;
        }

        protected bool SelectedRewardTypeIsTools()
        {
            return rewardType == MissionRewardType.Tool;
        }

        protected bool SelectedRewardTypeIsGeneric()
        {
            return rewardType == MissionRewardType.Generic;
        }

        #endregion

        [System.Serializable]
        public class ResourceRewardData
        {
            [SerializeField] CurrencyType currencyType;
            public CurrencyType CurrencyType => currencyType;

            [SerializeField] int amount;
            public int Amount => amount;
        }

        [System.Serializable]
        public class ToolRewardData
        {
            [SerializeField] InteractionAnimationType interactionToUnlock;
            public InteractionAnimationType InteractionToUnlock => interactionToUnlock;

            [SerializeField] UIMissionRewardPopUp.RewardInfo uiRewardInfo;
            public UIMissionRewardPopUp.RewardInfo RewardInfo => uiRewardInfo;
        }

        [System.Serializable]
        public class GenericRewardData
        {
            [SerializeField] UIMissionRewardPopUp.RewardInfo uiRewardInfo;
            public UIMissionRewardPopUp.RewardInfo RewardInfo => uiRewardInfo;
        }
    }
}