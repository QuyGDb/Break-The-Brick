using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public interface IDistanceToggle
    {
        public bool DistanceToggleActivated { get; } // if false - will be ignored by distance toggle sytem
        public bool IsDistanceToggleInCloseMode { get; } // true if object is in the "player is close" state

        public float ActivationDistanceOfDT { get; } // distance that will trigger zone crossing method
        public Vector3 OriginPositionOfDT { get; } // position make calculate distance from

        public void PlayerEnteredZone(); // called when player is closer than Activation Distance
        public void PlayerLeavedZone(); // called whn playr is further than Activation Distance
    }
}