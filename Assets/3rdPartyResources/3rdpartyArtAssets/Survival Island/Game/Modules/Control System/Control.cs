using UnityEngine;

namespace Watermelon
{
    public static class Control
    {
        public static InputType InputType { get; private set; }

        public static IControlBehavior CurrentControl { get; private set; }

        public static GamepadData GamepadData { get; private set; }

        public delegate void OnInputChangedCallback(InputType input);
        public static event OnInputChangedCallback OnInputChanged;

        public static void Initialise(InputType inputType, GamepadData gamepadData)
        {
            InputType = inputType;
            GamepadData = gamepadData;

            if (GamepadData != null) GamepadData.Initialise();
        }

        public static void ChangeInputType(InputType inputType)
        {
            InputType = inputType;

            Object.Destroy(CurrentControl as MonoBehaviour);

            switch (inputType)
            {
                case InputType.Gamepad:

                    GamepadControl gamepadControl = Initialiser.InitialiserGameObject.AddComponent<GamepadControl>();
                    gamepadControl.Initialise();

                    CurrentControl = gamepadControl;

                    break;

                case InputType.Keyboard:
                    KeyboardControl keyboardControl = Initialiser.InitialiserGameObject.AddComponent<KeyboardControl>();
                    keyboardControl.Initialise();

                    CurrentControl = keyboardControl;
                    break;
            }

            OnInputChanged?.Invoke(inputType);
        }

        public static void SetControl(IControlBehavior controlBehavior)
        {
            CurrentControl = controlBehavior;
        }

        public static void EnableMovementControl()
        {
#if UNITY_EDITOR
            if(CurrentControl == null)
            {
                Debug.LogError("[Control]: Control behavior isn't set!");

                return;
            }
#endif

            CurrentControl.EnableMovementControl();
        }

        public static void DisableMovementControl()
        {
#if UNITY_EDITOR
            if (CurrentControl == null)
            {
                Debug.LogError("[Control]: Control behavior isn't set!");

                return;
            }
#endif

            CurrentControl.DisableMovementControl();
        }
    }
}