using UnityEngine;

namespace Watermelon
{
    public class PlayerAnimationHandler : MonoBehaviour
    {
        private PlayerGraphics playerGraphics;
        private PlayerBehavior playerBehavior;

        private Interaction interaction;

        public void Inititalise(PlayerGraphics playerGraphics, PlayerBehavior playerBehavior)
        {
            this.playerGraphics = playerGraphics;
            this.playerBehavior = playerBehavior;

            interaction = playerGraphics.InteractionAnimations;
        }

        public void OnHit()
        {
            if (!interaction.IsAnimationActive)
                return;

            interaction.InvokeHitEvent();

            playerBehavior.OnResourceHit();
        }

        public void OnCustomInteractionEvent(string eventName)
        {
            interaction.InvokeCustomEvent(eventName);
        }

        public void Step()
        {
            playerGraphics.Step();
        }
    }
}