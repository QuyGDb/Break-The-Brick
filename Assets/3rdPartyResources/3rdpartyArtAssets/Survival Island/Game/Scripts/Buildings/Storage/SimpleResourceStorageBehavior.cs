using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class SimpleResourceStorageBehavior : ResourceStorageBehavior<ResourceCanvas>
    {
        public int Capacity { get; private set; }
        public int SpaceLeft => Capacity - Storage.Sum((resource) => resource.amount);

        public List<CurrencyType> StoredResourceTypes { get; private set; }

        private bool usedAsSingleResourceStorage = false;
        private List<Resource> maxCapacityListForCaseOfSingleResourceStorage = new List<Resource>();

        public delegate void SimpleResourceCallback(CurrencyType currency);
        public event SimpleResourceCallback OnResourceAdded;

        public void Init(string saveName, List<CurrencyType> storedResourceTypes, int capacity)
        {
            Init(saveName);

            Capacity = capacity;
            StoredResourceTypes = storedResourceTypes;

            usedAsSingleResourceStorage = storedResourceTypes.Count == 1;

            if (usedAsSingleResourceStorage)
            {
                maxCapacityListForCaseOfSingleResourceStorage = new List<Resource>() { new Resource(storedResourceTypes[0], capacity) };
            }


            PopulateRequiredResources();
            UpdateCanvas();

            InvokeOnResourceChanged();
        }

        public override bool IsFull()
        {
            int count = 0;

            for (int i = 0; i < Storage.Count; i++)
            {
                count += Storage[i].amount;
            }
            return count >= Capacity;
        }

        public override int RequiredMaxAmount(CurrencyType currency)
        {
            if (!RequiredResources.IsNullOrEmpty() && RequiredResources.Contains(currency))
                return Capacity - Storage.Count;

            return 0;
        }

        protected override void UpdateCanvas()
        {
            if (usedAsSingleResourceStorage)
            {
                resourceCanvas.SetData(Storage, maxCapacityListForCaseOfSingleResourceStorage);
            }
            else
            {
                resourceCanvas.SetData(Storage);
            }

            resourceCanvas.SetFullTextActive(IsFull());
        }

        protected override void PopulateRequiredResources()
        {
            if (!IsFull())
                RequiredResources = StoredResourceTypes;
            else
                RequiredResources = null;
        }

        public void SetCapacity(int newCapacity)
        {
            Capacity = newCapacity;
            PopulateRequiredResources();

            if (usedAsSingleResourceStorage)
            {
                maxCapacityListForCaseOfSingleResourceStorage = new List<Resource>() { new Resource(StoredResourceTypes[0], Capacity) };
            }

            UpdateCanvas();
        }

        public override void TakeResource(FlyingResourceBehavior flyingResource, bool fromPlayer)
        {
            base.TakeResource(flyingResource, fromPlayer);

            OnResourceAdded?.Invoke(flyingResource.ResourceType);
        }
    }
}
