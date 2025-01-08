using UnityEngine;

namespace Watermelon
{
    public abstract class FlyingResourceAnimation : ScriptableObject
    {
        public abstract TweenCaseCollection StartAnimation(FlyingResourceBehavior flyingResourceBehavior, Vector3 destinationPoint, SimpleCallback onAnimationCompleted);
    }
}