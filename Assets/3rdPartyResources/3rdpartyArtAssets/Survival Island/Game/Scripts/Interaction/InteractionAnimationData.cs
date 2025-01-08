using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class InteractionAnimationData
    {
        [SerializeField] InteractionAnimationType animationType;
        public InteractionAnimationType AnimationType => animationType;

        [SerializeField] AnimationClip animationClip;
        public AnimationClip AnimationClip => animationClip;

        [SerializeField] ToolBehavior toolBehaviorPrefab;
        public ToolBehavior ToolBehaviorPrefab => toolBehaviorPrefab;

        [SerializeField] Sprite interactionIcon;
        public Sprite InteractionIcon => interactionIcon;
    }
}