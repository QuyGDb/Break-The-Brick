using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Watermelon
{
    [CustomEditor(typeof(ProjectInitSettings))]
    public class ProjectInitSettingsEditor : Editor
    {
        private const string MODULES_PROPERTY_NAME = "modules";

        private const string DEFAULT_PROJECT_INIT_SETTINGS_PATH = "Assets/Project Data/Content/Settings/Project Init Settings.asset";

        private SerializedProperty modulesProperty;

        private List<InitModuleContainer> initModulesEditors;

        private GUIContent arrowDownContent;
        private GUIContent arrowUpContent;

        private GUIStyle arrowButtonStyle;

        private ProjectInitSettings projectInitSettings;
        private GenericMenu modulesGenericMenu;

        private static InitModulesHandler modulesHandler;

        [MenuItem("Tools/Editor/Project Init Settings")]
        public static void SelectProjectInitSettings()
        {
            UnityEngine.Object selectedObject = AssetDatabase.LoadAssetAtPath(DEFAULT_PROJECT_INIT_SETTINGS_PATH, typeof(ProjectInitSettings));

            if(selectedObject == null)
            {
                selectedObject = EditorUtils.GetAsset<ProjectInitSettings>();

                if(selectedObject == null)
                {
                    Debug.LogError("Asset with type \"ProjectInitSettings\" don`t exist.");
                }
                else
                {
                    Selection.activeObject = selectedObject;
                    Debug.LogWarning($"Asset with type \"ProjectInitSettings\" is misplaced. Expected path: {DEFAULT_PROJECT_INIT_SETTINGS_PATH} .Actual path: {AssetDatabase.GetAssetPath(selectedObject)}");

                }
            }
            else
            {
                Selection.activeObject = selectedObject;
            }
        }

        protected void OnEnable()
        {
            projectInitSettings = (ProjectInitSettings)target;

            modulesHandler = new InitModulesHandler();

            modulesProperty = serializedObject.FindProperty(MODULES_PROPERTY_NAME);

            InitGenericMenu();
            InitCoreModules(projectInitSettings.Modules);

            LoadEditorsList();

            arrowDownContent = EditorCustomStyles.foldoutArrowDown;
            arrowUpContent = EditorCustomStyles.foldoutArrowUp;

            arrowButtonStyle = new GUIStyle(EditorCustomStyles.padding00);

            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private void InitCoreModules(InitModule[] coreModules)
        {
            List<Type> requiredModules = GetRequiredModules(coreModules);
            if(requiredModules.Count > 0)
            {
                foreach(var type in requiredModules)
                {
                    AddModule(type);
                }

                LoadEditorsList();

                EditorUtility.SetDirty(target);

                AssetDatabase.SaveAssets();
            }
        }

        private void InitGenericMenu()
        {
            modulesGenericMenu = new GenericMenu();

            //Load all modules
            InitModule[] initModules = projectInitSettings.Modules;

            IEnumerable<Type> registeredTypes = GetRegisteredAttributes();
            foreach (Type type in registeredTypes)
            {
                RegisterModuleAttribute defineAttribute = (RegisterModuleAttribute)Attribute.GetCustomAttribute(type, typeof(RegisterModuleAttribute));
                if(defineAttribute != null)
                {
                    if (!defineAttribute.Core)
                    {
                        bool isAlreadyActive = initModules != null && initModules.Any(x => x != null && x.GetType() == type);
                        if (isAlreadyActive)
                        {
                            modulesGenericMenu.AddDisabledItem(new GUIContent("Add Module/" + defineAttribute.Path), false);
                        }
                        else
                        {
                            modulesGenericMenu.AddItem(new GUIContent("Add Module/" + defineAttribute.Path), false, delegate
                            {
                                AddModule(type);

                                InitGenericMenu();
                            });
                        }
                    }
                }
            }
        }

        private IEnumerable<Type> GetRegisteredAttributes()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                IEnumerable<Type> types = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(InitModule)));
                foreach (Type type in types)
                {
                    yield return type;
                }
            }
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= LogPlayModeState;
        }

        private void LogPlayModeState(PlayModeStateChange obj)
        {
            if (Selection.activeObject == target)
                Selection.activeObject = null;
        }

        private void LoadEditorsList()
        {
            ClearInitModules();
            SerializedProperty initModule;
            SerializedObject initModuleSerializedObject;

            for (int i = 0; i < modulesProperty.arraySize; i++)
            {
                initModule = modulesProperty.GetArrayElementAtIndex(i);

                if(initModule.objectReferenceValue != null)
                {
                    initModuleSerializedObject = new SerializedObject(initModule.objectReferenceValue);

                    initModulesEditors.Add(new InitModuleContainer(initModule.objectReferenceValue.GetType(), initModuleSerializedObject, Editor.CreateEditor(initModuleSerializedObject.targetObject), modulesHandler.IsCoreModule(initModule.objectReferenceValue.GetType())));
                }
            }
        }

        private InitModuleContainer GetEditor(Type type)
        {
            for(int i = 0; i < initModulesEditors.Count; i++)
            {
                if (initModulesEditors[i].Type == type)
                    return initModulesEditors[i];
            }

            return null;
        }

        private void OnDestroy()
        {
            ClearInitModules();
        }

        private void ClearInitModules()
        {
            if (initModulesEditors != null)
            {
                // Destroy old editors
                for (int i = 0; i < initModulesEditors.Count; i++)
                {
                    if (initModulesEditors[i] != null && initModulesEditors[i].Editor != null)
                    {
                        DestroyImmediate(initModulesEditors[i].Editor);
                    }
                }

                initModulesEditors.Clear();
            }
            else
            {
                initModulesEditors = new List<InitModuleContainer>();
            }
        }

        private void DrawModules(SerializedProperty arrayProperty)
        {
            if (arrayProperty.arraySize > 0)
            {
                for (int i = 0; i < arrayProperty.arraySize; i++)
                {
                    SerializedProperty initModuleProperty = arrayProperty.GetArrayElementAtIndex(i);

                    if (initModuleProperty.objectReferenceValue != null)
                    {
                        InitModule initModule = (InitModule)initModuleProperty.objectReferenceValue;

                        SerializedObject moduleSeializedObject = new SerializedObject(initModuleProperty.objectReferenceValue);

                        moduleSeializedObject.Update();

                        Rect moduleRect;

                        initModuleProperty.isExpanded = EditorGUILayoutCustom.BeginFoldoutBoxGroup(initModuleProperty.isExpanded, initModule.ModuleName, out moduleRect);

                        if (initModuleProperty.isExpanded)
                        {
                            InitModuleContainer moduleContainer = GetEditor(initModuleProperty.objectReferenceValue.GetType());
                            if (moduleContainer == null) continue;

                            moduleContainer.OnInspectorGUI();

                            GUILayout.Space(10);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

                            moduleContainer.DrawButtons();

                            if(!moduleContainer.IsCore)
                            {
                                if (GUILayout.Button("Remove", GUILayout.Width(90)))
                                {
                                    if (EditorUtility.DisplayDialog("This object will be removed!", "Are you sure?", "Remove", "Cancel"))
                                    {
                                        UnityEngine.Object removedObject = initModuleProperty.objectReferenceValue;
                                        initModuleProperty.isExpanded = false;
                                        arrayProperty.RemoveFromVariableArrayAt(i);

                                        LoadEditorsList();
                                        AssetDatabase.RemoveObjectFromAsset(removedObject);

                                        DestroyImmediate(removedObject, true);

                                        EditorUtility.SetDirty(target);

                                        AssetDatabase.SaveAssets();

                                        return;
                                    }
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayoutCustom.EndFoldoutBoxGroup();

                        if (GUI.Button(new Rect(moduleRect.x + moduleRect.width - 15, moduleRect.y, 10, 10), arrowUpContent, arrowButtonStyle))
                        {
                            if (i > 0)
                            {
                                bool expandState = arrayProperty.GetArrayElementAtIndex(i - 1).isExpanded;

                                arrayProperty.MoveArrayElement(i, i - 1);

                                arrayProperty.GetArrayElementAtIndex(i - 1).isExpanded = initModuleProperty.isExpanded;
                                arrayProperty.GetArrayElementAtIndex(i).isExpanded = expandState;
                                serializedObject.ApplyModifiedProperties();

                                GUIUtility.ExitGUI();
                                Event.current.Use();

                                return;
                            }
                        }
                        if (GUI.Button(new Rect(moduleRect.x + moduleRect.width - 15, moduleRect.y + 6, 10, 10), arrowDownContent, arrowButtonStyle))
                        {
                            if (i + 1 < arrayProperty.arraySize)
                            {
                                bool expandState = arrayProperty.GetArrayElementAtIndex(i + 1).isExpanded;

                                arrayProperty.MoveArrayElement(i, i + 1);

                                arrayProperty.GetArrayElementAtIndex(i + 1).isExpanded = initModuleProperty.isExpanded;
                                arrayProperty.GetArrayElementAtIndex(i).isExpanded = expandState;

                                serializedObject.ApplyModifiedProperties();

                                GUIUtility.ExitGUI();
                                Event.current.Use();

                                return;
                            }
                        }

                        if (GUI.Button(moduleRect, GUIContent.none, GUIStyle.none))
                        {
                            initModuleProperty.isExpanded = !initModuleProperty.isExpanded;
                        }

                        moduleSeializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal(EditorCustomStyles.box);
                        EditorGUILayout.BeginHorizontal(EditorCustomStyles.padding00);
                        EditorGUILayout.LabelField(EditorGUIUtility.IconContent("console.warnicon"), EditorCustomStyles.padding00, GUILayout.Width(16), GUILayout.Height(16));
                        EditorGUILayout.LabelField("Object referenct is null");
                        if (GUILayout.Button("Remove", EditorStyles.miniButton))
                        {
                            arrayProperty.RemoveFromVariableArrayAt(i);

                            InitGenericMenu();

                            GUIUtility.ExitGUI();
                            Event.current.Use();

                            return;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Modules list is empty!", MessageType.Info);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayoutCustom.BeginMenuBoxGroup("Modules", modulesGenericMenu);

            DrawModules(modulesProperty);

            EditorGUILayoutCustom.EndBoxGroup();

            GUILayout.FlexibleSpace();
        }

        public void AddModule(Type moduleType)
        {
            if(!moduleType.IsSubclassOf(typeof(InitModule)))
            {
                Debug.LogError("[Initialiser]: Module type should be subclass of InitModule class!");

                return;
            }

            Undo.RecordObject(target, "Add module");

            modulesProperty = serializedObject.FindProperty(MODULES_PROPERTY_NAME);

            serializedObject.Update();

            modulesProperty.arraySize++;

            InitModule initModule = (InitModule)ScriptableObject.CreateInstance(moduleType);
            initModule.name = moduleType.ToString();
            initModule.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(initModule, target);

            modulesProperty.GetArrayElementAtIndex(modulesProperty.arraySize - 1).objectReferenceValue = initModule;

            serializedObject.ApplyModifiedProperties();
            LoadEditorsList();

            EditorUtility.SetDirty(target);

            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/Create/Settings/Project Init Settings")]
        public static void CreateAsset()
        {
            ProjectInitSettings projectInitSettings = EditorUtils.GetAsset<ProjectInitSettings>();
            if (projectInitSettings)
            {
                Debug.Log("Project Init Settings file is already exits!");

                EditorGUIUtility.PingObject(projectInitSettings);

                return;
            }

            projectInitSettings = EditorUtils.CreateScriptableObject<ProjectInitSettings>("Project Init Settings");

            SerializedObject serializedObject = new SerializedObject(projectInitSettings);
            serializedObject.Update();

            SerializedProperty coreModulesProperty = serializedObject.FindProperty(MODULES_PROPERTY_NAME);

            List<Type> requiredModules = GetRequiredModules(null);
            foreach(Type type in requiredModules)
            {
                // Create init module
                InitModule initModule = (InitModule)ScriptableObject.CreateInstance(type);
                initModule.name = type.ToString();
                initModule.hideFlags = HideFlags.HideInHierarchy;

                AssetDatabase.AddObjectToAsset(initModule, projectInitSettings);

                coreModulesProperty.arraySize++;
                coreModulesProperty.GetArrayElementAtIndex(coreModulesProperty.arraySize - 1).objectReferenceValue = initModule;
            }

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(projectInitSettings);

            AssetDatabase.SaveAssets();

            EditorGUIUtility.PingObject(projectInitSettings);
        }

        private static List<Type> GetRequiredModules(InitModule[] coreModules)
        {
            // Get all registered init modules
            IEnumerable<Type> registeredTypes = Assembly.GetAssembly(typeof(InitModule)).GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(InitModule)));

            List<Type> requiredModules = new List<Type>();
            foreach (Type type in registeredTypes)
            {
                RegisterModuleAttribute[] defineAttributes = (RegisterModuleAttribute[])Attribute.GetCustomAttributes(type, typeof(RegisterModuleAttribute));
                for (int m = 0; m < defineAttributes.Length; m++)
                {
                    if (defineAttributes[m].Core)
                    {
                        bool isExists = coreModules != null && coreModules.Any(x => x != null && x.GetType() == type);
                        if (!isExists)
                        {
                            requiredModules.Add(type);
                        }
                    }
                }
            }

            return requiredModules;
        }

        private class InitModulesHandler
        {
            private IEnumerable<ModuleData> modulesData;

            public InitModulesHandler()
            {
                modulesData = GetModulesData();
            }

            private IEnumerable<ModuleData> GetModulesData()
            {
                IEnumerable<Type> registeredTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(InitModule)));

                foreach (Type type in registeredTypes)
                {
                    RegisterModuleAttribute defineAttribute = (RegisterModuleAttribute)Attribute.GetCustomAttribute(type, typeof(RegisterModuleAttribute));
                    if (defineAttribute != null)
                    {
                        yield return new ModuleData()
                        {
                            ClassType = type,
                            Attribute = defineAttribute
                        };
                    }
                }
            }

            public bool IsCoreModule(Type type)
            {
                foreach (var data in modulesData)
                {
                    if (type == data.ClassType && data.Attribute.Core)
                        return true;
                }

                return false;
            }

            public class ModuleData
            {
                public Type ClassType;
                public RegisterModuleAttribute Attribute;
            }
        }

        private class InitModuleContainer
        {
            public Type Type;
            public SerializedObject SerializedObject;
            public Editor Editor;

            private bool isModuleInitEditor;
            private InitModuleEditor initModuleEditor;

            public bool IsCore;

            public InitModuleContainer(Type type, SerializedObject serializedObject, Editor editor, bool isCore)
            {
                Type = type;
                SerializedObject = serializedObject;
                Editor = editor;
                IsCore = isCore;

                initModuleEditor = editor as InitModuleEditor;
                isModuleInitEditor = initModuleEditor != null;
            }

            public void OnInspectorGUI()
            {
                if(Editor != null)
                {
                    Editor.OnInspectorGUI();
                }
            }

            public void DrawButtons()
            {
                if (!isModuleInitEditor) return;

                initModuleEditor.Buttons();
            }
        }
    }
}