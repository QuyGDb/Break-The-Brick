using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public abstract class UpgradesTrigger : MonoBehaviour
    {
        protected List<IUpgrade> upgrades;

        // if there where registered upgrades - then it's trigger for local upgrades
        // otherwise - open global upgrades page
        protected bool IsLocalTrigger => !upgrades.IsNullOrEmpty();

        protected virtual void Awake()
        {
            upgrades = new List<IUpgrade>();
        }

        public virtual void RegisterUpgrade(IUpgrade upgrade)
        {
            upgrades.Add(upgrade);
        }

        public virtual void RegisterUpgrades(List<IUpgrade> newUpgrade)
        {
            upgrades.AddRange(newUpgrade);
        }

        public abstract void ShowUpgradesPanel();
    }
}