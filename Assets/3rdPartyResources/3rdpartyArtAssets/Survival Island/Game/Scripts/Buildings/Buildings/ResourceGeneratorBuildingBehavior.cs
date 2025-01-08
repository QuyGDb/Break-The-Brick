using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ResourceGeneratorBuildingBehavior : BuildingBehavior
    {
        [SerializeField] CurrencyType resourceType;
        public CurrencyType ResourceType => resourceType;

        [Space]
        [SerializeField] int storageCapacity = 20;
        [SerializeField] float generationDuration = 5;

        [Space]
        [SerializeField] SimpleResourceStorageBehavior storage;

        public event SimpleCallback OnAnimationStarted;
        public event SimpleCallback OnAnimationStoped;

        public bool IsAnimationRunning { get; private set; } = false;

        private bool isActive = false;
        private float lastTimeResourceGenerated;

        protected SimpleIntUpgrade StorageCapacityUpgrade { get; private set; }

        protected override void RegisterUpgrades()
        {
            for (int i = 0; i < buildingUpgrades.Count; i++)
            {
                var upgrade = buildingUpgrades[i];

                if (upgrade.UpgradeType == BuildingUpgradeType.StorageCapacity)
                {
                    StorageCapacityUpgrade = (SimpleIntUpgrade)upgrade.Upgrade;

                    string upgradeSaveName = $"{ID}_{BuildingUpgradeType.StorageCapacity}";
                    StorageCapacityUpgrade.Init(upgradeSaveName);

                    storageCapacity = StorageCapacityUpgrade.CurrentStage.Value;

                    StorageCapacityUpgrade.OnUpgraded += OnCapacityUpgraded;
                }
            }
        }

        protected override void Init()
        {
            base.Init();

            var storageSaveName = $"{ID}_OutStorage";
            storage.Init(storageSaveName, new List<CurrencyType>() { resourceType }, storageCapacity);
        }

        private void Update()
        {
            if (!isActive)
                return;

            if (!IsAnimationRunning && !storage.IsFull())
            {
                IsAnimationRunning = true;
                OnAnimationStarted?.Invoke();
            }
            else if (IsAnimationRunning && storage.IsFull())
            {
                IsAnimationRunning = false;
                OnAnimationStoped?.Invoke();
            }

            if (!storage.IsFull() && Time.time - lastTimeResourceGenerated > generationDuration)
            {
                storage.AddResources(resourceType);

                lastTimeResourceGenerated = Time.time;
            }
        }

        private void OnCapacityUpgraded()
        {
            storageCapacity = StorageCapacityUpgrade.CurrentStage.Value;

            storage.SetCapacity(storageCapacity);
        }

        public override void SpanwNotUnlocked()
        {
            base.SpanwNotUnlocked();

            isActive = false;
        }

        public override void FullyUnlock()
        {
            base.FullyUnlock();

            isActive = true;
        }

        public override void SpawnUnlocked()
        {
            base.SpawnUnlocked();

            isActive = true;
        }

        public override void OnWorldUnloaded()
        {
            base.OnWorldUnloaded();

            isActive = false;
        }
    }
}