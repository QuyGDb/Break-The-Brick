using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon
{
    public abstract class PauseItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image outlineImage;

        public bool IsSelected { get; private set; }

        protected virtual void Awake()
        {
            outlineImage.enabled = false;
        }

        public virtual void Select()
        {
            IsSelected = true;

            if (Control.InputType == InputType.Gamepad)
            {
                outlineImage.enabled = true;
            }

            Control.OnInputChanged += OnControlChanged;
        }

        public virtual void Deselect()
        {
            IsSelected = false;

            outlineImage.enabled = false;

            Control.OnInputChanged -= OnControlChanged;
        }

        protected virtual void OnControlChanged(InputType type)
        {
            outlineImage.enabled = type == InputType.Gamepad && IsSelected;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Click();
        }

        protected abstract void Click();

        protected virtual void Update()
        {
            if (IsSelected && Control.InputType == InputType.Gamepad)
            {
                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.A))
                {
                    Click();
                }
            }
        }
    }
}
