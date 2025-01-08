using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class BuildingUpgradeContainer
    {
        [SerializeField] BuildingUpgradeType upgradeType;
        public BuildingUpgradeType UpgradeType => upgradeType;

        [SerializeField] AbstractLocalUpgrade upgrade;
        public AbstractLocalUpgrade Upgrade => upgrade;

        public BuildingUpgradeContainer(BuildingUpgradeType type, AbstractLocalUpgrade upgrade)
        {
            upgradeType = type;
            this.upgrade = upgrade;
        }
    }
}
