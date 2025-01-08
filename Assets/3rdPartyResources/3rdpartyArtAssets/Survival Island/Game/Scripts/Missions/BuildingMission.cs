using UnityEngine;

namespace Watermelon
{
    public sealed class BuildingMission : Mission, IUnlockingMission
    {
        public override MissionUICase.Type MissionUIType => MissionUICase.Type.Build;

        [BoxGroup("Building Mission Special", "Building Mission Special")]
        [SerializeField] BuildingComplexBehavior buildingComplex;
        public BuildingComplexBehavior BuildingComplex => buildingComplex;

        public IUnlockableComplex LinkedBuilding => buildingComplex;
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

            if (buildingComplex.CanBePurchased || buildingComplex.CanBeConstructed)
            {
                StartMission();
            }
            else
            {
                FinishMission();
            }

            buildingComplex.SubscribeOnFullyUnlocked(OnUnlocked);
            buildingComplex.SubscribeOnResourcePlaced(OnResourcePlaced);
        }

        private void OnUnlocked()
        {
            if (!buildingComplex.CanBePurchased && !buildingComplex.CanBeConstructed)
            {
                FinishMission();
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            buildingComplex.UnsubscribeOnFullyUnlocked(OnUnlocked);
            buildingComplex.UnsubscribeOnResourcePlaced(OnResourcePlaced);
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
            return buildingComplex.CanBePurchased ? 0.0f : 1.0f;
        }

        public override Vector3 GetDefaultPreviewPosition()
        {
            return buildingComplex.transform.position;
        }

        [System.Serializable]
        public class Save : MissionSave
        {

        }
    }
}