#pragma warning disable 0649

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon
{
    [DefaultExecutionOrder(-999)]
    [HelpURL("https://docs.google.com/document/d/1ORNWkFMZ5_Cc-BUgu9Ds1DjMjR4ozMCyr6p_GGdyCZk")]
    public class Initialiser : MonoBehaviour
    {
        [SerializeField] ProjectInitSettings initSettings;
        [SerializeField] Canvas systemCanvas;
        [SerializeField] EventSystem eventSystem;

        public static Canvas SystemCanvas;
        public static GameObject InitialiserGameObject;

        public static bool IsInititalized { get; private set; }
        public static bool IsStartInitialized { get; private set; }
        public static ProjectInitSettings InitSettings { get; private set; }

        public void Awake()
        {
            if (!IsInititalized)
            {
                IsInititalized = true;

                InitSettings = initSettings;
                SystemCanvas = systemCanvas;
                InitialiserGameObject = gameObject;

#if MODULE_INPUT_SYSTEM
                eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
#endif

                DontDestroyOnLoad(gameObject);

                initSettings.Initialise(this);
            }
        }

        public void Start()
        {
            Initialise(true);
        }

        public void Initialise(bool loadingScene)
        {
            if (!IsStartInitialized)
            {
                IsStartInitialized = true;

                if (loadingScene)
                {
                    GameLoading.LoadGameScene();
                }
                else
                {
                    GameLoading.SimpleLoad();
                }
            }
        }

        public static bool IsModuleInitialised(Type moduleType)
        {
            ProjectInitSettings projectInitSettings = InitSettings;

            InitModule[] initModules = null;

#if UNITY_EDITOR
            if (!IsInititalized)
            {
                projectInitSettings = RuntimeEditorUtils.GetAssetByName<ProjectInitSettings>();
            }
#endif

            if (projectInitSettings != null)
            {
                initModules = projectInitSettings.Modules;
            }

            for (int i = 0; i < initModules.Length; i++)
            {
                if (initModules[i].GetType() == moduleType)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            IsInititalized = false;
        }
    }
}

// -----------------
// Initialiser v 0.4.4
// -----------------

// Changelog
// v 0.4.4
// • Added event system initialisation based on input module type
// v 0.4.3
// • Fixed editor adding core module bug
// v 0.4.2
// • Added loading scene logic
// v 0.4.1
// • Fixed error on module remove
// v 0.3.1
// • Added link to the documentation
// • Initializer renamed to Initialiser
// • Fixed problem with recompilation
// v 0.2
// • Added sorting feature
// • Initialiser MonoBehaviour will destroy after initialization
// v 0.1
// • Added basic version
