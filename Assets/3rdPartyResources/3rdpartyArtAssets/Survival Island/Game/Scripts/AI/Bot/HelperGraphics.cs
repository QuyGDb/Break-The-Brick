using UnityEngine;

namespace Watermelon
{
    public class HelperGraphics : CharacterGraphics
    {
        private HelperBehavior helperBehavior;

        public void Inititalise(HelperBehavior helperBehavior)
        {
            this.helperBehavior = helperBehavior;

            // Cache animator and create override animator controller
            PrepareComponents();

            interactionAnimations.Initialise(animator);
        }

        public void OnHit()
        {
            if (!interactionAnimations.IsAnimationActive)
                return;

            interactionAnimations.InvokeHitEvent();

            helperBehavior.OnResourceHit();
        }

        public void Step()
        {

        }

        public void OnCustomInteractionEvent(string eventName)
        {
            interactionAnimations.InvokeCustomEvent(eventName);
        }
    }
}