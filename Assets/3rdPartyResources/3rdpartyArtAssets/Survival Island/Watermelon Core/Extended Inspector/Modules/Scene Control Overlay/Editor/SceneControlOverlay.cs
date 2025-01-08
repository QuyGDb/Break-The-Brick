using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;


namespace Watermelon
{
    [UnityEditor.Overlays.Overlay(typeof(SceneView), "Control Overlay", true)]
    [Icon(ICON_ASSET_PATH)]
    public class SceneControlOverlay : UnityEditor.Overlays.Overlay
    {
        private const string ICON_ASSET_PATH = "Assets/Project Data/Watermelon Core/Extended Editor/Icons/icon_settings.png";
        private const string PREFERABLE_DATABASE_PATH = "Assets/Project Data/Content/Settings/Editor/Scene Overlay Database.asset";
        private const string TOGGLE_INTERFACE_SAVE_PREFIX = "overlay_t_i_";
        private const string GAMEOBJECT_PICABILITY_SAVE_PREFIX = "overlay_p_";
        private const string GAMEOBJECT_VISIBILITY_SAVE_PREFIX = "overlay_v_";

        private List<ParsedAnotations> anotations;
        private List<GizmoGroupCached> gizmoGroups;
        private List<GameObjectGroupCached> gameObjectPickabilityGroups;
        private List<GameObjectGroupCached> gameObjectVisibilityGroups;
        private List<ToggleInterfaceGroup> toggleInterfaceGroups;
        private List<ButtonInterfaceGroup> buttonInterfaceGroups;
        private MethodInfo setGizmoEnabled;
        private MethodInfo setIconEnabled;

        private SceneOverlayDatabase database;
        private bool isDatabaseFound;
        private List<Toggle> gizmoToggles;
        private List<Toggle> gameObjectsPickabilityToggles;
        private List<Toggle> gameObjectsVisibilityToggles;
        private Toggle showNawmeshToggle;
        private bool inited = false;
        private VisualElement root;
        private Foldout interfacesFoldout;
        private VisualElement interfacesFoldoutStyleCancel;
        private MethodInfo showNavMeshGetter;
        private MethodInfo showNavMeshSetter;

        private static IEnumerable<Type> registeredAttributeTypes;
        private IEnumerable<Type> registeredControlTypes;
        private List<CustomControlsGroup> customControls;
        private bool autoRefreshNessesary;

        public override VisualElement CreatePanelContent()
        {
            root = new VisualElement();
            
            root.RegisterCallback<MouseEnterEvent>(AutoRefreshOnMouseEnter); // MouseEnterWindowEvent event newer called

            if (!isDatabaseFound)
            {
                root.Add(new Label("Database not found"));
                Button searchButton = new Button(SearchButtonClick);
                searchButton.text = "Search again";
                root.Add(searchButton);
                Button createDatabaseButton = new Button(CreateDatabase);
                createDatabaseButton.text = "Create database";
                root.Add(createDatabaseButton);
            }
            else if (inited)
            {
                BuildUI();
            }
            else
            {
                DelayedInitialization();
            }

            return root;
        }

        private void AutoRefreshOnMouseEnter(MouseEnterEvent evt)
        {
            if (!isDatabaseFound)
            {
                SearchButtonClick();
            }
            else if (inited && autoRefreshNessesary)
            {
                OnHierarchyChanged(); // auto refresh every time mouse enters a window
                autoRefreshNessesary = false;
            }
        }

        private void SearchButtonClick()
        {
            LookForDatabase();

            if (isDatabaseFound)
            {
                DelayedInitialization();
            }
        }

        private void BuildUI()
        {
            root.Clear();
            Foldout gizmoFoldout = new Foldout();
            gizmoFoldout.text = "Gizmo";
            gizmoFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
            gizmoFoldout.SetValueWithoutNotify(false);

            VisualElement gizmoFoldoutStyleCancel = new VisualElement();
            gizmoFoldoutStyleCancel.style.unityFontStyleAndWeight = FontStyle.Normal;
            gizmoFoldout.Add(gizmoFoldoutStyleCancel);

            if (showNawmeshToggle != null)
            {
                gizmoFoldoutStyleCancel.Add(showNawmeshToggle);
            }

            for (int i = 0; i < gizmoToggles.Count; i++)
            {
                gizmoFoldoutStyleCancel.Add(gizmoToggles[i]);
            }
            
            root.Add(gizmoFoldout);

            Foldout gameObjectsPickabilityFoldout = new Foldout();
            gameObjectsPickabilityFoldout.text = "Pickability";
            gameObjectsPickabilityFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;

            VisualElement pickabilityStyleCancel = new VisualElement();
            pickabilityStyleCancel.style.unityFontStyleAndWeight = FontStyle.Normal;
            gameObjectsPickabilityFoldout.Add(pickabilityStyleCancel);

            for (int i = 0; i < gameObjectsPickabilityToggles.Count; i++)
            {
                pickabilityStyleCancel.Add(gameObjectsPickabilityToggles[i]);
            }

            gameObjectsPickabilityFoldout.SetValueWithoutNotify(false);
            root.Add(gameObjectsPickabilityFoldout);

            Foldout gameObjectsVisibilityFoldout = new Foldout();
            gameObjectsVisibilityFoldout.text = "Visibility";
            gameObjectsVisibilityFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;

            VisualElement visibilityStyleCancel = new VisualElement();
            visibilityStyleCancel.style.unityFontStyleAndWeight = FontStyle.Normal;
            gameObjectsVisibilityFoldout.Add(visibilityStyleCancel);

            for (int i = 0; i < gameObjectsVisibilityToggles.Count; i++)
            {
                visibilityStyleCancel.Add(gameObjectsVisibilityToggles[i]);
            }


            gameObjectsVisibilityFoldout.SetValueWithoutNotify(false);
            root.Add(gameObjectsVisibilityFoldout);

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                interfacesFoldout = new Foldout();
                interfacesFoldout.text = "World";
                interfacesFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;

                interfacesFoldoutStyleCancel = new VisualElement();
                interfacesFoldoutStyleCancel.style.unityFontStyleAndWeight = FontStyle.Normal;
                interfacesFoldout.Add(interfacesFoldoutStyleCancel);

                UpdateInterfacesFoldout();
                interfacesFoldout.SetValueWithoutNotify(false);
                root.Add(interfacesFoldout);
            }
        }

        private void UpdateInterfacesFoldout()
        {
            if(interfacesFoldout == null)
            {
                BuildUI();
            }

            interfacesFoldoutStyleCancel.contentContainer.Clear();

            for (int i = 0; i < toggleInterfaceGroups.Count; i++)
            {
                interfacesFoldoutStyleCancel.Add(toggleInterfaceGroups[i].toggle);
            }

            for (int i = 0; i < buttonInterfaceGroups.Count; i++)
            {
                interfacesFoldoutStyleCancel.Add(buttonInterfaceGroups[i].button);
            }

            for (int i = 0; i < customControls.Count; i++)
            {
                interfacesFoldoutStyleCancel.Add(customControls[i].control);
            }

            interfacesFoldout.MarkDirtyRepaint();
        }

        private void DelayedInitialization()
        {
            Unsubscribe();

            if (root == null)
            {
                return;
            }

            InitStuff();

            if (!inited)
            {
                return;
            }

            BuildUI();

            root.MarkDirtyRepaint();
        }

        private void SetUpToggles()
        {
            gizmoToggles = new List<Toggle>();
            gameObjectsPickabilityToggles = new List<Toggle>();
            gameObjectsVisibilityToggles = new List<Toggle>();

            for (int i = 0; i < gizmoGroups.Count; i++)
            {
                Toggle toggle = new Toggle(gizmoGroups[i].name);
                toggle.value = (gizmoGroups[i].status == GizmoGroupStatus.Enabled);

                if (gizmoGroups[i].status == GizmoGroupStatus.Unknown)
                {
                    toggle.showMixedValue = true;
                }

                if (gizmoGroups[i].indexes.Count == 0)
                {
                    toggle.SetEnabled(false);
                }

                toggle.RegisterValueChangedCallback<bool>(GizmoToggleCallback);
                gizmoToggles.Add(toggle);
            }

            for (int i = 0; i < gameObjectPickabilityGroups.Count; i++)
            {
                Toggle toggle = new Toggle(gameObjectPickabilityGroups[i].name);
                toggle.value = gameObjectPickabilityGroups[i].isEnabled;
                toggle.RegisterValueChangedCallback<bool>(GameObjectPickabilityToggleCallback);
                gameObjectsPickabilityToggles.Add(toggle);
            }

            for (int i = 0; i < gameObjectVisibilityGroups.Count; i++)
            {
                Toggle toggle = new Toggle(gameObjectVisibilityGroups[i].name);
                toggle.value = gameObjectVisibilityGroups[i].isEnabled;
                toggle.RegisterValueChangedCallback<bool>(GameObjectVisibilityToggleCallback);
                gameObjectsVisibilityToggles.Add(toggle);
            }
        }

        private void GizmoToggleCallback(ChangeEvent<bool> evt)
        {
            int index = -1;

            for (int i = 0; i < gizmoToggles.Count; i++)
            {
                if ((gizmoToggles[i] as IEventHandler) == evt.target)
                {
                    index = i;
                    break;
                }
            }

            ToggleGizmoGroup(index, evt.newValue);
            UpdateGizmoGroupsStatus();

            for (int i = 0; i < gizmoToggles.Count; i++)
            {
                gizmoToggles[i].SetValueWithoutNotify(gizmoGroups[i].status == GizmoGroupStatus.Enabled);
                gizmoToggles[i].showMixedValue = (gizmoGroups[i].status == GizmoGroupStatus.Unknown);
                gizmoToggles[i].MarkDirtyRepaint();
            }
        }

        private void GameObjectPickabilityToggleCallback(ChangeEvent<bool> evt)
        {
            int index = -1;

            for (int i = 0; i < gameObjectsPickabilityToggles.Count; i++)
            {
                if ((gameObjectsPickabilityToggles[i] as IEventHandler) == evt.target)
                {
                    index = i;
                    break;
                }
            }

            ToggleGameObjectPickabilityGroup(index, evt.newValue);
            EditorPrefs.SetBool(GAMEOBJECT_PICABILITY_SAVE_PREFIX + gameObjectPickabilityGroups[index].name, gameObjectsPickabilityToggles[index].value);
            UpdateGameObjectPickabilityToggles();
        }

        private void GameObjectVisibilityToggleCallback(ChangeEvent<bool> evt)
        {
            int index = -1;

            for (int i = 0; i < gameObjectsVisibilityToggles.Count; i++)
            {
                if ((gameObjectsVisibilityToggles[i] as IEventHandler) == evt.target)
                {
                    index = i;
                    break;
                }
            }

            ToggleGameObjectVisibilityGroup(index, evt.newValue);
            EditorPrefs.SetBool(GAMEOBJECT_VISIBILITY_SAVE_PREFIX + gameObjectVisibilityGroups[index].name, gameObjectsVisibilityToggles[index].value);
            UpdateGameObjectVisibilityToggles();
        }

        private void UpdateGameObjectPickabilityToggles()
        {
            for (int i = 0; i < gameObjectsPickabilityToggles.Count; i++)
            {
                gameObjectsPickabilityToggles[i].MarkDirtyRepaint();
            }
        }

        private void UpdateGameObjectVisibilityToggles()
        {
            for (int i = 0; i < gameObjectsVisibilityToggles.Count; i++)
            {
                gameObjectsVisibilityToggles[i].MarkDirtyRepaint();
            }
        }

        public override void OnCreated()
        {
            inited = false;
            autoRefreshNessesary = false;
            ObjectChangeEvents.changesPublished += HandleObjectChangeEvents;
            EditorSceneManager.sceneOpened += HandleSceneChange;

            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isUpdating)
            {
                AssemblyReloadEvents.afterAssemblyReload += DelayedInitialization;
            }
            else
            {
                InitStuff();
                inited = true;
            }
        }

        private void HandleSceneChange(Scene scene, OpenSceneMode mode)
        {
            if (inited)
            {
                OnHierarchyChanged();
                autoRefreshNessesary = false;
            }
        }

        private void HandleObjectChangeEvents(ref ObjectChangeEventStream stream)
        {
            ObjectChangeKind type;

            for (int i = 0; i < stream.length; i++)
            {
                type = stream.GetEventType(i);

                switch (type)
                {
                    case ObjectChangeKind.CreateGameObjectHierarchy:
                    case ObjectChangeKind.ChangeGameObjectStructure:
                    case ObjectChangeKind.DestroyGameObjectHierarchy:
                        autoRefreshNessesary = true;
                        break;
                    default:
                        break;
                }
            }
        }

        public override void OnWillBeDestroyed()
        {
            Unsubscribe();
            ObjectChangeEvents.changesPublished -= HandleObjectChangeEvents;
            EditorSceneManager.sceneOpened -= HandleSceneChange;
        }

        private void Unsubscribe()
        {
            AssemblyReloadEvents.afterAssemblyReload -= DelayedInitialization;
            EditorApplication.playModeStateChanged -= PlaymodeStateChanged;
        }

        private void PlaymodeStateChanged(PlayModeStateChange change)
        {
            OnHierarchyChanged();
        }

        private void InitStuff()
        {
            LookForDatabase();

            if (!isDatabaseFound)
            {
                return;
            }

            TryToCacheShowNawmesh();
            ParseAnotations();
            CacheGroups();
            CacheRegisteredTypes();
            UpdateGizmoGroupsStatus();
            UpdateGameObjects();
            SetUpToggles();
            CollectInterfaces();

            EditorApplication.playModeStateChanged += PlaymodeStateChanged;
            inited = true;
        }

        private void LookForDatabase()
        {
            database = EditorUtils.GetAsset<SceneOverlayDatabase>();
            isDatabaseFound = (database != null);
        }

        private void OnHierarchyChanged()
        {
            if(root == null)
            {
                Unsubscribe();
                return;
            }

            UpdateGameObjects();
            UpdateGameObjectPickabilityToggles();
            UpdateGameObjectVisibilityToggles();

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                CollectInterfaces();
                UpdateInterfacesFoldout();
            }
        }

        private void TryToCacheShowNawmesh()
        {
            System.Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.AI.NavMeshVisualizationSettings");

            if (type == null)
            {
                return;
            }

            PropertyInfo propInfo = type.GetProperty("showNavMesh",BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo[] methInfos = propInfo.GetAccessors(true);

            if(methInfos.Length != 2)
            {
                return;
            }

            showNawmeshToggle = new Toggle("Show NavMesh");

            for (int i = 0; i < methInfos.Length; i++)
            {
                if (methInfos[i].ReturnType == typeof(void))
                {
                    showNavMeshSetter = methInfos[i];
                }
                else
                {
                    showNavMeshGetter = methInfos[i];
                    UpdateShowNawMeshToggleValue();
                }
            }

            showNawmeshToggle.RegisterValueChangedCallback(ShowNawmeshCallback);
        }

        private void UpdateShowNawMeshToggleValue()
        {
            showNawmeshToggle.SetValueWithoutNotify((bool)showNavMeshGetter.Invoke(null, null));
        }

        private void ShowNawmeshCallback(ChangeEvent<bool> evt)
        {
            showNavMeshSetter.Invoke(null, new object[] { evt.newValue });
        }

        private void ParseAnotations()
        {
            System.Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.AnnotationUtility");

            if (type == null)
            {
                Debug.LogError("Can`t find type UnityEditor.AnnotationUtility");
                return;
            }

            MethodInfo getAnnotations = type.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
            setGizmoEnabled = type.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            setIconEnabled = type.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            anotations = new List<ParsedAnotations>();
            var annotations = getAnnotations.Invoke(null, null);
            System.Type annotationType;
            FieldInfo classIdField;
            FieldInfo scriptClassField;
            FieldInfo iconEnabledField;
            FieldInfo gizmoEnabledField;
            int classId;
            string scriptClass;
            bool iconEnabled;
            bool gizmoEnabled;
            string name;

            foreach (object annotation in (IEnumerable)annotations)
            {
                annotationType = annotation.GetType();
                classIdField = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
                scriptClassField = annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
                iconEnabledField = annotationType.GetField("iconEnabled", BindingFlags.Public | BindingFlags.Instance);
                gizmoEnabledField = annotationType.GetField("gizmoEnabled", BindingFlags.Public | BindingFlags.Instance);
                name = string.Empty;

                try
                {
                    classId = (int)classIdField.GetValue(annotation);
                }
                catch
                {
                    classId = -1;
                }
                try
                {
                    scriptClass = (string)scriptClassField.GetValue(annotation);
                    name = scriptClass;
                }
                catch
                {
                    scriptClass = string.Empty;
                }

                try
                {
                    iconEnabled = ((int)iconEnabledField.GetValue(annotation) > 0);
                }
                catch
                {
                    iconEnabled = false;
                }

                try
                {
                    gizmoEnabled = ((int)gizmoEnabledField.GetValue(annotation) > 0);
                }
                catch
                {
                    gizmoEnabled = false;
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = GetYAMLClassName(classId);
                }

                anotations.Add(new ParsedAnotations(name, scriptClass, classId, iconEnabled, gizmoEnabled));
            }
        }

        private void CreateDatabase()
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SceneOverlayDatabase>(), PREFERABLE_DATABASE_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            DelayedInitialization();
        }

        private void CacheGroups()
        {
            gizmoGroups = new List<GizmoGroupCached>();
            GizmoGroupCached newGizmoGroup;
            SerializedObject serializedObject = new SerializedObject(database);
            SerializedProperty groupsProperty = serializedObject.FindProperty("gizmoGroups");
            SerializedProperty elementProperty;
            SerializedProperty queriesProperty;

            for (int i = 0; i < groupsProperty.arraySize; i++)
            {
                elementProperty = groupsProperty.GetArrayElementAtIndex(i);
                newGizmoGroup = new GizmoGroupCached(elementProperty.FindPropertyRelative("name").stringValue);
                queriesProperty = elementProperty.FindPropertyRelative("queries");

                for (int j = 0; j < queriesProperty.arraySize; j++)
                {
                    CollectIndexes(newGizmoGroup,queriesProperty.GetArrayElementAtIndex(j).stringValue);
                }

                gizmoGroups.Add(newGizmoGroup);
            }

            gameObjectPickabilityGroups = new List<GameObjectGroupCached>();
            GameObjectGroupCached newGameObjectGroup;
            groupsProperty = serializedObject.FindProperty("pickabilityGroups");

            for (int i = 0; i < groupsProperty.arraySize; i++)
            {
                elementProperty = groupsProperty.GetArrayElementAtIndex(i);
                newGameObjectGroup = new GameObjectGroupCached(elementProperty.FindPropertyRelative("name").stringValue);
                newGameObjectGroup.isEnabled = EditorPrefs.GetBool(GAMEOBJECT_PICABILITY_SAVE_PREFIX + newGameObjectGroup.name, true);
                
                queriesProperty = elementProperty.FindPropertyRelative("queries");

                for (int j = 0; j < queriesProperty.arraySize; j++)
                {
                    newGameObjectGroup.queries.Add(queriesProperty.GetArrayElementAtIndex(j).stringValue);
                }

                gameObjectPickabilityGroups.Add(newGameObjectGroup);
            }

            gameObjectVisibilityGroups = new List<GameObjectGroupCached>();
            groupsProperty = serializedObject.FindProperty("visibilityGroups");

            for (int i = 0; i < groupsProperty.arraySize; i++)
            {
                elementProperty = groupsProperty.GetArrayElementAtIndex(i);
                newGameObjectGroup = new GameObjectGroupCached(elementProperty.FindPropertyRelative("name").stringValue);
                newGameObjectGroup.isEnabled = EditorPrefs.GetBool(GAMEOBJECT_VISIBILITY_SAVE_PREFIX + newGameObjectGroup.name, true);

                queriesProperty = elementProperty.FindPropertyRelative("queries");

                for (int j = 0; j < queriesProperty.arraySize; j++)
                {
                    newGameObjectGroup.queries.Add(queriesProperty.GetArrayElementAtIndex(j).stringValue);
                }

                gameObjectVisibilityGroups.Add(newGameObjectGroup);
            }

            toggleInterfaceGroups = new List<ToggleInterfaceGroup>();
            buttonInterfaceGroups = new List<ButtonInterfaceGroup>();
        }

        private void CollectIndexes(GizmoGroupCached newGroup, string query)
        {
            for (int i = 0; i < anotations.Count; i++)
            {
                if (anotations[i].name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    newGroup.indexes.Add(i);
                }
            }
        }

        private void UpdateGizmoGroupsStatus()
        {
            int enabledCounter;
            int disabledCounter;

            for (int i = 0; i < gizmoGroups.Count; i++)
            {
                enabledCounter = 0;
                disabledCounter = 0;

                for (int j = 0; j < gizmoGroups[i].indexes.Count; j++)
                {
                    if (anotations[gizmoGroups[i].indexes[j]].gizmoEnabled)
                    {
                        enabledCounter++;
                    }
                    else
                    {
                        disabledCounter++;
                    }
                }

                if(enabledCounter > 0)
                {
                    if(disabledCounter > 0)
                    {
                        gizmoGroups[i].status = GizmoGroupStatus.Unknown;
                    }
                    else
                    {
                        gizmoGroups[i].status = GizmoGroupStatus.Enabled;
                    }
                }
                else
                {
                    gizmoGroups[i].status = GizmoGroupStatus.Disabled;
                }
            }
        }

        public void ToggleGizmoGroup(int groupIndex, bool isEnabled)
        {
            int value = isEnabled ? 1 : 0;
            gizmoGroups[groupIndex].status = isEnabled ? GizmoGroupStatus.Enabled : GizmoGroupStatus.Disabled;
            ParsedAnotations item;

            foreach (int i in gizmoGroups[groupIndex].indexes)
            {
                item = anotations[i];
                item.gizmoEnabled = isEnabled;
                item.iconEnabled = isEnabled;
                setGizmoEnabled.Invoke(null, new object[] { item.classId, item.scriptClass, value, false });
                setIconEnabled.Invoke(null, new object[] { item.classId, item.scriptClass, value });
            }
        }

        private void UpdateGameObjects()
        {
            foreach (GameObjectGroupCached groupCached in gameObjectPickabilityGroups)
            {
                groupCached.gameObjects.Clear();

                foreach (string query in groupCached.queries)
                {
                    GameObject tempObject = GameObject.Find(query);
                    if(tempObject != null)
                    {
                        groupCached.gameObjects.Add(tempObject);
                    }
                }

                if (groupCached.isEnabled)
                {
                    SceneVisibilityManager.instance.DisablePicking(groupCached.gameObjects.ToArray(), true);
                }
                else
                {
                    SceneVisibilityManager.instance.EnablePicking(groupCached.gameObjects.ToArray(), true);
                }
            }

            foreach (GameObjectGroupCached groupCached in gameObjectVisibilityGroups)
            {
                groupCached.gameObjects.Clear();

                foreach (string query in groupCached.queries)
                {
                    GameObject tempObject = GameObject.Find(query);
                    if (tempObject != null)
                    {
                        groupCached.gameObjects.Add(tempObject);
                    }
                }

                if (groupCached.isEnabled)
                {
                    SceneVisibilityManager.instance.Hide(groupCached.gameObjects.ToArray(), true);
                }
                else
                {
                    SceneVisibilityManager.instance.Show(groupCached.gameObjects.ToArray(), true);
                }
            }
        }

        private void CacheRegisteredTypes()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<Type> types = assemblies.SelectMany(s => s.GetTypes());
            Type attributeType = typeof(CustomOverlayElementAttribute);
            registeredAttributeTypes = types.Where(p => !p.IsAbstract && p.IsDefined(attributeType, true));

            //look for custom controls
            Type customControlType = typeof(SceneOverlayCustomControl);
            registeredControlTypes = types.Where(p => !p.IsAbstract && p.IsSubclassOf(customControlType));
            customControls = new List<CustomControlsGroup>();

            foreach(Type type in registeredControlTypes)
            {
                customControls.Add(new CustomControlsGroup(type));
            }
        }

        private void CollectInterfaces()
        {
            toggleInterfaceGroups.Clear();
            buttonInterfaceGroups.Clear();
            ToggleInterfaceGroup newToggle = null;
            ButtonInterfaceGroup newButton = null;
            bool isToggle = true;

            foreach (Type type in registeredAttributeTypes)
            {
                IEnumerable<CustomOverlayElementAttribute> attributes = type.GetCustomAttributes<CustomOverlayElementAttribute>(true);

                UnityEngine.Object[] sceneObjects = GameObject.FindObjectsOfType(type, true);
                foreach (CustomOverlayElementAttribute attribute in attributes)
                {
                    newToggle = new ToggleInterfaceGroup(attribute.ElementName);
                    newToggle.isEnabled = EditorPrefs.GetBool(TOGGLE_INTERFACE_SAVE_PREFIX + attribute.ElementName, true);
                    newButton = new ButtonInterfaceGroup(attribute.ElementName);

                    foreach (UnityEngine.Object sceneObject in sceneObjects)
                    {
                        MethodInfo methodInfo = sceneObject.GetType().GetMethod(attribute.MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                        if(methodInfo == null)
                        {
                            Debug.LogError($"MethodInfo for method:{attribute.MethodName} in type: {type.FullName} is null.");
                            continue;
                        }


                        ParameterInfo[] methodParams = methodInfo.GetParameters();
                        if (methodParams.IsNullOrEmpty())
                        {
                            isToggle = false;
                            newButton.objects.Add(sceneObject);
                            newButton.methodInfo = methodInfo;
                        }
                        else if (methodParams[0].ParameterType == typeof(bool))
                        {
                            isToggle = true;
                            newToggle.objects.Add(sceneObject);
                            newToggle.methodInfo = methodInfo;
                        }
                    }

                    if (isToggle)
                    {
                        newToggle.CreateUIElement();
                        toggleInterfaceGroups.Add(newToggle);
                    }
                    else
                    {
                        newButton.CreateUIElement();
                        buttonInterfaceGroups.Add(newButton);
                    }
                }
            }

        }

        public void ToggleGameObjectPickabilityGroup(int groupIndex, bool isEnabled)
        {
            gameObjectPickabilityGroups[groupIndex].isEnabled = isEnabled;

            if (isEnabled)
            {
                SceneVisibilityManager.instance.DisablePicking(gameObjectPickabilityGroups[groupIndex].gameObjects.ToArray(), true);
            }
            else
            {
                SceneVisibilityManager.instance.EnablePicking(gameObjectPickabilityGroups[groupIndex].gameObjects.ToArray(), true);
            }

        }

        public void ToggleGameObjectVisibilityGroup(int groupIndex, bool isEnabled)
        {
            gameObjectVisibilityGroups[groupIndex].isEnabled = isEnabled;

            if (isEnabled)
            {
                SceneVisibilityManager.instance.Hide(gameObjectVisibilityGroups[groupIndex].gameObjects.ToArray(), true);
            }
            else
            {
                SceneVisibilityManager.instance.Show(gameObjectVisibilityGroups[groupIndex].gameObjects.ToArray(), true);
            }

        }

        #region YAML ids
        // from this page https://docs.unity3d.com/Manual/ClassIDReference.html
        public string GetYAMLClassName(int id)
        {
            switch (id)
            {
                case 0: return "Object";
                case 1: return "GameObject";
                case 2: return "Component";
                case 3: return "LevelGameManager";
                case 4: return "Transform";
                case 5: return "TimeManager";
                case 6: return "GlobalGameManager";
                case 8: return "Behaviour";
                case 9: return "GameManager";
                case 11: return "AudioManager";
                case 13: return "InputManager";
                case 18: return "EditorExtension";
                case 19: return "Physics2DSettings";
                case 20: return "Camera";
                case 21: return "Material";
                case 23: return "MeshRenderer";
                case 25: return "Renderer";
                case 27: return "Texture";
                case 28: return "Texture2D";
                case 29: return "OcclusionCullingSettings";
                case 30: return "GraphicsSettings";
                case 33: return "MeshFilter";
                case 41: return "OcclusionPortal";
                case 43: return "Mesh";
                case 45: return "Skybox";
                case 47: return "QualitySettings";
                case 48: return "Shader";
                case 49: return "TextAsset";
                case 50: return "Rigidbody2D";
                case 53: return "Collider2D";
                case 54: return "Rigidbody";
                case 55: return "PhysicsManager";
                case 56: return "Collider";
                case 57: return "Joint";
                case 58: return "CircleCollider2D";
                case 59: return "HingeJoint";
                case 60: return "PolygonCollider2D";
                case 61: return "BoxCollider2D";
                case 62: return "PhysicsMaterial2D";
                case 64: return "MeshCollider";
                case 65: return "BoxCollider";
                case 66: return "CompositeCollider2D";
                case 68: return "EdgeCollider2D";
                case 70: return "CapsuleCollider2D";
                case 72: return "ComputeShader";
                case 74: return "AnimationClip";
                case 75: return "ConstantForce";
                case 78: return "TagManager";
                case 81: return "AudioListener";
                case 82: return "AudioSource";
                case 83: return "AudioClip";
                case 84: return "RenderTexture";
                case 86: return "CustomRenderTexture";
                case 89: return "Cubemap";
                case 90: return "Avatar";
                case 91: return "AnimatorController";
                case 93: return "RuntimeAnimatorController";
                case 94: return "ScriptMapper";
                case 95: return "Animator";
                case 96: return "TrailRenderer";
                case 98: return "DelayedCallManager";
                case 102: return "TextMesh";
                case 104: return "RenderSettings";
                case 108: return "Light";
                case 109: return "CGProgram";
                case 110: return "BaseAnimationTrack";
                case 111: return "Animation";
                case 114: return "MonoBehaviour";
                case 115: return "MonoScript";
                case 116: return "MonoManager";
                case 117: return "Texture3D";
                case 118: return "NewAnimationTrack";
                case 119: return "Projector";
                case 120: return "LineRenderer";
                case 121: return "Flare";
                case 122: return "Halo";
                case 123: return "LensFlare";
                case 124: return "FlareLayer";
                case 125: return "HaloLayer";
                case 126: return "NavMeshProjectSettings";
                case 128: return "Font";
                case 129: return "PlayerSettings";
                case 130: return "NamedObject";
                case 134: return "PhysicMaterial";
                case 135: return "SphereCollider";
                case 136: return "CapsuleCollider";
                case 137: return "SkinnedMeshRenderer";
                case 138: return "FixedJoint";
                case 141: return "BuildSettings";
                case 142: return "AssetBundle";
                case 143: return "CharacterController";
                case 144: return "CharacterJoint";
                case 145: return "SpringJoint";
                case 146: return "WheelCollider";
                case 147: return "ResourceManager";
                case 150: return "PreloadData";
                case 153: return "ConfigurableJoint";
                case 154: return "TerrainCollider";
                case 156: return "TerrainData";
                case 157: return "LightmapSettings";
                case 158: return "WebCamTexture";
                case 159: return "EditorSettings";
                case 162: return "EditorUserSettings";
                case 164: return "AudioReverbFilter";
                case 165: return "AudioHighPassFilter";
                case 166: return "AudioChorusFilter";
                case 167: return "AudioReverbZone";
                case 168: return "AudioEchoFilter";
                case 169: return "AudioLowPassFilter";
                case 170: return "AudioDistortionFilter";
                case 171: return "SparseTexture";
                case 180: return "AudioBehaviour";
                case 181: return "AudioFilter";
                case 182: return "WindZone";
                case 183: return "Cloth";
                case 184: return "SubstanceArchive";
                case 185: return "ProceduralMaterial";
                case 186: return "ProceduralTexture";
                case 187: return "Texture2DArray";
                case 188: return "CubemapArray";
                case 191: return "OffMeshLink";
                case 192: return "OcclusionArea";
                case 193: return "Tree";
                case 195: return "NavMeshAgent";
                case 196: return "NavMeshSettings";
                case 198: return "ParticleSystem";
                case 199: return "ParticleSystemRenderer";
                case 200: return "ShaderVariantCollection";
                case 205: return "LODGroup";
                case 206: return "BlendTree";
                case 207: return "Motion";
                case 208: return "NavMeshObstacle";
                case 210: return "SortingGroup";
                case 212: return "SpriteRenderer";
                case 213: return "Sprite";
                case 214: return "CachedSpriteAtlas";
                case 215: return "ReflectionProbe";
                case 218: return "Terrain";
                case 220: return "LightProbeGroup";
                case 221: return "AnimatorOverrideController";
                case 222: return "CanvasRenderer";
                case 223: return "Canvas";
                case 224: return "RectTransform";
                case 225: return "CanvasGroup";
                case 226: return "BillboardAsset";
                case 227: return "BillboardRenderer";
                case 228: return "SpeedTreeWindAsset";
                case 229: return "AnchoredJoint2D";
                case 230: return "Joint2D";
                case 231: return "SpringJoint2D";
                case 232: return "DistanceJoint2D";
                case 233: return "HingeJoint2D";
                case 234: return "SliderJoint2D";
                case 235: return "WheelJoint2D";
                case 236: return "ClusterInputManager";
                case 237: return "BaseVideoTexture";
                case 238: return "NavMeshData";
                case 240: return "AudioMixer";
                case 241: return "AudioMixerController";
                case 243: return "AudioMixerGroupController";
                case 244: return "AudioMixerEffectController";
                case 245: return "AudioMixerSnapshotController";
                case 246: return "PhysicsUpdateBehaviour2D";
                case 247: return "ConstantForce2D";
                case 248: return "Effector2D";
                case 249: return "AreaEffector2D";
                case 250: return "PointEffector2D";
                case 251: return "PlatformEffector2D";
                case 252: return "SurfaceEffector2D";
                case 253: return "BuoyancyEffector2D";
                case 254: return "RelativeJoint2D";
                case 255: return "FixedJoint2D";
                case 256: return "FrictionJoint2D";
                case 257: return "TargetJoint2D";
                case 258: return "LightProbes";
                case 259: return "LightProbeProxyVolume";
                case 271: return "SampleClip";
                case 272: return "AudioMixerSnapshot";
                case 273: return "AudioMixerGroup";
                case 290: return "AssetBundleManifest";
                case 300: return "RuntimeInitializeOnLoadManager";
                case 310: return "UnityConnectSettings";
                case 319: return "AvatarMask";
                case 320: return "PlayableDirector";
                case 328: return "VideoPlayer";
                case 329: return "VideoClip";
                case 330: return "ParticleSystemForceField";
                case 331: return "SpriteMask";
                case 362: return "WorldAnchor";
                case 363: return "OcclusionCullingData";
                case 1001: return "PrefabInstance";
                case 1002: return "EditorExtensionImpl";
                case 1003: return "AssetImporter";
                case 1004: return "AssetDatabaseV1";
                case 1005: return "Mesh3DSImporter";
                case 1006: return "TextureImporter";
                case 1007: return "ShaderImporter";
                case 1008: return "ComputeShaderImporter";
                case 1020: return "AudioImporter";
                case 1026: return "HierarchyState";
                case 1028: return "AssetMetaData";
                case 1029: return "DefaultAsset";
                case 1030: return "DefaultImporter";
                case 1031: return "TextScriptImporter";
                case 1032: return "SceneAsset";
                case 1034: return "NativeFormatImporter";
                case 1035: return "MonoImporter";
                case 1038: return "LibraryAssetImporter";
                case 1040: return "ModelImporter";
                case 1041: return "FBXImporter";
                case 1042: return "TrueTypeFontImporter";
                case 1045: return "EditorBuildSettings";
                case 1048: return "InspectorExpandedState";
                case 1049: return "AnnotationManager";
                case 1050: return "PluginImporter";
                case 1051: return "EditorUserBuildSettings";
                case 1055: return "IHVImageFormatImporter";
                case 1101: return "AnimatorStateTransition";
                case 1102: return "AnimatorState";
                case 1105: return "HumanTemplate";
                case 1107: return "AnimatorStateMachine";
                case 1108: return "PreviewAnimationClip";
                case 1109: return "AnimatorTransition";
                case 1110: return "SpeedTreeImporter";
                case 1111: return "AnimatorTransitionBase";
                case 1112: return "SubstanceImporter";
                case 1113: return "LightmapParameters";
                case 1120: return "LightingDataAsset";
                case 1124: return "SketchUpImporter";
                case 1125: return "BuildReport";
                case 1126: return "PackedAssets";
                case 1127: return "VideoClipImporter";
                case 100000: return "int";
                case 100001: return "bool";
                case 100002: return "float";
                case 100003: return "MonoObject";
                case 100004: return "Collision";
                case 100005: return "Vector3f";
                case 100006: return "RootMotionData";
                case 100007: return "Collision2D";
                case 100008: return "AudioMixerLiveUpdateFloat";
                case 100009: return "AudioMixerLiveUpdateBool";
                case 100010: return "Polygon2D";
                case 100011: return "void";
                case 19719996: return "TilemapCollider2D";
                case 41386430: return "AssetImporterLog";
                case 73398921: return "VFXRenderer";
                case 76251197: return "SerializableManagedRefTestClass";
                case 156049354: return "Grid";
                case 156483287: return "ScenesUsingAssets";
                case 171741748: return "ArticulationBody";
                case 181963792: return "Preset";
                case 277625683: return "EmptyObject";
                case 285090594: return "IConstraint";
                case 293259124: return "TestObjectWithSpecialLayoutOne";
                case 294290339: return "AssemblyDefinitionReferenceImporter";
                case 334799969: return "SiblingDerived";
                case 342846651: return "TestObjectWithSerializedMapStringNonAlignedStruct";
                case 367388927: return "SubDerived";
                case 369655926: return "AssetImportInProgressProxy";
                case 382020655: return "PluginBuildInfo";
                case 426301858: return "EditorProjectAccess";
                case 468431735: return "PrefabImporter";
                case 478637458: return "TestObjectWithSerializedArray";
                case 478637459: return "TestObjectWithSerializedAnimationCurve";
                case 483693784: return "TilemapRenderer";
                case 488575907: return "ScriptableCamera";
                case 612988286: return "SpriteAtlasAsset";
                case 638013454: return "SpriteAtlasDatabase";
                case 641289076: return "AudioBuildInfo";
                case 644342135: return "CachedSpriteAtlasRuntimeData";
                case 646504946: return "RendererFake";
                case 662584278: return "AssemblyDefinitionReferenceAsset";
                case 668709126: return "BuiltAssetBundleInfoSet";
                case 687078895: return "SpriteAtlas";
                case 747330370: return "RayTracingShaderImporter";
                case 825902497: return "RayTracingShader";
                case 850595691: return "LightingSettings";
                case 877146078: return "PlatformModuleSetup";
                case 890905787: return "VersionControlSettings";
                case 895512359: return "AimConstraint";
                case 937362698: return "VFXManager";
                case 994735392: return "VisualEffectSubgraph";
                case 994735403: return "VisualEffectSubgraphOperator";
                case 994735404: return "VisualEffectSubgraphBlock";
                case 1001480554: return "Prefab";
                case 1027052791: return "LocalizationImporter";
                case 1091556383: return "Derived";
                case 1111377672: return "PropertyModificationsTargetTestObject";
                case 1114811875: return "ReferencesArtifactGenerator";
                case 1152215463: return "AssemblyDefinitionAsset";
                case 1154873562: return "SceneVisibilityState";
                case 1183024399: return "LookAtConstraint";
                case 1210832254: return "SpriteAtlasImporter";
                case 1223240404: return "MultiArtifactTestImporter";
                case 1268269756: return "GameObjectRecorder";
                case 1325145578: return "LightingDataAssetParent";
                case 1386491679: return "PresetManager";
                case 1392443030: return "TestObjectWithSpecialLayoutTwo";
                case 1403656975: return "StreamingManager";
                case 1480428607: return "LowerResBlitTexture";
                case 1542919678: return "StreamingController";
                case 1628831178: return "TestObjectVectorPairStringBool";
                case 1742807556: return "GridLayout";
                case 1766753193: return "AssemblyDefinitionImporter";
                case 1773428102: return "ParentConstraint";
                case 1803986026: return "FakeComponent";
                case 1818360608: return "PositionConstraint";
                case 1818360609: return "RotationConstraint";
                case 1818360610: return "ScaleConstraint";
                case 1839735485: return "Tilemap";
                case 1896753125: return "PackageManifest";
                case 1896753126: return "PackageManifestImporter";
                case 1953259897: return "TerrainLayer";
                case 1971053207: return "SpriteShapeRenderer";
                case 1977754360: return "NativeObjectType";
                case 1981279845: return "TestObjectWithSerializedMapStringBool";
                case 1995898324: return "SerializableManagedHost";
                case 2058629509: return "VisualEffectAsset";
                case 2058629510: return "VisualEffectImporter";
                case 2058629511: return "VisualEffectResource";
                case 2059678085: return "VisualEffectObject";
                case 2083052967: return "VisualEffect";
                case 2083778819: return "LocalizationAsset";
                default: return "Unknown";
            }
        }

        #endregion
        private class ParsedAnotations
        {
            public string name;
            public string scriptClass;
            public int classId;
            public bool iconEnabled;
            public bool gizmoEnabled;

            public ParsedAnotations(string name, string scriptClass, int classId, bool iconEnabled, bool gizmoEnabled)
            {
                this.name = name;
                this.scriptClass = scriptClass;
                this.classId = classId;
                this.iconEnabled = iconEnabled;
                this.gizmoEnabled = gizmoEnabled;
            }
        }

        private class GizmoGroupCached
        {
            public string name;
            public List<int> indexes;
            public GizmoGroupStatus status;

            public GizmoGroupCached(string name)
            {
                this.name = name;
                indexes = new List<int>();
            }
        }

        private class GameObjectGroupCached
        {
            public string name;
            public List<string> queries;
            public List<GameObject> gameObjects;
            public bool isEnabled;

            public GameObjectGroupCached(string name)
            {
                this.name = name;
                queries = new List<string>();
                gameObjects = new List<GameObject>();
            }
        }

        private class ToggleInterfaceGroup
        {
            public string name;
            public List<UnityEngine.Object> objects;
            public MethodInfo methodInfo;
            public bool isEnabled;
            public Toggle toggle;

            public ToggleInterfaceGroup(string name)
            {
                this.name = name;
                this.objects = new List<UnityEngine.Object>();
                this.isEnabled = true;
            }

            public void CreateUIElement()
            {
                toggle = new Toggle(name);
                toggle.SetValueWithoutNotify(isEnabled);
                toggle.RegisterValueChangedCallback(OnToggleClick);

            }

            private void OnToggleClick(ChangeEvent<bool> evt)
            {
                object[] parameters = new object[] { evt.newValue };

                foreach (UnityEngine.Object item in objects)
                {
                    methodInfo.Invoke(item, parameters);
                }
            }
        }

        private class ButtonInterfaceGroup
        {
            public string name;
            public List<UnityEngine.Object> objects;
            public MethodInfo methodInfo;
            public Button button;

            public ButtonInterfaceGroup(string name)
            {
                this.name = name;
                this.objects = new List<UnityEngine.Object>();
            }

            public void CreateUIElement()
            {
                button = new Button(OnButtonClicked);
                button.text = name;
            }

            private void OnButtonClicked()
            {
                foreach (UnityEngine.Object item in objects)
                {
                    methodInfo.Invoke(item, null);
                }
            }
        }

        private class CustomControlsGroup
        {
            public SceneOverlayCustomControl scriptInstance;
            public VisualElement control;

            public CustomControlsGroup(Type type)
            {
                scriptInstance = (SceneOverlayCustomControl)Activator.CreateInstance(type);
                control = scriptInstance.CreateControl();
            }
        }

        private enum GizmoGroupStatus
        {
            Unknown = -1,
            Enabled = 0,
            Disabled = 1
        }
    }

}