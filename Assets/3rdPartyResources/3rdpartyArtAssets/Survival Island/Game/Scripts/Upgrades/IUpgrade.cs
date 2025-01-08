using UnityEngine;

namespace Watermelon.GlobalUpgrades
{
    public interface IUpgrade
    {
        UpgradeType Type { get; }

        string Title { get; }
        Sprite Icon { get; }

        int UpgradeIndex { get; }

        bool IsMaxedOut { get; }
        Resource GetUpgradeCost(int stageId);
        string GetUpgradeDescription(int stageId);
        void UpgradeStage();

        bool IsHighlighted { get; set; }
        public int UpgradeLevel { get; }
    }
}