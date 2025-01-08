using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    // Marks the class that can be given resources
    public interface IResourceTaker
    {
        void TakeResource(FlyingResourceBehavior resource, bool fromPlayer);
        List<CurrencyType> RequiredResources { get; }

        Vector3 FlyingResourceDestination { get; }

        int RequiredMaxAmount(CurrencyType currency);

        bool IsResourceTakingBlocked { get; }

        void Rejected();
    }
}

