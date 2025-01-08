using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Marks the class that can take resources in order to unlock another object marked as 'IPurchasable'
    /// </summary>
    public interface IPurchaser
    {
        bool Init(IUnlockableComplex unlockableComplex);

        public ResourcesList CostLeft { get; }

        event SimpleCallback OnPurhcased;
        event SimpleCallback OnResourcePlaced;

        public void Enable();
        public void Disable();

        public void Destroy();

        // editor feature
        public void UpdateCostInEditor(List<Resource> cost);

        public bool LookUpPurchased(IUnlockableComplex unlockableComplex);
    }
}