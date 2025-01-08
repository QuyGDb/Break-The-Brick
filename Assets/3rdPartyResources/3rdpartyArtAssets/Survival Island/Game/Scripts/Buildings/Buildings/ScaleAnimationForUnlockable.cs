using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ScaleAnimationForUnlockable : AnimationForUnlockable
    {
        [SerializeField] protected bool apearAnimation;
        [SerializeField, ShowIf("apearAnimation")] protected List<Transform> objectsToAppear = new List<Transform>();

        [SerializeField] protected bool hideAnimation;
        [SerializeField, ShowIf("hideAnimation")] protected List<Transform> objectsToHide = new List<Transform>();

        [Space]
        [SerializeField] float scaleDuration = 0.3f;
        [SerializeField] DuoFloat radomDelay = new DuoFloat(0f, 0.2f);

        public override float TotalAnimationDuration => scaleDuration + radomDelay.Max;

        public override void RunUnlockedAnimation()
        {
            if (apearAnimation && !objectsToAppear.IsNullOrEmpty())
            {
                for (int i = 0; i < objectsToAppear.Count; i++)
                {
                    Vector3 initialScale = objectsToAppear[i].localScale;
                    objectsToAppear[i].localScale = Vector3.zero;

                    objectsToAppear[i].DOScale(initialScale, scaleDuration, radomDelay.Random()).SetEasing(Ease.Type.BackOut);
                }
            }

            if (hideAnimation && !objectsToHide.IsNullOrEmpty())
            {
                for (int i = 0; i < objectsToHide.Count; i++)
                {
                    objectsToHide[i].DOScale(0f, scaleDuration, radomDelay.Random()).SetEasing(Ease.Type.BackIn);
                }
            }
        }
    }
}
