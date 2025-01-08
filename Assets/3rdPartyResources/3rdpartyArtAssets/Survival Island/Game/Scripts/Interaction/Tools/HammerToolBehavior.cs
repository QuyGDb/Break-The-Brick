using UnityEngine;

namespace Watermelon
{
    public class HammerToolBehavior : ToolBehavior
    {
        [SerializeField] Transform hitParticleSpawnPoint;
        [SerializeField] ParticleSystem hitParticleSystem;

        public override void OnToolEnabled()
        {
            hitParticleSystem.transform.SetParent(null);
        }

        public override void OnToolDisabled()
        {
            Destroy(hitParticleSystem.gameObject);
        }

        public override void OnHitPerformed()
        {
            hitParticleSystem.transform.position = hitParticleSpawnPoint.position;
            hitParticleSystem.Play();
        }
    }
}