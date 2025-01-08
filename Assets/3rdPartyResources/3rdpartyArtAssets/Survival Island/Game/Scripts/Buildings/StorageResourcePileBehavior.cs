using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class StorageResourcePileBehavior : ResourcePileBehaviour
    {
        [Header("Storage")]
        [SerializeField] SimpleResourceStorageBehavior storageReference;
        [SerializeField] ResourcePileVisType visualizationType;

        private void OnEnable()
        {
            if (storageReference == null)
            {
                Debug.Log("Storage pile is not referenced. Object name: " + name);
                return;
            }

            storageReference.OnResourcesChanged += OnResourceChanged;
        }

        private void OnResourceChanged()
        {
            int resourcesInStorageAmount = storageReference.GetResourceCount(currencyToVisualise);
            int difference = 0;

            if (storageReference.Capacity == 0)
                return;

            if (visualizationType == ResourcePileVisType.OneToOne)
            {
                difference = resourcesInStorageAmount - activeObjets.Count;
            }
            else
            {
                float currentFillRelation = (float)resourcesInStorageAmount / storageReference.Capacity;

                int requiredElementsInStorage = Mathf.CeilToInt(currentFillRelation * PileCapacity);

                difference = requiredElementsInStorage - activeObjets.Count;
            }

            if (difference == 0)
                return;

            if (difference > 0)
            {
                AddResources(difference);
            }
            else
            {
                RemoveResources(Mathf.Abs(difference));
            }
        }

        private void OnDisable()
        {
            if (storageReference == null)
                return;

            storageReference.OnResourcesChanged -= OnResourceChanged;
            
            Unload();
        }
    }

    public enum ResourcePileVisType
    {
        OneToOne = 0,
        Relative = 1,
    }
}