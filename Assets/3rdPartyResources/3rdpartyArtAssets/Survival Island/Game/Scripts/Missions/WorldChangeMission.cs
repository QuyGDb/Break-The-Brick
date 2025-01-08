using UnityEngine;

namespace Watermelon
{
    public sealed class WorldChangeMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Task;

        [BoxGroup("World Change Mission Special", "World Change Mission Special")]
        [SerializeField] WorldChangeZoneBehavior worldChangeZone;

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

            worldChangeZone.OnWorldChangeZoneEntered += OnWorldChangeTriggered;
        }

        public override void Deactivate()
        {
            base.Deactivate();

            worldChangeZone.OnWorldChangeZoneEntered -= OnWorldChangeTriggered;
        }

        private void OnWorldChangeTriggered()
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
            return worldChangeZone.transform.position;
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