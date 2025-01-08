#pragma warning disable 0067

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if MODULE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.UI;
#endif

namespace Watermelon
{
    public class GamepadControl : MonoBehaviour, IControlBehavior
    {
        //Left Stick x and y axes
        public Vector3 MovementInput { get; private set; }
        public bool IsMovementInputNonZero { get; private set; }

        public bool IsMovementControlActive { get; private set; }

        public event SimpleCallback OnMovementInputActivated;

        private static Dictionary<GamepadButtonType, float> gamepadButtonPressedTime = new Dictionary<GamepadButtonType, float>();

        public void Initialise()
        {
            if (Control.InputType == InputType.Gamepad)
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
            if (Gamepad.current == null || Keyboard.current.wasUpdatedThisFrame)
            {
                Control.ChangeInputType(InputType.Keyboard);

                return;
            }

            GamepadButtonsUpdate();

            if (!IsMovementControlActive) return;

            float horizontalInput = Gamepad.current.leftStick.x.value;
            float verticalInput = Gamepad.current.leftStick.y.value;

            MovementInput = Vector3.ClampMagnitude(new Vector3(horizontalInput, 0, verticalInput), 1);

            if (!IsMovementInputNonZero && MovementInput.magnitude > 0.1f)
            {
                IsMovementInputNonZero = true;

                OnMovementInputActivated?.Invoke();
            }

            IsMovementInputNonZero = MovementInput.magnitude > 0.1f;
#endif
        }

        // Dev: reasons for adding this method: it helps to abstract ui gamepad buttons from the actual gamepad buttons, and keeps the code cleaner
        public static bool WasButtonPressedThisFrame(GamepadButtonType button)
        {
#if MODULE_INPUT_SYSTEM
            if(Gamepad.current == null) return false;

            switch (button)
            {
                case GamepadButtonType.A: return Gamepad.current.aButton.wasPressedThisFrame;
                case GamepadButtonType.B: return Gamepad.current.bButton.wasPressedThisFrame;
                case GamepadButtonType.X: return Gamepad.current.xButton.wasPressedThisFrame;
                case GamepadButtonType.Y: return Gamepad.current.yButton.wasPressedThisFrame;

                case GamepadButtonType.Start: return Gamepad.current.startButton.wasPressedThisFrame;
                case GamepadButtonType.Select: return Gamepad.current.selectButton.wasPressedThisFrame;

                case GamepadButtonType.DDown: return Gamepad.current.dpad.down.wasPressedThisFrame;
                case GamepadButtonType.DUp: return Gamepad.current.dpad.up.wasPressedThisFrame;
                case GamepadButtonType.DLeft: return Gamepad.current.dpad.left.wasPressedThisFrame;
                case GamepadButtonType.DRight: return Gamepad.current.dpad.right.wasPressedThisFrame;

                case GamepadButtonType.LB: return Gamepad.current.leftShoulder.wasPressedThisFrame;
                case GamepadButtonType.RB: return Gamepad.current.rightShoulder.wasPressedThisFrame;

                case GamepadButtonType.L3: return Gamepad.current.leftStickButton.wasPressedThisFrame;
                case GamepadButtonType.R3: return Gamepad.current.rightStickButton.wasPressedThisFrame;

                default: return false;
            }
#else
            return false;
#endif
        }

        public static float GetButtonPressedTime(GamepadButtonType type)
        {
            if (gamepadButtonPressedTime.ContainsKey(type))
            {
                return gamepadButtonPressedTime[type];
            }

            return -1;
        }

#if MODULE_INPUT_SYSTEM
        private void GamepadButtonsUpdate()
        {
            GamepadButtonUpdate(GamepadButtonType.A, Gamepad.current.aButton.isPressed);
            GamepadButtonUpdate(GamepadButtonType.B, Gamepad.current.bButton.isPressed);
            GamepadButtonUpdate(GamepadButtonType.X, Gamepad.current.xButton.isPressed);
            GamepadButtonUpdate(GamepadButtonType.Y, Gamepad.current.yButton.isPressed);

            GamepadButtonUpdate(GamepadButtonType.DDown, Gamepad.current.dpad.down.isPressed);
            GamepadButtonUpdate(GamepadButtonType.DUp, Gamepad.current.dpad.up.isPressed);
            GamepadButtonUpdate(GamepadButtonType.DLeft, Gamepad.current.dpad.left.isPressed);
            GamepadButtonUpdate(GamepadButtonType.DRight, Gamepad.current.dpad.right.isPressed);

            GamepadButtonUpdate(GamepadButtonType.LB, Gamepad.current.leftShoulder.isPressed);
            GamepadButtonUpdate(GamepadButtonType.RB, Gamepad.current.rightShoulder.isPressed);

            GamepadButtonUpdate(GamepadButtonType.LT, Gamepad.current.leftTrigger.isPressed);
            GamepadButtonUpdate(GamepadButtonType.RT, Gamepad.current.rightTrigger.isPressed);

            GamepadButtonUpdate(GamepadButtonType.L3, Gamepad.current.leftStickButton.isPressed);
            GamepadButtonUpdate(GamepadButtonType.R3, Gamepad.current.rightStickButton.isPressed);

            GamepadButtonUpdate(GamepadButtonType.Start, Gamepad.current.startButton.isPressed);
            GamepadButtonUpdate(GamepadButtonType.Select, Gamepad.current.selectButton.isPressed);
        }
#endif

        private void GamepadButtonUpdate(GamepadButtonType type, bool isPressed)
        {
            if (isPressed)
            {
                if (!gamepadButtonPressedTime.ContainsKey(type)) gamepadButtonPressedTime.Add(type, Time.time);
            } else
            {
                if (gamepadButtonPressedTime.ContainsKey(type)) gamepadButtonPressedTime.Remove(type);
            }                
        }



        #region Control management

        public void DisableMovementControl()
        {
            IsMovementControlActive = false;
            MovementInput = Vector3.zero;
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