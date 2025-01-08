using UnityEngine;
using Watermelon.GlobalUpgrades;
using System;
using UnityEditor;
using System.Globalization;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Movement Speed Upgrade", menuName = "Content/Upgrades/Movement Speed Upgrade")]
    public class MovementSpeedUpgrade : GlobalUpgrade<MovementSpeedUpgrade.MovementSpeedStage>
    {
        public override void Initialise()
        {

        }

        public override string GetUpgradeDescription(int stageId)
        {
            try
            {
                var prevValue = GetStage(stageId).PlayerMovementSpeed;
                var value = GetStage(stageId + 1).PlayerMovementSpeed;

                return string.Format(DescriptionFormat, prevValue, value);
            }
            catch
            {
                return "";
            }
        }

        [Serializable]
        public class MovementSpeedStage : GlobalUpgradeStage
        {
            [SerializeField] float playerMovementSpeed;
            public float PlayerMovementSpeed => playerMovementSpeed;

            [SerializeField] float playerAcceleration;
            public float PlayerAcceleration => playerAcceleration;
        }
    }
}