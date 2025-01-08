using UnityEngine;
using UnityEditor;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace Watermelon
{
    public class SetupGuideWindow : EditorWindow
    {
        private static readonly Vector2 WINDOW_SIZE = new Vector2(490, 590);
        private static readonly string WINDOW_TITLE = "Setup Guide";

        private static SetupGuideWindow setupWindow;
        
        private Vector2 scrollView;

        private int currentTab = 0;
        private static TabContainer[] tabContainers;
        private static GUIContent[] tabs;

        [InitializeOnLoadMethod]
        private static void OnProjectLoaded()
        {
            SetupGuideInfo setupGuideInfo = EditorUtils.GetAsset<SetupGuideInfo>();

            //We use EditorApplication.timeSinceStartup to make sure to avoid  showing window every time script assembly gets reloaded
            if ((setupGuideInfo != null) && (EditorApplication.timeSinceStartup < 30)) 
            {
                EditorApplication.delayCall += ShowWindow;
            }
        }

        [MenuItem("Tools/Project Setup Guide")]
        [MenuItem("Window/Project Setup Guide")]
        static void ShowWindow()
        {
            SetupGuideWindow tempWindow = (SetupGuideWindow)GetWindow(typeof(SetupGuideWindow), false, WINDOW_TITLE);
            tempWindow.minSize = WINDOW_SIZE;
            tempWindow.titleContent = new GUIContent(WINDOW_TITLE, EditorCustomStyles.GetIcon("icon_title"));

            setupWindow = tempWindow;

            EditorApplication.delayCall -= ShowWindow;
        }

        protected void OnEnable()
        {
            setupWindow = this;

            // Tabs
            List<TabContainer> tabsList = new List<TabContainer>();

            List<Type> gameTypes = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly assembly in assemblies)
            {
                Type[] tempTypes = assembly.GetTypes().Where(m => m.IsDefined(typeof(SetupTabAttribute), true)).ToArray();
                if(!tempTypes.IsNullOrEmpty())
                    gameTypes.AddRange(tempTypes);
            }

            foreach (Type type in gameTypes)
            {
                //Get attribute
                SetupTabAttribute[] tabAttributes = (SetupTabAttribute[])Attribute.GetCustomAttributes(type, typeof(SetupTabAttribute));

                for (int i = 0; i < tabAttributes.Length; i++)
                {
                    UnityEngine.Object tabObject = EditorUtils.GetAsset(type);
                    if (tabObject != null)
                    {
                        tabsList.Add(new TabContainer(tabAttributes[i].tabName, tabAttributes[i].texture, tabObject, tabAttributes[i].showScrollView, tabAttributes[i].priority));
                    }
                }
            }
            
            tabContainers = tabsList.OrderBy(x => x.priority).ToArray();

            PrepareStyles();
        }

        protected void PrepareStyles()
        {
            titleContent = new GUIContent(WINDOW_TITLE, EditorCustomStyles.GetIcon("icon_title"));

            tabs = new GUIContent[tabContainers.Length];
            for (int i = 0; i < tabs.Length; i++)
            {
                Texture tabTexture = null;

                if (!string.IsNullOrEmpty(tabContainers[i].texture))
                {
                    tabTexture = EditorCustomStyles.GetIcon(tabContainers[i].texture);
                }

                if (!string.IsNullOrEmpty(tabContainers[i].name))
                {
                    tabs[i] = new GUIContent(tabTexture, tabContainers[i].name);
                }
                else
                {
                    tabs[i] = new GUIContent(tabTexture);
                }
            }
        }

        private void OnDisable()
        {
            for(int i = 0; i < tabContainers.Length; i++)
            {
                tabContainers[i].Destroy();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            int tempTab = GUILayout.Toolbar(currentTab, tabs, EditorCustomStyles.tab, GUILayout.Height(30));
            if (tempTab != currentTab)
            {
                currentTab = tempTab;
                
                scrollView = Vector2.zero;

                GUI.FocusControl(null);
            }

            if (tabContainers[currentTab].showScrollView)
                scrollView = EditorGUILayout.BeginScrollView(scrollView);

            EditorGUILayout.BeginVertical(EditorCustomStyles.windowSpacedContent);

            tabContainers[currentTab].DrawTab();

            EditorGUILayout.EndVertical();

            if (tabContainers[currentTab].showScrollView)
                EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }
        
        [InitializeOnLoadMethod]
        public static void SetupGuideStartup()
        {
            if (EditorPrefs.HasKey("ShowStartupGuide"))
                return;

            EditorApplication.delayCall += delegate
            {
                SetupGuideWindow.ShowWindow();
            };

            EditorPrefs.SetBool("ShowStartupGuide", true);

            EditorApplication.quitting += delegate
            {
                EditorPrefs.DeleteKey("ShowStartupGuide");
            };
        }

        public static void RepaintWindow()
        {
            if (setupWindow != null)
                setupWindow.Repaint();
        }

        private class TabContainer
        {
            public string name;
            public string texture;
            public int priority;
            public bool showScrollView = true;

            private Object tabObject;
            private Editor tabEditor;
            private DrawTabDelegate drawTabFunction;

            public TabContainer(string name, string texture, DrawTabDelegate drawTabFunction, bool showScrollView = true, int priority = int.MaxValue)
            {
                this.name = name;
                this.texture = texture;
                this.drawTabFunction = drawTabFunction;
                this.priority = priority;
                this.showScrollView = showScrollView;
            }

            public TabContainer(string name, string texture, Object tabObject, bool showScrollView = true, int priority = int.MaxValue)
            {
                this.name = name;
                this.texture = texture;
                this.tabObject = tabObject;
                this.priority = priority;
                this.showScrollView = showScrollView;

                Editor.CreateCachedEditor(tabObject, null, ref tabEditor);
            }

            public void DrawTab()
            {
                if(tabEditor)
                {
                    tabEditor.serializedObject.Update();
                    tabEditor.OnInspectorGUI();
                    tabEditor.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    if(drawTabFunction != null)
                    {
                        drawTabFunction.Invoke();
                    }
                }
            }

            public void Destroy()
            {
                if (tabEditor != null)
                    DestroyImmediate(tabEditor);
            }

            public delegate void DrawTabDelegate();
        }
    }
}


// -----------------
// Setup Guide v 1.0.2
// -----------------

// Changelog
// v 1.0.2
// • Added launch on start
// v 1.0
// • Added documentation link
// • Added basic version