using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ResGeneratorSplitAnimation : MonoBehaviour
    {
        [SerializeField] ResourceGeneratorBuildingBehavior generatorRef;

        [SerializeField] Transform rotationTransform;
        [SerializeField] float rotationSpeed;

        [SerializeField] Transform resStartPosition;
        [SerializeField] Transform resControlPosition;
        [SerializeField] Transform resDisappearPosition;

        private Transform flyingResTransform1;
        private Transform flyingResTransform2;

        private void Start()
        {
            StartCoroutine(ResSplitAnimationCoroutine());

            flyingResTransform1 = CurrenciesController.GetCurrency(generatorRef.ResourceType).Data.FlyingResPool.GetPooledObject().transform;
            flyingResTransform2 = CurrenciesController.GetCurrency(generatorRef.ResourceType).Data.FlyingResPool.GetPooledObject().transform;

            flyingResTransform1.localScale = Vector3.zero;
            flyingResTransform2.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (!generatorRef.IsAnimationRunning)
                return;

            rotationTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        private IEnumerator ResSplitAnimationCoroutine()
        {
            WaitForSeconds delay = new WaitForSeconds(0.2f);

            while (true)
            {
                if (generatorRef.IsAnimationRunning)
                {
                    float animationDuration = 1f;

                    flyingResTransform2.localScale = Vector3.one;

                    // position
                    flyingResTransform1.position = resStartPosition.position;
                    flyingResTransform2.position = resStartPosition.position;

                    float sideOffset = 0.3f;

                    flyingResTransform1.DOBezierMove(resDisappearPosition.position.AddToZ(-sideOffset), resControlPosition.position.AddToZ(-sideOffset * 0.5f), animationDuration * 0.8f);
                    flyingResTransform2.DOBezierMove(resDisappearPosition.position.AddToZ(sideOffset), resControlPosition.position.AddToZ(sideOffset * 0.5f), animationDuration * 0.8f);

                    // scale
                    flyingResTransform1.localScale = Vector3.zero;

                    flyingResTransform1.DOScale(1f, animationDuration * 0.05f);

                    yield return new WaitForSeconds(animationDuration * 0.4f);

                    // split section
                    flyingResTransform2.localScale = Vector3.one;

                    flyingResTransform1.DOScale(0f, animationDuration * 0.4f).SetEasing(Ease.Type.QuartIn);
                    flyingResTransform2.DOScale(0f, animationDuration * 0.4f).SetEasing(Ease.Type.QuartIn);

                    yield return new WaitForSeconds(animationDuration * 0.4f);

                    flyingResTransform1.localScale = Vector3.zero;
                    flyingResTransform2.localScale = Vector3.zero;

                    // little pause
                    yield return new WaitForSeconds(animationDuration * 0.2f);
                }
                else
                {
                    yield return delay;
                }
            }
        }
    }
}
