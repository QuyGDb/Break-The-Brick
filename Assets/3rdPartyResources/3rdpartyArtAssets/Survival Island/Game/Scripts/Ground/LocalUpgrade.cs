using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class LocalUpgrade<T> : AbstractLocalUpgrade
    {
        [SerializeField] List<SimpleUpgradeStage<T>> stages;

        public override int UpgradesCount => stages.Count;

        public SimpleUpgradeStage<T> CurrentStage => stages[UpgradeIndex];
        public SimpleUpgradeStage<T> NextStage => stages.Count > UpgradeIndex + 1 ? stages[UpgradeIndex + 1] : null;

        public SimpleUpgradeStage<T> GetStage(int index) => stages[index];

        public override Resource GetUpgradeCost(int level)
        {
            if (level >= stages.Count)
                return new Resource(CurrencyType.Coins, 0);

            var upgradeStage = stages[level];

            return upgradeStage.Cost;
        }

        public override string GetUpgradeDescription(int stageId)
        {
            try
            {
                var prevValue = GetStage(stageId).Value;
                var value = GetStage(stageId + 1).Value;

                return string.Format(DescriptionFormat, prevValue, value);
            }
            catch
            {
                return "";
            }
        }

        [Button("Upgrade")]
        public override void UpgradeStage()
        {
            if (!IsMaxedOut)
            {
                UpgradeIndex += 1;
            }
        }

        [System.Serializable]
        public class SimpleUpgradeStage<K> where K : T
        {
            [SerializeField] Resource cost;
            public Resource Cost => cost;

            [SerializeField] K value;
            public K Value => value;
        }
    }
}