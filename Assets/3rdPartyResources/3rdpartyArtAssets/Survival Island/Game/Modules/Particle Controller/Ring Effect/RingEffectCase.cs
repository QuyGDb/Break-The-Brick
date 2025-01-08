using UnityEngine;

namespace Watermelon
{
    public class RingEffectCase : TweenCase
    {
        private static readonly int SHADER_SCALE_PROPERTY = Shader.PropertyToID("_Scale");
        private static readonly int SHADER_COLOR_PROPERTY = Shader.PropertyToID("_Color");

        private GameObject ringGameObject;
        private MeshRenderer ringMeshRenderer;

        private MaterialPropertyBlock materialPropertyBlock;

        private float targetSize;
        private Gradient targetGradient;

        public RingEffectCase(GameObject gameObject, float targetSize, Gradient targetGradient)
        {
            ringGameObject = gameObject;
            ringMeshRenderer = ringGameObject.GetComponent<MeshRenderer>();

            this.targetGradient = targetGradient;
            this.targetSize = targetSize;

            materialPropertyBlock = new MaterialPropertyBlock();

            ringMeshRenderer.GetPropertyBlock(materialPropertyBlock);
            //materialPropertyBlock.SetFloat(SHADER_SCALE_PROPERTY, 0.1f);
            materialPropertyBlock.SetColor(SHADER_COLOR_PROPERTY, targetGradient.Evaluate(0.0f));
            ringMeshRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        public override void DefaultComplete()
        {
            ringGameObject.transform.localScale = targetSize.ToVector3();

            ringMeshRenderer.GetPropertyBlock(materialPropertyBlock);
            //materialPropertyBlock.SetFloat(SHADER_SCALE_PROPERTY, targetSize);
            materialPropertyBlock.SetColor(SHADER_COLOR_PROPERTY, targetGradient.Evaluate(1.0f));
            ringMeshRenderer.SetPropertyBlock(materialPropertyBlock);

            ringGameObject.SetActive(false);
        }

        public override void Invoke(float deltaTime)
        {
            float interpolatedState = Interpolate(State);

            ringMeshRenderer.GetPropertyBlock(materialPropertyBlock);
            //materialPropertyBlock.SetFloat(SHADER_SCALE_PROPERTY, Mathf.LerpUnclamped(0.1f, targetSize, interpolatedState));
            materialPropertyBlock.SetColor(SHADER_COLOR_PROPERTY, targetGradient.Evaluate(interpolatedState));
            ringMeshRenderer.SetPropertyBlock(materialPropertyBlock);

            ringGameObject.transform.localScale = Vector3.one * Mathf.LerpUnclamped(0.1f, targetSize, interpolatedState);
        }

        public override bool Validate()
        {
            return true;
        }
    }
}
