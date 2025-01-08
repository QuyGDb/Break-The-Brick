using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIController : MonoBehaviour
    {
        private static UIController uiController;

        [SerializeField] FloatingCloud currencyCloud;
        [SerializeField] NotchSaveArea notchSaveArea;

        private static List<UIPage> pages;
        private static Dictionary<Type, UIPage> pagesLink = new Dictionary<Type, UIPage>();

        private static bool isTablet;
        public static bool IsTablet => isTablet;

        private static Canvas mainCanvas;
        public static Canvas MainCanvas => mainCanvas;
        public static CanvasScaler CanvasScaler { get; private set; }

        private static Camera mainCamera;

        private static SimpleCallback localPageClosedCallback;

        public static event PageCallback OnPageOpenedEvent;
        public static event PageCallback OnPageClosedEvent;

        public static event PopupWindowCallback OnPoupWindowStateChanged;

        public void Initialise()
        {
            uiController = this;

            mainCanvas = GetComponent<Canvas>();
            CanvasScaler = GetComponent<CanvasScaler>();

            isTablet = UIUtils.IsWideScreen(Camera.main);
            mainCamera = Camera.main;

            CanvasScaler.matchWidthOrHeight = isTablet ? 1 : 0;

            pages = new List<UIPage>();
            pagesLink = new Dictionary<Type, UIPage>();
            for (int i = 0; i < transform.childCount; i++)
            {
                UIPage uiPage = transform.GetChild(i).GetComponent<UIPage>();
                if(uiPage != null)
                {
                    uiPage.CacheComponents();

                    pagesLink.Add(uiPage.GetType(), uiPage);

                    pages.Add(uiPage);
                }
            }

            // Initialise global overlay
            Overlay.Initialise(this);
        }

        public void InitialisePages()
        {
            // Refresh notch save area
            notchSaveArea.Initialise();

            // Initialise currency cloud
            currencyCloud.Initialise();

            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].Initialise();
                pages[i].DisableCanvas();
            }
        }

        public static void ResetPages()
        {
            UIController controller = uiController;
            if (controller != null)
            {
                for (int i = 0; i < pages.Count; i++)
                {
                    if (pages[i].IsPageDisplayed)
                    {
                        pages[i].Unload();
                    }
                }
            }
        }

        public static void ShowPage<T>() where T : UIPage
        {
            Type pageType = typeof(T);
            UIPage page = pagesLink[pageType];
            if (!page.IsPageDisplayed)
            {
                page.PlayShowAnimation();
                page.EnableCanvas();
                page.GraphicRaycaster.enabled = true;
            }
        }

        public static void ShowPage(UIPage page)
        {
            if (!page.IsPageDisplayed)
            {
                page.PlayShowAnimation();
                page.EnableCanvas();
                page.GraphicRaycaster.enabled = true;
            }
        }

        public static void HidePage<T>(SimpleCallback onPageClosed = null)
        {
            Type pageType = typeof(T);
            UIPage page = pagesLink[pageType];
            if (page.IsPageDisplayed)
            {
                localPageClosedCallback = onPageClosed;

                page.GraphicRaycaster.enabled = false;
                page.PlayHideAnimation();
            }
            else
            {
                onPageClosed?.Invoke();
            }
        }

        public static void OnPageClosed(UIPage page)
        {
            page.DisableCanvas();

            OnPageClosedEvent?.Invoke(page, page.GetType());

            if (localPageClosedCallback != null)
            {
                localPageClosedCallback.Invoke();
                localPageClosedCallback = null;
            }
        }

        public static void OnPageOpened(UIPage page)
        {
            OnPageOpenedEvent?.Invoke(page, page.GetType());
        }

        public static T GetPage<T>() where T : UIPage
        {
            return pagesLink[typeof(T)] as T;
        }

        public static void OnPopupWindowOpened(IPopupWindow popupWindow)
        {
            OnPoupWindowStateChanged?.Invoke(popupWindow, true);
        }

        public static void OnPopupWindowClosed(IPopupWindow popupWindow)
        {
            OnPoupWindowStateChanged?.Invoke(popupWindow, false);
        }

        public static Vector3 FixUIElementToWorld(Transform target, Vector3 offset)
        {
            Vector3 targPos = target.transform.position + offset;
            Vector3 camForward = mainCamera.transform.forward;

            float distInFrontOfCamera = Vector3.Dot(targPos - (mainCamera.transform.position + camForward), camForward);
            if (distInFrontOfCamera < 0f)
            {
                targPos -= camForward * distInFrontOfCamera;
            }

            return RectTransformUtility.WorldToScreenPoint(mainCamera, targPos);
        }

        private void OnDestroy()
        {
            FloatingCloud.Clear();

            Overlay.Clear();
        }

        public delegate void PageCallback(UIPage page, Type pageType);
        public delegate void PopupWindowCallback(IPopupWindow popupWindow, bool state);
    }
}

// -----------------
// UI Controller v1.2.1
// -----------------

// Changelog
// v 1.2.1
// • Added Editor script that automatically configure CanvasScaler
// v 1.2
// • Added global overlay
// v 1.1
// • Added popup callbacks and methods to handle when a custom window is opened
// • RectTransform can be added to NotchSaveArea using NotchSaveArea.RegisterRectTransform method
// v 1.0
// • Basic logic