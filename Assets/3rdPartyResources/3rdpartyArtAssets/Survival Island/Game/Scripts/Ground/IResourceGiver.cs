using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public interface IResourceGiver
    {
        Vector3 FlyingResourceSpawnPosition { get; }

        float LastTimeResourceGiven { get; }
        bool IsResourceGivingBlocked { get; }

        bool HasResource(Resource resource);
        int GetResourceCount(CurrencyType currencyType);
        void GiveResource(Resource resource);

        bool HasResources();

        bool IsPlayer { get; }
    }

    [Flags]
    public enum ResourceCarrierType
    {
        Player = 1,
        Helper = 2,
    }
}