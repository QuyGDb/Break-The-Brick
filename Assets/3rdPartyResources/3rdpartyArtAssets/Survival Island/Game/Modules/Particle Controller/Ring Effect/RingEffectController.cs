using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class RingEffectController : MonoBehaviour
    {
        private static RingEffectController ringEffectController;

        [SerializeField] GameObject ringEffectPrefab;
        [SerializeField] Gradient defaultGradient;

        private Pool ringEffectPool;

        private void Awake()
        {
            ringEffectController = this;

            ringEffectPool = new Pool(new PoolSettings(ringEffectPrefab.name, ringEffectPrefab, 1, true));
        }

        public static RingEffectCase SpawnEffect(Vector3 position, float targetSize, float time, Ease.Type easing)
        {
            return SpawnEffect(position, ringEffectController.defaultGradient, targetSize, time, easing);
        }

        public static RingEffectCase SpawnEffect(Vector3 position, Gradient gradient, float targetSize, float time, Ease.Type easing)
        {
            GameObject ringObject = ringEffectController.ringEffectPool.GetPooledObject();
            ringObject.transform.position = position;
            ringObject.transform.localScale = Vector3.zero;
            ringObject.SetActive(true);

            RingEffectCase ringEffectCase = new RingEffectCase(ringObject, targetSize, gradient);

            ringEffectCase.SetDuration(time);
            ringEffectCase.SetEasing(easing);
            ringEffectCase.StartTween();

            return ringEffectCase;
        }
    }
}
