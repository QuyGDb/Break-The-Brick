using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class WorldItemCollectorCase
    {
        private IWorldItemCollector itemObject;

        private float disableTime;
        public float DisableTime => disableTime;

        private bool isDisabled = false;
        public bool IsDisabled => isDisabled;

        public WorldItemCollectorCase(IWorldItemCollector itemObject, float disableTime)
        {
            this.itemObject = itemObject;
            this.disableTime = Time.realtimeSinceStartup + disableTime;

            isDisabled = false;
        }

        public void MarkAsDisabled()
        {
            isDisabled = true;
        }

        public void Disable()
        {
            if (itemObject != null)
                itemObject.OnWorldItemCollected();

            isDisabled = true;
        }
    }
}
