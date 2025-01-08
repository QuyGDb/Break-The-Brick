using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    using GlobalUpgrades;

    public class GlobalUpgradesController : MonoBehaviour
    {
        private const string SAVE_IDENTIFIER = "upgrades:{0}";

        [SerializeField] GlobalUpgradesDatabase upgradesDatabase;

        private static List<AbstactGlobalUpgrade> activeUpgrades;
        public static List<AbstactGlobalUpgrade> ActiveUpgrades => activeUpgrades;

        private static Dictionary<GlobalUpgradeType, AbstactGlobalUpgrade> activeUpgradesLink;

        private static List<IUpgrade> globalSimpleUpgrades = new List<IUpgrade>();

        private static UIUpgrades uiUpgrades;

        public void Initialise()
        {
            activeUpgrades = new List<AbstactGlobalUpgrade>(upgradesDatabase.Upgrades);
            
            activeUpgradesLink = new Dictionary<GlobalUpgradeType, AbstactGlobalUpgrade>();
            for (int i = 0; i < activeUpgrades.Count; i++)
            {
                var upgrade = activeUpgrades[i];

                var hash = string.Format(SAVE_IDENTIFIER, upgrade.GlobalUpgradeType.ToString()).GetHashCode();

                UpgradeSavableObject save = SaveController.GetSaveObject<UpgradeSavableObject>(hash); ;

                upgrade.SetSave(save);

                if (!activeUpgradesLink.ContainsKey(upgrade.GlobalUpgradeType))
                {
                    upgrade.Initialise();

                    activeUpgradesLink.Add(upgrade.GlobalUpgradeType, activeUpgrades[i]);
                }
            }

            uiUpgrades = UIController.GetPage<UIUpgrades>();
        }

        [System.Obsolete]
        public static AbstactGlobalUpgrade GetUpgradeByType(GlobalUpgradeType perkType)
        {
            if (activeUpgradesLink.ContainsKey(perkType))
                return activeUpgradesLink[perkType];

            Debug.LogError($"[Perks]: Upgrade with type {perkType} isn't registered!");

            return null;
        }

        public static T GetUpgrade<T>(GlobalUpgradeType type) where T : AbstactGlobalUpgrade
        {
            if (activeUpgradesLink.ContainsKey(type))
                return activeUpgradesLink[type] as T;

            Debug.LogError($"[Perks]: Upgrade with type {type} isn't registered!");

            return null;
        }

        public static void RegisterSimpleUpgrade(IUpgrade upgrade)
        {
            globalSimpleUpgrades.Add(upgrade);
        }

        public static void OpenMainUpgradesPage()
        {
            uiUpgrades.ResetUpgrades();
            uiUpgrades.RegisterUpgrades(ActiveUpgrades.ConvertAll(upgrade => (IUpgrade)upgrade));
            uiUpgrades.RegisterUpgrades(globalSimpleUpgrades);

            UIController.ShowPage<UIUpgrades>();
        }

        public static void OpenUpgradesPage(List<IUpgrade> upgradesToOpen)
        {
            uiUpgrades.ResetUpgrades();
            uiUpgrades.RegisterUpgrades(upgradesToOpen);

            UIController.ShowPage<UIUpgrades>();
        }
    }
}