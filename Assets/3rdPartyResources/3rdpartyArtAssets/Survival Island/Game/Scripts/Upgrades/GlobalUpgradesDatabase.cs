using UnityEngine;

namespace Watermelon.GlobalUpgrades
{
    [CreateAssetMenu(fileName = "Upgrades Database", menuName = "Content/Upgrades/Upgrades Database")]
    public class GlobalUpgradesDatabase : ScriptableObject
    {
        [SerializeField] AbstactGlobalUpgrade[] upgrades;
        public AbstactGlobalUpgrade[] Upgrades => upgrades;
    }
}