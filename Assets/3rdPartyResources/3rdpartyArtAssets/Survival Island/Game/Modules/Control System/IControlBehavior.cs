using UnityEngine;

namespace Watermelon
{
    public interface IControlBehavior
    {
        public Vector3 MovementInput { get; }
        public bool IsMovementInputNonZero { get; }

        public void EnableMovementControl();
        public void DisableMovementControl();
        public void ResetControl();

        public event SimpleCallback OnMovementInputActivated;
    }
}