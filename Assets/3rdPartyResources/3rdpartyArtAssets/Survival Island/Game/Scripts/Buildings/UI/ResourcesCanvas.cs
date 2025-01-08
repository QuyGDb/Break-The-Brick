using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class ResourcesCanvas : ResourceCanvas
    {
        private List<ResourceUI> resourceUIList = new List<ResourceUI>();

        [SerializeField] TMP_Text fullText;

        protected override void Awake()
        {
            base.Awake();

            InitialiseUIPanels();

            SetFullTextActive(false);
        }

        private void InitialiseUIPanels()
        {
            resourceUIList.Clear();

            for (int i = 0; i < resourceUIHolder.childCount; i++)
            {
                ResourceUI resUI = resourceUIHolder.GetChild(i).GetComponent<ResourceUI>();

                if (resUI != null)
                {
                    resourceUIList.Add(resUI);
                }
            }
        }

        public override void SetData(List<Resource> resources)
        {
            UpdateResourcesUIPanel(resources.Count);

            for (int i = 0; i < resources.Count; i++)
            {
                resourceUIList[i].SetData(resources[i].currency, resources[i].amount.ToString());
            }
        }

        public override void SetData(List<Resource> currentResources, List<Resource> maxCapacity)
        {
            UpdateResourcesUIPanel(maxCapacity.Count);

            for (int i = 0; i < maxCapacity.Count; i++)
            {
                Resource capacity = maxCapacity[i];

                Resource price = Resource.Zero(capacity.currency);

                for (int j = 0; j < currentResources.Count; j++)
                {
                    var testPrice = currentResources[j];

                    if (capacity.currency == testPrice.currency)
                    {
                        price = testPrice;

                        break;
                    }
                }

                string amountText = price.amount + "/" + capacity.amount;
                resourceUIList[i].SetData(price.currency, amountText);
            }
        }

        public override void SetFullTextActive(bool isActive)
        {
            fullText.gameObject.SetActive(isActive);
        }

        private void UpdateResourcesUIPanel(int amount)
        {
            // this override allows to use this method in the editor
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                InitialiseUIPanels();
            }
#endif

            if (resourceUIList.Count <= amount)
            {
                // adding more resource ui
                for (int i = resourceUIList.Count; i < amount; i++)
                {
                    ResourceUI resUI = Instantiate(resourceUIPrefab, resourceUIHolder).GetComponent<ResourceUI>();
                    resourceUIList.Add(resUI);
                }
            }
            else
            {
                // removing unneeded resource ui
                for (int i = resourceUIList.Count - 1; i > amount - 1; i--)
                {
                    // this override allows to use this method in the editor
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        DestroyImmediate(resourceUIList[i].gameObject);
                        resourceUIList.RemoveAt(i);
                        return;
                    }
#endif

                    Destroy(resourceUIList[i].gameObject);
                    resourceUIList.RemoveAt(i);
                }
            }
        }
    }
}