using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public static class GlobalUpgradesEventsHandler
    {
        public static Events.OnUpgradedCallback OnUpgraded;

        public static class Events
        {
            public delegate void OnUpgradedCallback(GlobalUpgradeType upgradeType, AbstactGlobalUpgrade upgrade);

            public static void OnUpgraded(GlobalUpgradeType upgradeType, AbstactGlobalUpgrade upgrade)
            {
                GlobalUpgradesEventsHandler.OnUpgraded?.Invoke(upgradeType, upgrade);
            }
        }
    }
}