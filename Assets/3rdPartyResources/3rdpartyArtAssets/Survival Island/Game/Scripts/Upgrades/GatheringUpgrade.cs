using UnityEngine;
using Watermelon.GlobalUpgrades;
using System;
using UnityEditor;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Gathering Upgrade", menuName = "Content/Upgrades/Gathering Upgrade")]
    public class GatheringUpgrade : GlobalUpgrade<GatheringUpgrade.GatheringUpgradeStage>
    {
        public override void Initialise()
        {

        }

        public override string GetUpgradeDescription(int stageId)
        {
            try
            {
                var prevValue = GetStage(stageId).DamageMultiplier;
                var value = GetStage(stageId + 1).DamageMultiplier;

                return string.Format(DescriptionFormat, prevValue, value);
            }
            catch
            {
                return "";
            }
        }

        [System.Serializable]
        public class GatheringUpgradeStage : GlobalUpgradeStage
        {
            [SerializeField] float damageMultiplier = 1;
            public float DamageMultiplier => damageMultiplier;
        }
    }
}