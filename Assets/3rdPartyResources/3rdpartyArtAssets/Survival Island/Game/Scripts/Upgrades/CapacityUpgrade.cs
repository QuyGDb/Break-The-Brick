using UnityEngine;
using Watermelon.GlobalUpgrades;
using System;
using UnityEditor;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Capacity Upgrade", menuName = "Content/Upgrades/Capacity Upgrade")]
    public class CapacityUpgrade : GlobalUpgrade<CapacityUpgrade.CapacityUpgradeStage>
    {
        public override void Initialise()
        {

        }

        public override string GetUpgradeDescription(int stageId)
        {
            try
            {
                var prevValue = GetStage(stageId).Capacity;
                var value = GetStage(stageId + 1).Capacity;

                return string.Format(DescriptionFormat, prevValue, value);
            }
            catch
            {
                return "";
            }
        }

        [System.Serializable]
        public class CapacityUpgradeStage : GlobalUpgradeStage
        {
            [SerializeField] int capacity;
            public int Capacity => capacity;
        }
    }
}