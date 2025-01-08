using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class AbstractHitableBehavior : MonoBehaviour, IHitable
    {
        [SerializeField]
        [BoxFoldout("Hittable", "Hittable")]
        protected InteractionAnimationType interactionAnimationType;
        public InteractionAnimationType InteractionAnimationType => interactionAnimationType;

        public virtual Transform SnappingTransform => transform;

        public bool IsActive { get; protected set; }

        public abstract bool IsMutlipleObjectsHitRestricted { get; }
        public abstract int HittableID { get; }

        public abstract void GetHit(Vector3 hitSourcePosition, bool param = true, IHitter resourcePicked = null);
        public abstract bool IsHittable();

        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField] bool rotateBeforeHit;
        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField] protected bool hasSnappingDistance;
        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField] float snappingDistance;
        [BoxFoldout("Hittable", "Hittable")]
        [SerializeField] float snappingSpeedMultiplier;

        public bool RotateBeforeHit => rotateBeforeHit;
        public bool HasSnappingDistance => hasSnappingDistance;
        public float SnappingDistance => snappingDistance;
        public float SnappingSpeedMultiplier => snappingSpeedMultiplier;

        public virtual void ActivateInteractionAnimation(Interaction interactionAnimations)
        {
            interactionAnimations.Activate(interactionAnimationType);
        }
    }
}