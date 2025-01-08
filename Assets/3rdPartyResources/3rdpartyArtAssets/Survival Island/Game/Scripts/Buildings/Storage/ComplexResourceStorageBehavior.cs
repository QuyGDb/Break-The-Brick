using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// The purpose of this class is to take resources (including physically from the player), store them and give them away (also could be physically to the player)
    /// </summary>
    public class ComplexResourceStorageBehavior: ResourceStorageBehavior<ResourceCanvas>
    {
        private ResourcesList maxCapacity;

        public void Init(string saveName, ResourcesList maxCapacity)
        {
            Init(saveName);

            this.maxCapacity = new ResourcesList(maxCapacity);
            UpdateCanvas();

            PopulateRequiredResources();

            InvokeOnResourceChanged();
        }

        public void SetMaxCapacity(ResourcesList maxCapacity)
        {
            this.maxCapacity = new ResourcesList(maxCapacity);

            UpdateCanvas();

            PopulateRequiredResources();
        }

        protected override void UpdateCanvas()
        {
            resourceCanvas.SetData(displayedStorage, maxCapacity);
        }

        public override int RequiredMaxAmount(CurrencyType currency)
        {
            var freeSpace = maxCapacity - Storage;

            for (int i = 0; i < freeSpace.Count; i++)
            {
                var resource = freeSpace[i];

                if (resource.currency == currency)
                {
                    return resource.amount;
                }
            }

            return 0;
        }

        protected override void PopulateRequiredResources()
        {
            RequiredResources.Clear();

            for(int j = 0; j < maxCapacity.Count; j++)
            {
                var resourceType = maxCapacity[j].currency;
                var resourceCapacity = maxCapacity[j].amount;

                bool found = false;
                for(int i = 0; i < Storage.Count; i++)
                {
                    var resource = Storage[i];

                    if(resource.currency == resourceType)
                    {
                        found = true;

                        if(resourceCapacity - resource.amount > 0)
                        {
                            RequiredResources.Add(resourceType);
                        }

                        break;
                    }
                }

                if (!found)
                {
                    RequiredResources.Add(resourceType);
                }
            }
        }

        public override bool IsFull()
        {
            if (Storage == null) return true;
            for(int i = 0; i < maxCapacity.Count; i++)
            {
                var refResource = maxCapacity[i];

                bool found = false;
                for(int j = 0; j < Storage.Count; j++)
                {
                    var resource = Storage[j];
                    if (refResource.currency == resource.currency)
                    {
                        found = true;

                        if (resource.amount < refResource.amount) return false;
                    }
                }

                if (!found) return false;
            }

            return true;
        }

        [Button]
        public void IsFullCheck()
        {
            Debug.Log("IsFull: " + IsFull());
        }
    }
}