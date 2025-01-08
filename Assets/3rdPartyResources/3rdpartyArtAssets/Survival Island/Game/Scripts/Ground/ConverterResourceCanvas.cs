using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ConverterResourceCanvas : ResourceCanvas
    {
        private List<ConverterResourceUI> resourceUIList = new List<ConverterResourceUI>();
        public List<ConverterResourceUI> ResourceUIList => resourceUIList;

        protected override void Awake()
        {
            base.Awake();

            InitialiseUIPanels();
        }

        private void InitialiseUIPanels()
        {
            resourceUIList.Clear();

            for (int i = 0; i < resourceUIHolder.childCount; i++)
            {
                ConverterResourceUI resUI = resourceUIHolder.GetChild(i).GetComponent<ConverterResourceUI>();

                if (resUI != null)
                {
                    resourceUIList.Add(resUI);
                }
            }
        }

        public override void SetData(List<Resource> currentResources, List<Resource> maxCapacity)
        {
            if (resourceUIList.Count <= maxCapacity.Count)
            {
                // adding more resource ui
                for (int i = resourceUIList.Count; i < maxCapacity.Count; i++)
                {
                    ConverterResourceUI resUI = Instantiate(resourceUIPrefab, resourceUIHolder).GetComponent<ConverterResourceUI>();
                    resourceUIList.Add(resUI);
                }
            }
            else
            {
                // removing unneeded resource ui
                for (int i = resourceUIList.Count - 1; i > maxCapacity.Count - 1; i--)
                {
                    Destroy(resourceUIList[i].gameObject);
                    resourceUIList.RemoveAt(i);
                }
            }

            for (int i = 0; i < maxCapacity.Count; i++)
            {
                Resource capacity = maxCapacity[i];

                Resource currentAmount = Resource.Zero(capacity.currency);

                for (int j = 0; j < currentResources.Count; j++)
                {
                    var testPrice = currentResources[j];

                    if (capacity.currency == testPrice.currency)
                    {
                        currentAmount = testPrice;

                        break;
                    }
                }

                string amountText = currentAmount.amount + "/" + capacity.amount;
                resourceUIList[i].SetData(currentAmount.currency, amountText);
            }

        }

        public override void SetData(List<Resource> resources)
        {
            
        }

        public override void SetFullTextActive(bool isActive)
        {
            
        }
    }
}