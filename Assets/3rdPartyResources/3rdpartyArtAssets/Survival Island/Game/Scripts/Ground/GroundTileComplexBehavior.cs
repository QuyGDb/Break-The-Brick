using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CustomOverlayElement("Ground locked/unlocked", "OnToggleValueChanged")]
    public class GroundTileComplexBehavior : AbstractComplexBehavior<GroundTileBehavior, PurchasePoint> 
    {
        private void Start()
        {
            Init();
        }

        public void OnToggleValueChanged(bool enabled)
        {
            if (unlockable == null) return;

            if (unlockable.OpenedVisuals != null)
                unlockable.OpenedVisuals.SetActive(enabled);

            if ((!enabled && isOpenFromStart))
            {
                if (unlockable.OpenedVisuals != null)
                    unlockable.OpenedVisuals.SetActive(true);
            }

            if (unlockable.ClosedVisuals != null)
                unlockable.ClosedVisuals.SetActive(!enabled);
        }
    }
}