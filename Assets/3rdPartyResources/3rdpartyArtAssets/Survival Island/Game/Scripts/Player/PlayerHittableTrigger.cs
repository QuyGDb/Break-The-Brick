using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerHittableTrigger : MonoBehaviour
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
            if(other.gameObject.layer == PhysicsHelper.LAYER_HITTABLE)
            {
                IHitable resource = other.gameObject.GetComponent<IHitable>();

                player.OnResourceInRange(resource);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_HITTABLE)
            {
                IHitable resource = other.gameObject.GetComponent<IHitable>();

                player.OnHittableOutsideRangeOrCompleted(resource);
            }
        }
    }
}