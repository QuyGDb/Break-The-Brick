using UnityEngine;

namespace Watermelon
{
    public sealed class HelperMission : Mission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Task;

        [BoxGroup("Helper Mission Special", "Helper Mission Special")]
        [SerializeField] HelperBehavior helperBehavior;
        public HelperBehavior HelperBehavior => helperBehavior;

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

            isDirty = true;

            if (!helperBehavior.IsOpened)
            {
                helperBehavior.HelperUnlocked += OnUnlocked;

                StartMission();
            }
            else
            {
                FinishMission();
            }
        }

        private void OnUnlocked()
        {
            FinishMission();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            helperBehavior.HelperUnlocked -= OnUnlocked;
        }

        public override string GetFormattedProgress()
        {
            return "";
        }

        public override float GetProgress()
        {
            return !helperBehavior.IsOpened ? 0.0f : 1.0f;
        }

        public override Vector3 GetDefaultPreviewPosition()
        {
            return helperBehavior.transform.position;
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