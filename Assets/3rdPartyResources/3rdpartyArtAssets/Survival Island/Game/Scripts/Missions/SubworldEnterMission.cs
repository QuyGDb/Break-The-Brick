using UnityEngine;

namespace Watermelon
{
    public sealed class SubworldEnterMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Task;

        [BoxGroup("Subworld Enter Mission Special", "Subworld Enter Mission Special")]
        [SerializeField] SubworldEntrance subworldEntranceRef;

        private Save save;

        public override void Initialise()
        {
            base.Initialise();

            save = SaveController.GetSaveObject<Save>(GetSaveString());
            save.LinkMission(this);

            // Load mission stage
            missionStage = save.MissionStage;
        }

        public override void Activate()
        {
            base.Activate();

            StartMission();

            subworldEntrance.SubworldEntered += OnSubworldEnterTriggered;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            subworldEntrance.SubworldEntered -= OnSubworldEnterTriggered;
        }

        private void OnSubworldEnterTriggered()
        {
            isDirty = true;
            FinishMission();
            MissionsController.CompleteMission();
        }

        public override float GetProgress()
        {
            switch (missionStage)
            {
                case Stage.Uninitialised:
                    return 0.0f;
                case Stage.Active:
                    return 0.0f;
                case Stage.Finished:
                    return 1.0f;
                case Stage.Collected:
                    return 1.0f;
            }

            return 0.0f;
        }

        public override Vector3 GetDefaultPreviewPosition()
        {
            return subworldEntrance.transform.position;
        }

        #region Development

        [Button("Auto Adjust Pointer", "ShowCustomPointerFieldEditor", ButtonVisibility.ShowIf)]
        public void AutoAdjustPointer()
        {
            if (CustomPointerLocation != null)
            {
                CustomPointerLocation.position = GetDefaultPreviewPosition();
                RuntimeEditorUtils.SetDirty(CustomPointerLocation);
            }
        }

        #endregion

        [System.Serializable]
        public class Save : MissionSave
        {
        }
    }
}