using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class GamepadIndicatorUI : MonoBehaviour
    {
        public static GamepadIndicatorUI Instance { get; private set; }

        [SerializeField] TMP_Text descriptionText;
        [SerializeField] Image rbImage;
        [SerializeField] Image lbImage;

        public List<IGamepadInteraction> interactions;

        public int SelectedInteractionId { get; private set; }
        public IGamepadInteraction SelectedInteraction => interactions[SelectedInteractionId];

        public void Init()
        {
            Instance = this;

            interactions = new List<IGamepadInteraction>();

            rbImage.enabled = false;
            lbImage.enabled = false;

            Control.OnInputChanged += OnInputChanged;
            gameObject.SetActive(false);
        }

        private void OnInputChanged(InputType inputType)
        {
            if(inputType == InputType.Gamepad)
            {
                gameObject.SetActive(interactions.Count > 0);
            } else
            {
                gameObject.SetActive(false);
            }        
        }

        public void AddGamepadInteractiopPoint(IGamepadInteraction interaction)
        {
            interactions.Add(interaction);

            RedrawUI();
        }

        public void RemoveGamepadInteractiopPoint(IGamepadInteraction interaction)
        {
            interactions.Remove(interaction);

            if(interactions.Count > 0 )
            {
                SelectedInteractionId = Mathf.Clamp(SelectedInteractionId, 0, interactions.Count - 1);
            } else
            {
                SelectedInteractionId = 0;
            }
            
            RedrawUI();
        }

        private void Update()
        {
            if (interactions.Count == 0) return;

            if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.Y))
            {
                SelectedInteraction.Interact();
            }

            if(interactions.Count > 1) 
            {
                if (SelectedInteractionId < interactions.Count - 1)
                {
                    if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.RB)){
                        SelectedInteractionId++;
                        RedrawUI();
                    }
                }

                if(SelectedInteractionId > 0)
                {
                    if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.LB))
                    {
                        SelectedInteractionId--;
                        RedrawUI();
                    }
                }
            }
        }

        private void RedrawUI()
        {
            if(this == null || gameObject == null) return;

            if(interactions.Count == 0 || Control.InputType != InputType.Gamepad)
            {
                gameObject.SetActive(false);
                return;
            }

            if(!gameObject.activeSelf) gameObject.SetActive(true);

            if(interactions.Count > 1)
            {
                rbImage.enabled = SelectedInteractionId < interactions.Count - 1;
                lbImage.enabled = SelectedInteractionId > 0;
            } else
            {
                rbImage.enabled = false;
                lbImage.enabled = false;
            }

            descriptionText.text = SelectedInteraction.Description;
        }
    }

    public interface IGamepadInteraction
    {
        string Description { get; }
        void Interact();
    }
}
