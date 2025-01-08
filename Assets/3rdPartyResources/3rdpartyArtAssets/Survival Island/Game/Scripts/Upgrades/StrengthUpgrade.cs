using UnityEngine;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Strength Upgrade", menuName = "Content/Upgrades/Strength Upgrade")]
    public class StrengthUpgrade : GlobalUpgrade<StrengthUpgrade.StrengthStage>
    {
        public override void Initialise()
        {

        }

        public override string GetUpgradeDescription(int stageId)
        {
            try
            {
                var prevValue = GetStage(stageId).PlayerItemCarryingAmount;
                var value = GetStage(stageId + 1).PlayerItemCarryingAmount;

                return string.Format(DescriptionFormat, prevValue, value);
            }
            catch
            {
                return "";
            }
        }

        [System.Serializable]
        public class StrengthStage : GlobalUpgradeStage
        {
            [SerializeField] int playerAnimalCarryingAmount;
            public float PlayerAnimalCarryingAmount => playerAnimalCarryingAmount;

            [SerializeField] int playerItemCarryingAmount;
            public float PlayerItemCarryingAmount => playerItemCarryingAmount;
        }
    }
}