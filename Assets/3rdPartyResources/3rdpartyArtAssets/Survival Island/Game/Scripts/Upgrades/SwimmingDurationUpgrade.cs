using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Swimming Duration Upgrade", menuName = "Content/Upgrades/Swimming Duration Upgrade")]
    public class SwimmingDurationUpgrade : GlobalUpgrade<SwimmingDurationUpgrade.DurationStage>
    {
        public override void Initialise()
        {

        }

        public override string GetUpgradeDescription(int stageId)
        {
            try
            {
                var prevValue = GetStage(stageId).DurationinSeconds;
                var value = GetStage(stageId + 1).DurationinSeconds;

                return string.Format(DescriptionFormat, prevValue, value);
            }
            catch
            {
                return "";
            }
        }

        [System.Serializable]
        public class DurationStage : GlobalUpgradeStage
        {
            [SerializeField] float durationInSeconds;
            public float DurationinSeconds => durationInSeconds;
        }
    }
}
