#pragma warning disable 0067
#pragma warning disable 0414

using UnityEngine;

#if MODULE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Watermelon
{
    public class KeyboardControl : MonoBehaviour, IControlBehavior
    {
        // WASD and arrow keys
        public Vector3 MovementInput { get; private set; }
        public bool IsMovementInputNonZero { get; private set; }

        private bool IsMovementControlActive;

        public event SimpleCallback OnMovementInputActivated;

        public void Initialise()
        {
            if (Control.InputType == InputType.Keyboard)
            {
                Control.SetControl(this);

                // As Behavior.enabled, inherited variable
                enabled = true;
                IsMovementControlActive = true;
            }
            else
            {
                enabled = false;
            }
        }

        private void Update()
        {
#if MODULE_INPUT_SYSTEM
            // Dev: not 100% sure this 'if' statement works in every scenario, but so far so good
            if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame && !Gamepad.current.CheckStateIsAtDefaultIgnoringNoise())
            {
                Control.ChangeInputType(InputType.Gamepad);

                Destroy(this);
                return;
            }

            if(Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                WorldSpaceRaycaster.Raycast(Mouse.current.position.value);
            }

            if (!IsMovementControlActive) return;
            if (Keyboard.current == null) return;

            float horizontalInput = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1 : 0;
            horizontalInput += Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? -1 : 0;

            float verticalInput = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1 : 0;
            verticalInput += Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? -1 : 0;

            MovementInput = Vector3.ClampMagnitude(new Vector3(horizontalInput, 0, verticalInput), 1);

            if(!IsMovementInputNonZero && MovementInput.magnitude > 0.1f)
            {
                IsMovementInputNonZero = true;

                OnMovementInputActivated?.Invoke();
            }

            IsMovementInputNonZero = MovementInput.magnitude > 0.1f;
#endif
        }

#region Control management

        public void DisableMovementControl()
        {
            IsMovementControlActive = false;
        }

        public void EnableMovementControl()
        {
            IsMovementControlActive = true;
        }

        public void ResetControl()
        {
            IsMovementInputNonZero = false;
            MovementInput = Vector3.zero;
        }

#endregion
    }
}