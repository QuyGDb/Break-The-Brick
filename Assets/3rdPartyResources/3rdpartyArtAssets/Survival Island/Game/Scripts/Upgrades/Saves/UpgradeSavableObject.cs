using UnityEngine;

namespace Watermelon.GlobalUpgrades
{
    [System.Serializable]
    public class UpgradeSavableObject : ISaveObject
    {
        [SerializeField] int upgradeIndex;
        public int UpgradeLevel { get => upgradeIndex; set => upgradeIndex = value; }

        public UpgradeSavableObject(int upgradeLevel)
        {
            this.upgradeIndex = upgradeLevel;
        }

        public UpgradeSavableObject()
        {
            upgradeIndex = 0;
        }

        public virtual void Flush() { }
    }

}