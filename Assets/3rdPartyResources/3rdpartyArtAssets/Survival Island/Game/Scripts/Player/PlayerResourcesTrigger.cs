using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerResourcesTrigger : MonoBehaviour
    {
        private SphereCollider sphereCollider;
        public SphereCollider SphereCollider => sphereCollider;

        private PlayerBehavior player;

        public void Init(PlayerBehavior player)
        {
            this.player = player;

            sphereCollider = GetComponent<SphereCollider>();
        }

        public void SetRadius(float radius)
        {
            sphereCollider.radius = radius;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_RESOURCE)
            {
                ResourceDropBehavior dropBehavior = other.gameObject.GetComponent<ResourceDropBehavior>();
                if (dropBehavior != null && !dropBehavior.IsFlying && !dropBehavior.IsPicked)
                {
                    dropBehavior.PerformPick(player);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_RESOURCE)
            {
                ResourceDropBehavior dropBehavior = other.gameObject.GetComponent<ResourceDropBehavior>();
                if (dropBehavior != null && !dropBehavior.IsFlying && !dropBehavior.IsPicked)
                {
                    dropBehavior.PerformPick(player);
                }
            }
        }
    }
}