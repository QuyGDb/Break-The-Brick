using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class DistanceToggle
    {
        private static List<IDistanceToggle> distanceToggles = new List<IDistanceToggle>();

        private static bool isActive;
        public static bool IsActive => isActive;

        private static Vector3 tempDistance;
        private static float tempDistanceMagnitude;
        private static bool tempIsVisible;

        private static Transform playerTransform;

        private static Coroutine updateCoroutine;

        public static void Initialise(Transform transform)
        {
            playerTransform = transform;

            isActive = true;

            // Activate update coroutine
            updateCoroutine = Tween.InvokeCoroutine(UpdateCoroutine());
        }

        private static IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                if (isActive)
                {
                    for (int i = 0; i < distanceToggles.Count; i++)
                    {
                        if (!distanceToggles[i].DistanceToggleActivated)
                            continue;

                        tempIsVisible = distanceToggles[i].IsDistanceToggleInCloseMode;

                        tempDistance = playerTransform.position - distanceToggles[i].OriginPositionOfDT;
                        tempDistance.y = 0;

                        tempDistanceMagnitude = tempDistance.magnitude;

                        if (!tempIsVisible && tempDistanceMagnitude <= distanceToggles[i].ActivationDistanceOfDT)
                        {
                            distanceToggles[i].PlayerEnteredZone();
                        }
                        else if (tempIsVisible && tempDistanceMagnitude > distanceToggles[i].ActivationDistanceOfDT)
                        {
                            distanceToggles[i].PlayerLeavedZone();
                        }
                    }
                }

                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
            }
        }

        public static void AddObject(IDistanceToggle distanceToggle)
        {
            distanceToggles.Add(distanceToggle);
        }

        public static void RemoveObject(IDistanceToggle distanceToggle)
        {
            distanceToggles.Remove(distanceToggle);
        }

        public static bool IsInRange(IDistanceToggle distanceToggle)
        {
            tempDistance = playerTransform.position - distanceToggle.OriginPositionOfDT;
            tempDistance.y = 0;

            tempDistanceMagnitude = tempDistance.magnitude;

            return tempDistanceMagnitude <= distanceToggle.ActivationDistanceOfDT;
        }

        public static void Enable()
        {
            isActive = true;
        }

        public static void Disable()
        {
            isActive = false;
        }

        public static void Unload()
        {
            if (updateCoroutine != null)
                Tween.StopCustomCoroutine(updateCoroutine);

            distanceToggles.Clear();

            isActive = false;
        }

        public static TweenCase RunShowAnimation(Transform panelTransform, System.Action OnComplete = null)
        {
            Vector3 defaultScale = panelTransform.localScale;

            panelTransform.localScale = Vector3.zero;
            return panelTransform.DOScale(defaultScale, 0.2f).SetEasing(Ease.Type.BackOut).OnComplete(() =>
             {
                 OnComplete?.Invoke();
             });
        }

        public static TweenCase RunHideAnimation(Transform panelTransform, System.Action OnComplete = null)
        {
            Vector3 defaultScale = panelTransform.localScale;

            return panelTransform.DOScale(0f, 0.2f).SetEasing(Ease.Type.BackIn).OnComplete(() =>
            {
                panelTransform.localScale = defaultScale;
                OnComplete?.Invoke();
            });
        }
    }
}