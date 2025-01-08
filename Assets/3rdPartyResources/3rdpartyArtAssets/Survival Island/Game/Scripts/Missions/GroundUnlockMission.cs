using UnityEngine;

namespace Watermelon
{
    public sealed class GroundUnlockMission : Mission, IUnlockingMission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Build;

        [BoxGroup("Ground Unlock Mission Special", "Ground Unlock Mission Special")]
        [SerializeField] GroundTileComplexBehavior groundTile;
        public GroundTileComplexBehavior GroundTile => groundTile;

        public IUnlockableComplex LinkedBuilding => groundTile;

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

            if (groundTile.CanBePurchased || groundTile.CanBeConstructed)
            {
                StartMission();
            }
            else
            {
                FinishMission();
            }

            groundTile.SubscribeOnFullyUnlocked(OnUnlocked);
            groundTile.SubscribeOnResourcePlaced(OnResourcePlaced);
        }

        private void OnUnlocked()
        {
            if (!groundTile.CanBePurchased && !groundTile.CanBeConstructed)
            {
                FinishMission();
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            groundTile.UnsubscribeOnFullyUnlocked(OnUnlocked);
            groundTile.UnsubscribeOnResourcePlaced(OnResourcePlaced);
        }

        private void OnResourcePlaced()
        {
            isDirty = true;
        }

        public override string GetFormattedProgress()
        {
            return "";
        }

        public override float GetProgress()
        {
            return groundTile.CanBePurchased ? 0.0f : 1.0f;
        }

        public override Vector3 GetDefaultPreviewPosition()
        {
            return groundTile.Position;
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