using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class ResourceStorageBuildingBehavior : BuildingBehavior, IGroundOpenable
    {
        [SerializeField] List<CurrencyType> storedResources;
        public List<CurrencyType> StoredResources => storedResources;

        [SerializeField, HideIf("EditorHaveCapacityUpgrade")] int capacity;

        [Space]
        [SerializeField] SimpleResourceStorageBehavior storage;
        public SimpleResourceStorageBehavior Storage => storage;

        [Space]
        [SerializeField] bool isHelperTaskActive = true;
        public bool IsHelperTaskActive => isHelperTaskActive;

        [Space]
        [SerializeField] GameObject emptyStorageIndicator;
        [SerializeField] TMP_Text emptyStorageIndicatorText;

        [Space]
        [SerializeField] bool fillUpOnGameStartap;
        [SerializeField] float minAwayDurationInMinutes = 30;

        private ResourcesSave save;

        public bool IsFull => storage.IsFull();

        private StoreResourcesTask storeResourcesTask;

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

                    capacity = StorageCapacityUpgrade.CurrentStage.Value;

                    StorageCapacityUpgrade.OnUpgraded += OnCapacityUpgraded;
                }
            }
        }

        protected override void Init()
        {
            base.Init();

            string storageSaveName = $"{ID}_Storage";
            storage.Init(storageSaveName, storedResources, capacity);

            storeResourcesTask = new StoreResourcesTask(this);
            storeResourcesTask.Activate();
            storeResourcesTask.Register(LinkedWorldBehavior.TaskHandler);

            storage.OnResourcesChanged += OnStorageResourcesChanged;
            OnStorageResourcesChanged();

            emptyStorageIndicatorText.text = "0/" + capacity;

            save = SaveController.GetSaveObject<ResourcesSave>(ID);
            save.Init();
            storage.OnResourceAdded += OnResourceStored;
        }

        private void OnResourceStored(CurrencyType resourceType)
        {
            if(!save.Resources.Contains(resourceType))
                save.Resources.Add(resourceType);
        }

        private void OnStorageResourcesChanged()
        {
            emptyStorageIndicator.SetActive(storage.IsEmpty());
        }

        private void OnCapacityUpgraded()
        {
            capacity = StorageCapacityUpgrade.CurrentStage.Value;
            emptyStorageIndicatorText.text = "0/" + capacity;

            Storage.SetCapacity(capacity);
        }

        public override void OnWorldLoaded()
        {
            base.OnWorldLoaded();
        }

        public override void OnWorldUnloaded()
        {
            base.OnWorldUnloaded();
        }

        public void OnGroundOpen(bool immediately = false)
        {
            gameObject.SetActive(true);
        }

        public void OnGroundHidden(bool immediately = false)
        {
            gameObject.SetActive(false);
        }

        public override void SpawnUnlocked()
        {
            base.SpawnUnlocked();

            if (fillUpOnGameStartap && !Storage.IsFull() && !save.Resources.IsNullOrEmpty())
            {
                var timeSpan = DateTime.Now - SaveController.LastExitTime;
                var minutes = (float)timeSpan.TotalMinutes;

                var t = Mathf.Clamp01(Mathf.InverseLerp(0, minAwayDurationInMinutes, minutes));
                var resourcesToAdd = Mathf.RoundToInt(t * Storage.SpaceLeft);
               
                for(int i = 0; i < resourcesToAdd; i++)
                {
                    Storage.AddResources(Resource.One(save.Resources.GetRandomItem()));
                }
            }
        }

        [Button]
        public void MakeFull()
        {
            while(!Storage.IsFull())
            {
                Storage.AddResources(Resource.One(Storage.RequiredResources.GetRandomItem()));
            }
        }

        private void OnDisable()
        {
            if (storage != null)
                storage.OnResourcesChanged -= OnStorageResourcesChanged;
        }

        [System.Serializable]
        private class ResourcesSave : ISaveObject
        {
            public List<CurrencyType> Resources { get; set; }

            [SerializeField] private CurrencyType[] saveCost;

            public void Init()
            {
                if (saveCost != null)
                {
                    Resources = new List<CurrencyType>(saveCost);
                }
                else
                {
                    Resources = new List<CurrencyType>();
                }
            }

            public void Flush()
            {
                saveCost = Resources.ToArray();
            }
        }
    }
}