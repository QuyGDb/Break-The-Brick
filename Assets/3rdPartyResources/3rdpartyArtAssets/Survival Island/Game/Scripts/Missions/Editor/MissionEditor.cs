using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;
using Watermelon.List;
using UnityEditorInternal;

namespace Watermelon
{
    public class MissionEditor : EditorWindow
    {

        //PlayerPrefs
        private const string PREFS_MISSION = "editor_mission_index";
        private const string PREFS_WIDTH = "editor_sidebar_width";

        private const string UNDO_ORDER_CHANGE = "[Mission Editor] Undo order change";
        private const string UNDO_ADD_MISSION = "[Mission Editor] Undo add mission";
        private const string UNDO_REMOVE_MISSION = "[Mission Editor] Undo remove mission";

        private CustomList missionsReordableList;
        private List<MissionRepresentation> missions;
        private bool lastActiveMissionOpened;
        private Rect separatorRect;
        private bool separatorIsDragged;
        private int currentSideBarWidth;
        private int SIDEBAR_WIDTH = 320;
        public WorldBehavior worldBehavior;
        public MissionsHolder missionsHolder;
        private static Vector3 defaultPreviewPointPosition;
        private List<Type> possibleMissionTypes;
        private Vector2 scrollVector;
        public string scenePrefix;
        private bool needToUpdateListAfterUndo;

        [MenuItem("Tools/Mission Editor")]
        static void ShowWindow()
        {
            MissionEditor window = GetWindow<MissionEditor>();
            window.titleContent = new GUIContent("Mission Editor");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnEnable()
        {
            worldBehavior = null;
            SearchForWorldBehaviour();

            if (worldBehavior == null)
            {
                return;
            }

            Type[] types = Assembly.GetAssembly(typeof(Mission)).GetTypes();
            possibleMissionTypes = new List<Type>();

            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].IsSubclassOf(typeof(Mission)))
                {
                    possibleMissionTypes.Add(types[i]);
                }
            }

            missions = new List<MissionRepresentation>();

            if (missionsHolder == null)
            {
                SearchForMissionsHolder();
            }

            Undo.undoRedoEvent += OnUndoRedoEvent;

            SetUpReordableList();
            currentSideBarWidth = PlayerPrefs.GetInt(PREFS_WIDTH, SIDEBAR_WIDTH);


            if (missionsHolder != null)
            {
                UpdateMissionList();
            }
        }

        void OnUndoRedoEvent(in UndoRedoInfo info)
        {
            if ((info.undoName.Equals(UNDO_ORDER_CHANGE)) || (info.undoName.Equals(UNDO_ADD_MISSION)) || (info.undoName.Equals(UNDO_REMOVE_MISSION)))
            {
                needToUpdateListAfterUndo = true;
            }
        }

        private void HandleListUpdateAfterUndo()
        {
            if (needToUpdateListAfterUndo)
            {
                missionsReordableList.SelectedIndex = -1;
                UpdateMissionList();
                needToUpdateListAfterUndo = false;
            }
        }

        private void OnDisable()
        {
            MissionRepresentation.UnloadEditor();
            Undo.undoRedoEvent -= OnUndoRedoEvent;
        }

        public void SearchForWorldBehaviour()
        {
            GameObject[] rootGameobjects = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int i = 0; i < rootGameobjects.Length; i++)
            {
                worldBehavior = rootGameobjects[i].GetComponentInChildren<WorldBehavior>();

                if (worldBehavior != null)
                {
                    missionsHolder = GetMissionsHolderWithReflection();
                    defaultPreviewPointPosition = worldBehavior.SpawnPoint.transform.position;
                    return;
                }
            }
        }

        public void SearchForMissionsHolder()
        {
            GameObject[] rootGameobjects = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int i = 0; i < rootGameobjects.Length; i++)
            {
                missionsHolder = rootGameobjects[i].GetComponentInChildren<MissionsHolder>();

                if (missionsHolder != null)
                {
                    SetMissionsHolderWithReflection();
                    return;
                }
            }

        }

        public void CreateMissionsHolder()
        {
            GameObject missionsHolderGameObject = new GameObject("Missions Holder");
            missionsHolderGameObject.transform.SetParent(worldBehavior.transform);
            missionsHolder = missionsHolderGameObject.AddComponent<MissionsHolder>();
            SetMissionsHolderWithReflection();
        }

        public void SetMissionsHolderWithReflection()
        {
            worldBehavior.GetType().GetField("missionsHolder", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(worldBehavior, missionsHolder);
        }

        public MissionsHolder GetMissionsHolderWithReflection()
        {
            return (MissionsHolder)worldBehavior.GetType().GetField("missionsHolder", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(worldBehavior);
        }

        private void OpenLastActiveMission()
        {
            if (!lastActiveMissionOpened)
            {
                if (missions.Count > 0 && PlayerPrefs.HasKey(PREFS_MISSION))
                {
                    int missionIndex = Mathf.Clamp(PlayerPrefs.GetInt(PREFS_MISSION, 0), 0, missions.Count - 1);
                    missionsReordableList.SelectedIndex = missionIndex;
                }

                lastActiveMissionOpened = true;
            }
        }


        #region ReordableList
        private void SetUpReordableList()
        {
            missionsReordableList = new CustomList(missions, GetLabelCallback);
            missionsReordableList.selectionChangedCallback = SelectionChangedCallback;
            missionsReordableList.getHeaderLabelCallback = GetHeaderLabelCallback;
            missionsReordableList.removeElementCallback = RemoveElementCallback;
            missionsReordableList.addElementWithDropdownCallback = AddElementsWithDropdownCallback;
            missionsReordableList.listReorderedCallback = ListReorderedCallback;
            missionsReordableList.listUndoCallback = HandleListUndo;
        }


        private string GetLabelCallback(SerializedProperty elementProperty, int elementIndex)
        {
            return missions[elementIndex].listLabel;
        }

        private void SelectionChangedCallback()
        {
            PlayerPrefs.SetInt(PREFS_MISSION, missionsReordableList.SelectedIndex);
            PlayerPrefs.Save();
        }

        private void RemoveElementCallback()
        {
            if (EditorUtility.DisplayDialog("Warning", $"Are you sure that you want to delete {missions[missionsReordableList.SelectedIndex].listLabel}?", "Yes", "Cancel"))
            {
                Undo.RegisterChildrenOrderUndo(missionsHolder.gameObject, UNDO_REMOVE_MISSION);
                GameObject gameObject = missions[missionsReordableList.SelectedIndex].mission.gameObject;
                missionsReordableList.SelectedIndex = -1;
                DestroyImmediate(gameObject);
                UpdateMissionList();
                ListReorderedCallback();
            }
        }

        private string GetHeaderLabelCallback()
        {
            return "Missions:";
        }

        private void AddElementsWithDropdownCallback()
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < possibleMissionTypes.Count; i++)
            {
                menu.AddItem(new GUIContent(possibleMissionTypes[i].ToString()), false, AddMission, i);
            }


            menu.ShowAsContext();
        }

        private void AddMission(object userData)
        {
            Undo.RegisterChildrenOrderUndo(missionsHolder.gameObject, UNDO_ADD_MISSION);
            Type missionType = possibleMissionTypes[(int)userData];

            GameObject missionsHolderGameObject = new GameObject($"M #{missions.Count + 1} - ");
            missionsHolderGameObject.transform.SetParent(missionsHolder.transform);
            Component component = missionsHolderGameObject.AddComponent(missionType);
            missions.Add(new MissionRepresentation((Mission)component, scenePrefix));
            missionsReordableList.SelectedIndex = missions.Count - 1;

            FieldInfo refField = missionType.GetField("pointerLocation", BindingFlags.Instance | BindingFlags.NonPublic);

            if (refField != null)
            {
                GameObject previewPoint = new GameObject("Custom Pointer Location");
                previewPoint.transform.position = defaultPreviewPointPosition;
                previewPoint.transform.parent = missionsHolderGameObject.transform;
                refField.SetValue(component, previewPoint.transform);
            }
        }

        private void ListReorderedCallback()
        {
            GameObject[] gameObjects = new GameObject[missions.Count];

            for (int i = 0; i < missions.Count; i++)
            {
                gameObjects[i] = missions[i].mission.gameObject;
            }

            for (int i = 0; i < missions.Count; i++)
            {
                missions[i].mission.transform.SetParent(null);
            }

            for (int i = 0; i < missions.Count; i++)
            {
                missions[i].mission.transform.SetParent(missionsHolder.transform);
                missions[i].listLabel = $"M #{(i + 1).ToString("D2")} - {missions[i].note}";
                missions[i].mission.gameObject.name = missions[i].listLabel;
            }
        }

        private void HandleListUndo()
        {
            Undo.RegisterChildrenOrderUndo(missionsHolder.gameObject, UNDO_ORDER_CHANGE);
        }


        #endregion

        private void UpdateMissionList()
        {
            string sceneName = missionsHolder.gameObject.scene.name;
            int emptySpaceIndex = sceneName.IndexOf(' ');

            if (emptySpaceIndex != -1)
            {
                scenePrefix = " " + sceneName.Remove(emptySpaceIndex) + sceneName.Substring(emptySpaceIndex + 1) + " ";
            }
            else
            {
                scenePrefix = sceneName;
            }


            Mission[] missionComponents = missionsHolder.GetComponentsInChildren<Mission>();
            missions.Clear();

            for (int i = 0; i < missionComponents.Length; i++)
            {
                missions.Add(new MissionRepresentation(missionComponents[i], scenePrefix));
            }
        }



        private void OnGUI()
        {
            if (worldBehavior == null)
            {
                EditorGUILayout.HelpBox("World Behaviour is not found on  \"" + SceneView.lastActiveSceneView.name + "\" scene.", MessageType.Error);

                if (!EditorApplication.isPlaying)
                {
                    OnEnable();
                }

                return;
            }

            if (missionsHolder == null)
            {
                EditorGUILayout.HelpBox("Missions Holder is not found on  \"" + SceneView.lastActiveSceneView.name + "\" scene.", MessageType.Error);

                if (GUILayout.Button("Create Missions Holder"))
                {
                    CreateMissionsHolder();
                    UpdateMissionList();
                }

                return;
            }

            DisplayMissionsTab();
            HandleListUpdateAfterUndo();
        }

        private void DisplayMissionsTab()
        {
            OpenLastActiveMission();
            EditorGUILayout.BeginHorizontal();
            //sidebar
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(currentSideBarWidth));
            missionsReordableList.Display();
            DisplaySidebarButtons();
            EditorGUILayout.EndVertical();

            HandleChangingSideBar();

            scrollVector = EditorGUILayout.BeginScrollView(scrollVector);
            //level content
            EditorGUILayout.BeginVertical(EditorCustomStyles.windowSpacedContent);
            DisplaySelectedMission();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();
        }

        private void DisplaySidebarButtons()
        {
        }

        private void DisplaySelectedMission()
        {
            if (missionsReordableList.SelectedIndex == -1)
            {
                return;
            }

            EditorGUILayoutCustom.BeginBoxGroup(missions[missionsReordableList.SelectedIndex].listLabel + " | " + missions[missionsReordableList.SelectedIndex].mission.GetType().ToString());

            EditorGUI.BeginChangeCheck();
            missions[missionsReordableList.SelectedIndex].note = EditorGUILayout.TextField("Note", missions[missionsReordableList.SelectedIndex].note);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(missions[missionsReordableList.SelectedIndex].mission.gameObject, "Change name");
                missions[missionsReordableList.SelectedIndex].listLabel = $"M #{(missionsReordableList.SelectedIndex + 1).ToString("D2")} - {missions[missionsReordableList.SelectedIndex].note}";
                missions[missionsReordableList.SelectedIndex].mission.gameObject.name = missions[missionsReordableList.SelectedIndex].listLabel;
            }

            missions[missionsReordableList.SelectedIndex].DrawSerializedObject();
            EditorGUILayoutCustom.EndBoxGroup();

            GUILayout.FlexibleSpace();

        }

        private void HandleChangingSideBar()
        {
            separatorRect = EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.MinWidth(8), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.AddCursorRect(separatorRect, MouseCursor.ResizeHorizontal);


            if (separatorRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    separatorIsDragged = true;
                    missionsReordableList.IgnoreDragEvents = true;
                    Event.current.Use();
                }
            }

            if (separatorIsDragged)
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    separatorIsDragged = false;
                    missionsReordableList.IgnoreDragEvents = false;
                    PlayerPrefs.SetInt(PREFS_WIDTH, currentSideBarWidth);
                    PlayerPrefs.Save();
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    currentSideBarWidth = Mathf.RoundToInt(Event.current.delta.x) + currentSideBarWidth;
                    Event.current.Use();
                }
            }
        }

        private void OnDestroy()
        {
        }

        public void OnBeforeAssemblyReload()
        {
        }

        private class MissionRepresentation
        {
            private const string CUSTOM_TAEGET_PROPERTY_PATH = "customTargetPosition";
            private const string USE_CUSTOM_TARGET_PROPERTY_PATH = "useCustomTargetPosition";
            private const string POINTER_NAME = "Custom Pointer Location";
            public string listLabel;
            public string note;
            public Mission mission;
            public SerializedObject missionSerializedObject;
            public bool saveExist;
            public string saveString;

            private static WatermelonEditor editor;

            public MissionRepresentation(Mission mission, string scenePrefix)
            {
                this.mission = mission;
                saveString = scenePrefix + mission.gameObject.name;
                listLabel = mission.gameObject.name;
                missionSerializedObject = new SerializedObject(mission);
                ParseNote();
                saveExist = SavePresets.IsSaveExistById(mission.ID);
            }

            private void ParseNote()
            {
                int index = listLabel.IndexOf('-');

                if (index == -1)
                {
                    note = string.Empty;
                }
                else
                {
                    note = listLabel.Substring(index + 2, listLabel.Length - index - 2);
                }
            }

            public void DrawSerializedObject()
            {
                missionSerializedObject.Update();
                UpdateEditorIfNessesary();
                editor.OnInspectorGUI();

                SerializedProperty useCustomTargetProperty = missionSerializedObject.FindProperty(USE_CUSTOM_TARGET_PROPERTY_PATH);
                SerializedProperty customTargetProperty = missionSerializedObject.FindProperty(CUSTOM_TAEGET_PROPERTY_PATH);

                if (useCustomTargetProperty.boolValue)
                {
                    if (customTargetProperty.objectReferenceValue == null)// creating a ref
                    {
                        Transform missionTransform = ((Mission)missionSerializedObject.targetObject).transform;
                        bool searching = true;

                        for (int i = 0; searching && i < missionTransform.childCount; i++)
                        {
                            if (missionTransform.GetChild(i).name.Equals(POINTER_NAME))
                            {
                                customTargetProperty.objectReferenceValue = missionTransform.GetChild(i);
                                missionSerializedObject.ApplyModifiedProperties();
                                searching = false;
                            }
                        }

                        if (searching)
                        {
                            GameObject pointerLocation = new GameObject("Custom Pointer Location");
                            pointerLocation.transform.position = defaultPreviewPointPosition;
                            pointerLocation.transform.parent = missionTransform;

                            customTargetProperty.objectReferenceValue = pointerLocation.transform;
                            missionSerializedObject.ApplyModifiedProperties();
                        }
                    }
                }

                missionSerializedObject.ApplyModifiedProperties();

                if (saveExist && GUILayout.Button("Load save"))
                {
                    SavePresets.LoadSaveById(mission.ID);
                }
            }

            public static void UnloadEditor()
            {
                if (editor != null)
                {
                    DestroyImmediate(editor);
                }
            }

            public void UpdateEditorIfNessesary()
            {
                if(editor != null)
                {
                    if(editor.target == missionSerializedObject.targetObject)
                    {
                        return;
                    }
                    else
                    {
                        DestroyImmediate(editor);
                        editor = (WatermelonEditor)Editor.CreateEditor(missionSerializedObject.targetObject, typeof(WatermelonEditor));
                    }
                }
                else
                {
                    editor = (WatermelonEditor)Editor.CreateEditor(missionSerializedObject.targetObject, typeof(WatermelonEditor));
                }
            }
        }
    }
}