using UnityEngine;

namespace Watermelon.GlobalUpgrades
{
    public abstract class GlobalUpgrade<T> : AbstactGlobalUpgrade where T : GlobalUpgradeStage
    {
        [SerializeField]
        protected T[] upgrades;
        public override GlobalUpgradeStage[] Upgrades => upgrades;

        public T GetCurrentStage()
        {
            if (upgrades.IsInRange(UpgradeIndex))
                return upgrades[UpgradeIndex];

            UpgradeIndex = upgrades.Length - 1;
            Debug.Log("[Perks]: Perk level is out of range!");

            return upgrades[UpgradeIndex];
        }

        public T GetNextStage()
        {
            if (upgrades.IsInRange(UpgradeIndex + 1))
                return upgrades[UpgradeIndex + 1];

            return null;
        }

        [Button("Upgrade")]
        public override void UpgradeStage()
        {
            if (upgrades.IsInRange(UpgradeIndex + 1))
            {
                UpgradeIndex += 1;

                InvokeOnUpgraded();
            }
        }

        public T GetStage(int i)
        {
            if (upgrades.IsInRange(i))
                return upgrades[i];
            return null;
        }

        public void TestUpgrade()
        {
            UpgradeStage();
        }
    }
}