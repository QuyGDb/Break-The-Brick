using UnityEngine;

namespace Watermelon
{
    public class ToolBehavior : MonoBehaviour
    {
        public virtual void Initialise(InteractionAnimationType animationType) { }

        public virtual void OnToolEnabled() { }
        public virtual void OnToolDisabled() { }

        public virtual void OnHitPerformed() { }

        public virtual void OnCustomEventInvoked(string eventName) { }
    }
}