using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public abstract class WorldSpaceCanvas : MonoBehaviour, IDistanceToggle
    {
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected RectTransform background;

        [SerializeField] float canvasHideDistance = 6;

        public bool Visible { get => gameObject.activeSelf; set => gameObject.SetActive(value); }

        public bool DistanceToggleActivated { get; private set; }
        public bool IsDistanceToggleInCloseMode { get; private set; }

        public float ActivationDistanceOfDT => canvasHideDistance;
        public Vector3 OriginPositionOfDT => distanceToggleOriginPosition;

        private TweenCase canvasAppearCase;
        private Vector3 canvasDefaultScale;
        private Vector3 distanceToggleOriginPosition;

        protected virtual void Awake()
        {
            canvasDefaultScale = transform.localScale;
            DistanceToggleActivated = true;
            IsDistanceToggleInCloseMode = false;
            distanceToggleOriginPosition = transform.position;

            PlayerLeavedZone();

            // Add object to distance toggle
            DistanceToggle.AddObject(this);
        }

        public void Disable()
        {
            if (background != null)
                background.gameObject.SetActive(false);
        }

        public virtual void OnDestroy()
        {
            // remove object from distance toggle system
            DistanceToggle.RemoveObject(this);
        }

        public virtual void PlayerEnteredZone()
        {
            IsDistanceToggleInCloseMode = true;
            Visible = true;

            canvasAppearCase.KillActive();

            transform.localScale = canvasDefaultScale;
            canvasAppearCase = DistanceToggle.RunShowAnimation(transform);
        }

        public virtual void PlayerLeavedZone()
        {
            IsDistanceToggleInCloseMode = false;

            canvasAppearCase.KillActive();
            canvasAppearCase = DistanceToggle.RunHideAnimation(transform, () =>
            {
                Visible = false;
            });
        }

        public void OverrideOriginPositionOfDT(Vector3 newPosition)
        {
            distanceToggleOriginPosition = newPosition;
        }
    }
}
