using UnityEngine;

namespace Watermelon
{
    public class FishingRodToolBehavior : ToolBehavior
    {
        public const string EVENT_ENABLE_FISHING_FLOAT = "EnableFishingFloat";
        public const string EVENT_DISABLE_FISHING_FLOAT = "DisableFishingFloat";

        [SerializeField] Transform lineStartPoint;
        [SerializeField] LineRenderer lineRenderer;

        [Space]
        [SerializeField] Transform fishingFloat;

        private Transform targetTransform;

        private void Update()
        {
            if (targetTransform == null) return;

            // Recalculate line renderer
            lineRenderer.SetPosition(0, lineStartPoint.position);
            lineRenderer.SetPosition(1, targetTransform.position);
        }

        public override void OnToolEnabled()
        {
            base.OnToolEnabled();

            lineRenderer.enabled = false;
            fishingFloat.gameObject.SetActive(false);
        }

        public void SetTargetTransform(Transform transform)
        {
            targetTransform = transform;

            fishingFloat.SetParent(null);
            fishingFloat.SetPositionAndRotation(targetTransform.position, Quaternion.identity);
        }

        public override void OnCustomEventInvoked(string eventName)
        {
            switch (eventName)
            {
                case EVENT_ENABLE_FISHING_FLOAT:
                    fishingFloat.gameObject.SetActive(true);
                    lineRenderer.enabled = true;
                    break;
                case EVENT_DISABLE_FISHING_FLOAT:
                    fishingFloat.gameObject.SetActive(false);
                    lineRenderer.enabled = false;
                    break;
            }
        }

        private void OnDestroy()
        {
            if(fishingFloat != null)
                Destroy(fishingFloat.gameObject);
        }
    }
}