using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if MODULE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Watermelon
{
    [RequireComponent(typeof(Button))]
    public class UIGamepadButton : MonoBehaviour
    {
        [SerializeField] GamepadButtonType buttonType;
        [SerializeField] UIGamepadButtonTag buttonTag;

        [Space]
        [SerializeField] UIGamepadButtonTag buttonsToDisable;
        [SerializeField] UIGamepadButtonTag buttonsToEnable;

        [Space]
        // Helps to manualy controll if the button is active even if it's tag is active
        [SerializeField] bool isInFocus = true;
        public bool IsInFocus { get => isInFocus; private set => isInFocus = value; }

        [Space]
        [SerializeField] Image gamepadButtonIcon;

        private Button button;

        private static UIGamepadButtonTag ActiveButtonTags { get; set; } = UIGamepadButtonTag.MainMenu;

        private void Awake()
        {
            button = GetComponent<Button>();

            //Initialising input for the first time. Later it will happen automatically, when the event is triggered
            OnInputChanged(Control.InputType);
            Control.OnInputChanged += OnInputChanged;

            button.onClick.AddListener(OnButtonClick);

            EventSystem.current.SetSelectedGameObject(null);
        }

        private void Update()
        {
            if (Control.InputType != InputType.Gamepad || (ActiveButtonTags & buttonTag) == 0 || !IsInFocus) return;

            if (GamepadControl.WasButtonPressedThisFrame(buttonType))
            {
                // Changing tags next frame to avoid a situation where a ui button with the same gamepad button and a newly enabled tag triggers on the same frame as this button.
                // Buttons have to still work with the game paused, thus uscaledTime is true
                Tween.NextFrame(() => {
                    DisableTag(buttonsToDisable);
                    EnableTag(buttonsToEnable);
                }, unscaledTime: true, framesOffset: 2);
                
                button.ClickButton();
            }
        }

        // Showing or hiding gamepad icon on a button if necessary
        private void OnInputChanged(InputType type)
        {
            if (Control.InputType == InputType.Gamepad && IsInFocus)
            {
                gamepadButtonIcon.enabled = true;

                if (Control.GamepadData != null) gamepadButtonIcon.sprite = Control.GamepadData.GetButtonIcon(buttonType);
            }
            else
            {
                gamepadButtonIcon.enabled = false;
            }
        }

        // We still need to keep track of the active buttons in order to be able to swap the control from gamepad to keyboard and vice versa
        private void OnButtonClick()
        {
            if(Control.InputType != InputType.Gamepad)
            {
                DisableTag(buttonsToDisable);
                EnableTag(buttonsToEnable);
            }
        }

        //Gives the ability to control the button visibility even if it's tag is active. Useful for lists, scroll views, etc.
        public void SetFocus(bool focus)
        {
            IsInFocus = focus;

            gamepadButtonIcon.enabled = focus && Control.InputType == InputType.Gamepad;
        }

        public static void EnableTag(UIGamepadButtonTag tagToEnable)
        {
            // Binary operation to add values to the flag
            ActiveButtonTags |= tagToEnable;
        }

        public static void DisableTag(UIGamepadButtonTag tagToDisable)
        {
            // Binary operation to remove values from the flag
            ActiveButtonTags &= ~tagToDisable;
        }

        public static void DisableAllTags()
        {
            ActiveButtonTags &= 0;
        }

#region Highlight

        private TweenCase highlightScaleCase;
        private TweenCase returnCase;
        private bool isHighlightActive;

        public void StartHighlight()
        {
            if (isHighlightActive) return;
            isHighlightActive = true;

            // Killing return case just to be sure
            returnCase.KillActive();

            PingPongAnimation();
        }

        public void StopHighLight()
        {
            if (!isHighlightActive) return;
            isHighlightActive = false;

            // Definitely should kill highlight case
            highlightScaleCase.KillActive();

            returnCase = gamepadButtonIcon.DOScale(1f, 0.2f).SetEasing(Ease.Type.SineOut);
        }

        private void PingPongAnimation()
        {
            // Shouldn trigger stack overflown because of how tween works
            highlightScaleCase = gamepadButtonIcon.DOPingPongScale(0.9f, 1.2f, 1, Ease.Type.SineInOut, Ease.Type.SineInOut).OnComplete(PingPongAnimation);
        }

#endregion

#region Editor Tool

#if UNITY_EDITOR
        [Button]
        public void ShowGamepadButtonsInEditor()
        {
            if (Application.isPlaying) return;

            // Enabling gamepad icon images in every UIGamepadButton on the scene.
            // WILL NOT ENABLE THEM INSIDE PREFABS!
            FindObjectsByType<UIGamepadButton>(FindObjectsInactive.Include, FindObjectsSortMode.None).ForEach(button => button.gamepadButtonIcon.enabled = true);
        }

        [Button]
        public void HideGamepadButtonsInEditor()
        {
            if (Application.isPlaying) return;

            // Disabling gamepad icon images in every UIGamepadButton on the scene.
            // WILL NOT DISABLE THEM INSIDE PREFABS!
            FindObjectsByType<UIGamepadButton>(FindObjectsInactive.Include, FindObjectsSortMode.None).ForEach(button => button.gamepadButtonIcon.enabled = false);
        }

        [Button]
        public void PrintEnabledTags()
        {
            Debug.Log(ActiveButtonTags);
        }
#endif

#endregion
    }
}