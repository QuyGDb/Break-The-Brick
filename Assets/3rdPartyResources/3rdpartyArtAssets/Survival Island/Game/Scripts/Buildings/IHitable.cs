using UnityEngine;

namespace Watermelon
{
    public interface IHitable
    {
        InteractionAnimationType InteractionAnimationType { get; }

        Transform SnappingTransform { get; }

        bool IsActive { get; }
        bool IsMutlipleObjectsHitRestricted { get; }
        int HittableID { get; }

        bool HasSnappingDistance { get; }
        float SnappingDistance { get; }
        float SnappingSpeedMultiplier { get; }

        bool RotateBeforeHit { get; }

        void ActivateInteractionAnimation(Interaction interactionAnimations);
        void GetHit(Vector3 hitSourcePosition, bool param = true, IHitter resourcePicked = null);
        bool IsHittable();
    }
}