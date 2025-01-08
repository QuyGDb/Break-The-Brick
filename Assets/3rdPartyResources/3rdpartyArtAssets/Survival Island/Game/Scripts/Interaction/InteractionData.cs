using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Interaction Data", menuName = "Content/Interaction Data")]
    public class InteractionData : ScriptableObject
    {
        [SerializeField] InteractionAnimationData defaultAnimationData;
        public InteractionAnimationData DefaultAnimationData => defaultAnimationData;

        [SerializeField] InteractionAnimationData[] animationsData;
        public InteractionAnimationData[] AnimationsData => animationsData;
    }
}