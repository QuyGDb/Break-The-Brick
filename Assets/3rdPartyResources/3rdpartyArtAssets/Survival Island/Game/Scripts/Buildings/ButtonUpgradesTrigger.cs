using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public class ButtonUpgradesTrigger : UpgradesTrigger, IDistanceToggle, IGamepadInteraction
    {
        [SerializeField] WorldSpaceButton worldSpaceButtonRef;
        [SerializeField] Canvas canvasRef;
        [SerializeField] string gamepadDescription;

        public bool DistanceToggleActivated { get; private set; }
        public bool IsDistanceToggleInCloseMode { get; private set; }


        [SerializeField] float canvasHideDistance = 6f;
        public float ActivationDistanceOfDT => canvasHideDistance;

        private TweenCase canvasAppearCase;
        private Vector3 canvasDefaultScale;

        public Vector3 OriginPositionOfDT => transform.position;

        public string Description => gamepadDescription;

        protected override void Awake()
        {
            base.Awake();

            // subscribing on button click
            worldSpaceButtonRef.AddOnClickListener(ShowUpgradesPanel);
            canvasDefaultScale = canvasRef.transform.localScale;
        }

        public override void ShowUpgradesPanel()
        {
            if (IsLocalTrigger)
            {
                GlobalUpgradesController.OpenUpgradesPage(upgrades);
            }
            else
            {
                GlobalUpgradesController.OpenMainUpgradesPage();
            }

            UIGamepadButton.EnableTag(UIGamepadButtonTag.Upgrades);
            UIGamepadButton.DisableTag(UIGamepadButtonTag.Game);
        }

        protected void OnEnable()
        {
            // the next 2 fields responcible for canvas hiding and showing depending on player's position
            DistanceToggleActivated = true;
            IsDistanceToggleInCloseMode = false;
            canvasRef.enabled = false;

            // Add object to distance toggle
            DistanceToggle.AddObject(this);
        }

        protected void OnDisable()
        {
            // remove object from distance toggle system
            DistanceToggle.RemoveObject(this);
        }

        private void OnDestroy()
        {
            GamepadIndicatorUI.Instance.RemoveGamepadInteractiopPoint(this);
        }

        public void PlayerEnteredZone()
        {
            canvasRef.enabled = Control.InputType != InputType.Gamepad;
            IsDistanceToggleInCloseMode = true;

            canvasAppearCase.KillActive();

            canvasRef.transform.localScale = canvasDefaultScale;
            canvasAppearCase = DistanceToggle.RunShowAnimation(canvasRef.transform);

            GamepadIndicatorUI.Instance.AddGamepadInteractiopPoint(this);
        }

        public void PlayerLeavedZone()
        {
            IsDistanceToggleInCloseMode = false;

            canvasAppearCase.KillActive();
            canvasAppearCase = DistanceToggle.RunHideAnimation(canvasRef.transform, () =>
            {
                canvasRef.enabled = false;
            });

            GamepadIndicatorUI.Instance.RemoveGamepadInteractiopPoint(this);
        }

        [Button]
        public void Test()
        {
            Debug.Log("dist: " + DistanceToggleActivated);
        }

        public void Interact()
        {
            ShowUpgradesPanel();
        }
    }
}