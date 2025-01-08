using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class GroundTileBehavior : MonoBehaviour, IUnlockable
    {
        public string ID { get; private set; }

        [Space]
        [SerializeField] GameObject openedVisuals;
        public GameObject OpenedVisuals => openedVisuals;
        [SerializeField] GameObject closedVisuals;
        public GameObject ClosedVisuals => closedVisuals;

        [Space(5)]
        [SerializeField] protected AnimationForUnlockable unlockAnimation;
        [SerializeField] protected AudioClip unlockSound;

        private List<IGroundOpenable> openables = new List<IGroundOpenable>();

        public bool IsInitialised { get; private set; }
        public bool IsOpen { get; private set; }

        public void RegisterOpenable(IGroundOpenable openable)
        {
            if (!openables.Contains(openable))
            {
                openables.Add(openable);

                if (IsInitialised)
                {
                    if (IsOpen)
                    {
                        openable.OnGroundOpen(true);
                    }
                    else
                    {
                        openable.OnGroundHidden(true);
                    }
                }
            }
        }

        public void SpawnUnlocked()
        {
            IsInitialised = true;
            IsOpen = true;

            if (openedVisuals != null)
                openedVisuals.SetActive(true);

            if (closedVisuals != null)
                closedVisuals.SetActive(false);

            foreach (var openable in openables)
            {
                openable.OnGroundOpen(true);
            }
        }

        public void SpanwNotUnlocked()
        {
            IsInitialised = true;
            IsOpen = false;

            if (openedVisuals != null)
                openedVisuals.SetActive(false);

            if (closedVisuals != null)
                closedVisuals.SetActive(true);

            foreach (var openable in openables)
            {
                openable.OnGroundHidden(true);
            }
        }

        public void FullyUnlock()
        {
            if (unlockAnimation != null)
                unlockAnimation.RunUnlockedAnimation();

            // waiting for animation to complete before enabling navmesh
            Tween.DelayedCall(unlockAnimation != null ? unlockAnimation.TotalAnimationDuration : 0f, () =>
            {
                if (openedVisuals != null)
                    openedVisuals.SetActive(true);

                if (closedVisuals != null)
                    closedVisuals.SetActive(false);

                IsOpen = true;

                foreach (var openable in openables)
                {
                    openable.OnGroundOpen(false);
                }

                if (unlockSound != null)
                    AudioController.PlaySound(unlockSound);

                NavMeshController.CalculateNavMesh();
            });
        }

        public void SetID(string id)
        {
            ID = id;
        }

        #region Development

#if UNITY_EDITOR
        private Color gizmoDefaultColorDev;
        private Vector3 tileSize;

        private void OnValidate()
        {
            gizmoDefaultColorDev = Gizmos.color;
            tileSize = new Vector3(5f * transform.parent.localScale.x, 0f, 5f * transform.parent.localScale.z);
        }

        private void OnDrawGizmos()
        {
            float minHeight = ControllOverlayTilesDimensionSettings.MinValue;
            float maxHeight = ControllOverlayTilesDimensionSettings.MaxValue;

            if (transform.position.y < minHeight || transform.position.y > maxHeight)
                return;

            float heightCoef = Mathf.Clamp01((transform.position.y - minHeight) / (maxHeight - minHeight));
            Gizmos.color = Color.Lerp(Color.blue, Color.red, heightCoef);

            List<Vector3> points = new List<Vector3>();

            points.Add(transform.position.AddToX(tileSize.x * -0.5f).AddToZ(tileSize.z * -0.5f));
            points.Add(transform.position.AddToX(tileSize.x * 0.5f).AddToZ(tileSize.z * -0.5f));

            points.Add(transform.position.AddToX(tileSize.x * 0.5f).AddToZ(tileSize.z * -0.5f));
            points.Add(transform.position.AddToX(tileSize.x * 0.5f).AddToZ(tileSize.z * 0.5f));

            points.Add(transform.position.AddToX(tileSize.x * 0.5f).AddToZ(tileSize.z * 0.5f));
            points.Add(transform.position.AddToX(tileSize.x * -0.5f).AddToZ(tileSize.z * 0.5f));

            points.Add(transform.position.AddToX(tileSize.x * -0.5f).AddToZ(tileSize.z * 0.5f));
            points.Add(transform.position.AddToX(tileSize.x * -0.5f).AddToZ(tileSize.z * -0.5f));

            Gizmos.DrawLineList(points.ToArray());

            Gizmos.color = gizmoDefaultColorDev;
        }


#endif

        #endregion
    }
}