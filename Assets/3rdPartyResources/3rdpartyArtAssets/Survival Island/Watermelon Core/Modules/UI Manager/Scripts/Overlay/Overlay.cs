using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public static class Overlay
    {
        private static IOverlayPanel overlayPanel;

        public static void Initialise(UIController uiController)
        {
            foreach (Transform child in uiController.transform)
            {
                Component component = child.GetComponent(typeof(IOverlayPanel));

                if (component != null)
                {
                    overlayPanel = (IOverlayPanel)component;

                    break;
                }
            }

            if(overlayPanel == null)
            {
                // Create a custom canvas
                GameObject canvasObject = new GameObject("[TEMP OVERLAY]");
                canvasObject.transform.SetParent(uiController.transform);
                canvasObject.transform.ResetLocal();
                canvasObject.layer = LayerMask.NameToLayer("UI");

                RectTransform canvasRectTransform = canvasObject.AddComponent<RectTransform>();
                canvasRectTransform.anchorMin = new Vector2(0, 0);
                canvasRectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                canvasRectTransform.sizeDelta = Vector2.zero;

                Canvas overlayCanvas = canvasObject.AddComponent<Canvas>();
                overlayCanvas.overrideSorting = true;
                overlayCanvas.sortingOrder = 999;

                canvasObject.AddComponent<GraphicRaycaster>();

                DummyOverlayPanel dummyOverlayPanel = new DummyOverlayPanel();
                dummyOverlayPanel.SetCanvas(overlayCanvas);

                overlayPanel = dummyOverlayPanel;
            }

            overlayPanel.Initialise();
            overlayPanel.SetState(false);
            overlayPanel.SetLoadingState(false);
        }

        public static void Show(float duration, SimpleCallback onCompleted, bool showLoadingAnimation = false)
        {
            overlayPanel.SetState(true);
            overlayPanel.Show(duration, onCompleted);

            if(showLoadingAnimation)
                overlayPanel.SetLoadingState(true);
        }

        public static void Hide(float duration, SimpleCallback onCompleted = null)
        {
            overlayPanel.Hide(duration, () =>
            {
                overlayPanel.SetState(false);
                overlayPanel.SetLoadingState(false);

                onCompleted?.Invoke();
            });
        }

        public static void Clear()
        {
            if(overlayPanel != null)
            {
                overlayPanel.Clear();
            }
        }
    }
}
