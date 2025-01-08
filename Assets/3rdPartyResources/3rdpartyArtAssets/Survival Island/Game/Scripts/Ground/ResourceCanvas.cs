using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public abstract class ResourceCanvas: WorldSpaceCanvas
    {
        [Space]
        [SerializeField] protected GameObject resourceUIPrefab;
        [SerializeField] protected Transform resourceUIHolder;

        public abstract void SetData(List<Resource> currentResources, List<Resource> maxCapacity);
        public abstract void SetData(List<Resource> resources);

        public abstract void SetFullTextActive(bool isActive);
    }
}