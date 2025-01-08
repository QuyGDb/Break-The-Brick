using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Control Manager")]
    public class ControlInitModule : InitModule
    {
        public override string ModuleName => "Control Manager";

        [SerializeField] bool selectAutomatically = true;

        [HideIf("selectAutomatically")]
        [SerializeField] InputType inputType;

        [HideIf("IsJoystickCondition")]
        [SerializeField] GamepadData gamepadData;

        public override void CreateComponent(GameObject holderObject)
        {
            if (selectAutomatically)
                inputType = ControlUtils.GetCurrentInputType();

            Control.Initialise(inputType, gamepadData);

            if(inputType == InputType.Keyboard)
            {
                KeyboardControl keyboardControl = holderObject.AddComponent<KeyboardControl>();
                keyboardControl.Initialise();
            } 
            else if(inputType == InputType.Gamepad)
            {
                GamepadControl gamepadControl = holderObject.AddComponent<GamepadControl>();
                gamepadControl.Initialise();
            }
        }

        private bool IsJoystickCondition()
        {
            return selectAutomatically ? ControlUtils.GetCurrentInputType() == InputType.UIJoystick : inputType == InputType.UIJoystick;
        }
    }
}