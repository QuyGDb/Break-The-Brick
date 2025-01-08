using UnityEngine;

namespace Watermelon
{
    public abstract class CharacterGraphics : MonoBehaviour
    {
        [BoxGroup("References")]
        [SerializeField] protected Animator animator;
        public Animator Animator => animator;

        [BoxGroup("References"), UnpackNested]
        [SerializeField] protected Interaction interactionAnimations;
        public Interaction InteractionAnimations => interactionAnimations;

        protected AnimatorOverrideController overrideAnimatorController;

        protected void PrepareComponents()
        {
            // Create a runtime Animator Controller
            RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;

            // Create a new AnimatorOverrideController based on the runtime controller
            overrideAnimatorController = new AnimatorOverrideController(runtimeAnimatorController);

            // Assign the override controller back to the animator
            animator.runtimeAnimatorController = overrideAnimatorController;
        }

        public void OverrideAnimation(string name, AnimationClip animationClip)
        {
            overrideAnimatorController[name] = animationClip;
        }
    }
}