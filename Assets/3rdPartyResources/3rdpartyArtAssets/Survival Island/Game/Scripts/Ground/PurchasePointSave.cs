using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PurchasePointSave : ResourceListSave
    {
        [SerializeField] bool isBought;
        public bool IsBought { get => isBought; set => isBought = value; }
    }
}
