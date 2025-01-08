using System;
using UnityEngine;

namespace Watermelon.GlobalUpgrades
{
    public abstract class AbstactGlobalUpgrade : ScriptableObject, IUpgrade
    {
        [SerializeField]
        protected GlobalUpgradeType globalUpgradeType;
        public GlobalUpgradeType GlobalUpgradeType => globalUpgradeType;

        [SerializeField]
        protected string title;
        public string Title => title;

        [SerializeField]
        protected Sprite icon;
        public Sprite Icon => icon;

        public int UpgradeIndex { get => save.UpgradeLevel; set => save.UpgradeLevel = value; } // upgrade index - index of the upgrade in the upgrades list (starts from 0)
        public int UpgradeLevel => UpgradeIndex + 1; // upgrade level similar to upgrade index but starts from 1

        [NonSerialized]
        protected UpgradeSavableObject save;

        [SerializeField] string descriptionFormat;
        public string DescriptionFormat => descriptionFormat;

        public abstract GlobalUpgradeStage[] Upgrades { get; }
        public int UpgradesCount => Upgrades.Length;
        public bool IsMaxedOut => UpgradeIndex + 1 >= UpgradesCount;

        public GlobalUpgradeStage CurrentStage => Upgrades[UpgradeIndex];
        public GlobalUpgradeStage NextStage => Upgrades.Length > UpgradeIndex + 1 ? Upgrades[UpgradeIndex + 1] : null;

        public abstract void Initialise();
        public abstract void UpgradeStage();
        public abstract string GetUpgradeDescription(int stageId);

        public event SimpleCallback OnUpgraded;

        public UpgradeType Type => UpgradeType.Global;
        public bool IsHighlighted { get; set; }

        protected void InvokeOnUpgraded()
        {
            GlobalUpgradesEventsHandler.OnUpgraded(globalUpgradeType, this);
            OnUpgraded?.Invoke();
        }

        public Resource GetUpgradeCost(int level)
        {
            if (level >= Upgrades.Length)
                return new Resource(CurrencyType.Coins, 0);

            var upgradeStage = Upgrades[level];

            return new Resource(upgradeStage.CurrencyType, upgradeStage.Price);
        }

        public virtual void SetSave(UpgradeSavableObject save)
        {
            this.save = save;
        }

        public Sprite GetPreviewImage(int level)
        {
            if (level >= Upgrades.Length)
                return null;

            var upgradeStage = Upgrades[level];

            return upgradeStage.PreviewSprite;
        }

        [Button("Reset")]
        public virtual void ResetUpgrade()
        {
            UpgradeIndex = 0;

            InvokeOnUpgraded();
        }

        [Button("Max")]
        public void TestMaxUpgrade()
        {
            UpgradeIndex = UpgradesCount - 1;

            InvokeOnUpgraded();
        }
    }
}