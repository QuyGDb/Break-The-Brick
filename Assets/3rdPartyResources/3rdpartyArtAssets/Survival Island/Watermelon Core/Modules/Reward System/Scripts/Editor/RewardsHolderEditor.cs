using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace Watermelon
{
    [CustomEditor(typeof(RewardsHolder), true)]
    public class RewardsHolderEditor : Editor
    {
        private Reward[] rewards;
        private WatermelonEditor[] editors;

        private SerializedProperty scriptsProperty;

        private IEnumerable<SerializedProperty> properties;
        private IEnumerable<SerializedProperty> eventsProperties;
        private IEnumerable<SerializedProperty> ungroupedProperties;

        private static List<Type> availableTypes;

        private GUIContent addButton;
        private GUIStyle removeButtonStyle;

        private List<Type> nestedTypes;

        [InitializeOnLoadMethod]
        private static void InitialiseTypes()
        {
            availableTypes = new List<Type>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly != null)
                {
                    IEnumerable<Type> types = assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Reward)));
                    foreach (Type type in types)
                    {
                        availableTypes.Add(type);
                    }
                }
            }
        }

        private void OnEnable()
        {
            // Cache nested classes
            nestedTypes = PropertyUtility.GetClassNestedTypes(target.GetType());

            addButton = new GUIContent("", EditorCustomStyles.GetIcon("icon_add"));

            removeButtonStyle = new GUIStyle(EditorCustomStyles.buttonRed);
            removeButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            removeButtonStyle.fontSize = 9;
            removeButtonStyle.fontStyle = FontStyle.Bold;

            scriptsProperty = serializedObject.FindProperty("m_Script");

            properties = serializedObject.GetPropertiesByGroup(nestedTypes, "Settings");
            eventsProperties = serializedObject.GetPropertiesByGroup(nestedTypes, "Events");

            ungroupedProperties = serializedObject.GetUngroupProperties();

            UpdateOffers();

            Undo.undoRedoPerformed += OnUndoPerformed;
        }

        private void OnDisable()
        {
            if (!editors.IsNullOrEmpty())
            {
                for (int i = 0; i < editors.Length; i++)
                {
                    if (editors[i] != null)
                    {
                        Editor.DestroyImmediate(editors[i]);
                    }
                }
            }

            Undo.undoRedoPerformed -= OnUndoPerformed;
        }

        private void OnUndoPerformed()
        {
            UpdateOffers();
        }

        private void UpdateOffers()
        {
            if (!editors.IsNullOrEmpty())
            {
                for (int i = 0; i < editors.Length; i++)
                {
                    if (editors[i] != null)
                    {
                        Editor.DestroyImmediate(editors[i]);
                    }
                }
            }

            RewardsHolder rewardsHolder = (RewardsHolder)target;

            rewards = rewardsHolder.GetComponents<Reward>();
            editors = new WatermelonEditor[rewards.Length];

            for (int i = 0; i < rewards.Length; i++)
            {
                Editor cachedEditor = null;

                Editor.CreateCachedEditor(rewards[i], typeof(WatermelonEditor), ref cachedEditor);

                editors[i] = (WatermelonEditor)cachedEditor;
                editors[i].SetScriptFieldState(false);

                rewards[i].hideFlags = HideFlags.HideInInspector;
            }

        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (scriptsProperty != null)
            {
                using (new EditorGUI.DisabledScope(disabled: true))
                {
                    EditorGUILayout.PropertyField(scriptsProperty);
                }
            }

            foreach (var property in properties)
            {
                EditorGUILayout.PropertyField(property);
            }

            int indentLevel = EditorGUI.indentLevel;

            Rect boxRect = EditorGUILayout.BeginVertical(EditorCustomStyles.box);

            EditorGUILayout.LabelField("Rewards", EditorCustomStyles.labelBold);

            if (!editors.IsNullOrEmpty())
            {
                for (int i = 0; i < editors.Length; i++)
                {
                    Rect tittleRect = EditorGUILayout.BeginHorizontal();

                    if (editors[i].ElementsExist)
                    {
                        editors[i].ScriptProperty.isExpanded = EditorGUILayout.Foldout(editors[i].ScriptProperty.isExpanded, new GUIContent(FormatOfferName(rewards[i].GetType())), true);
                    }
                    else
                    {
                        EditorGUILayout.PrefixLabel(new GUIContent(FormatOfferName(rewards[i].GetType())));
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("X", removeButtonStyle, GUILayout.Width(14), GUILayout.Height(14)))
                    {
                        Undo.DestroyObjectImmediate(rewards[i]);

                        Component.DestroyImmediate(rewards[i]);

                        UpdateOffers();

                        return;
                    }

                    EditorGUILayout.EndHorizontal();

                    if(GUI.Button(tittleRect, GUIContent.none, GUIStyle.none))
                    {
                        editors[i].ScriptProperty.isExpanded = !editors[i].ScriptProperty.isExpanded;
                    }

                    if (editors[i].ScriptProperty.isExpanded)
                    {
                        EditorGUI.indentLevel++;

                        editors[i].OnInspectorGUI();
                    }

                    EditorGUI.indentLevel = indentLevel;
                }
            }
            else
            {
                EditorGUILayout.LabelField("List is empty!");
            }

            EditorGUILayout.EndVertical();

            // Buttons panel
            GUILayout.Space(20);

            Rect buttonsPanelRect = new Rect(boxRect.x + boxRect.width - 35, boxRect.y + boxRect.height - 1, 24, 16);
            Rect addButtonRect = new Rect(buttonsPanelRect.x + 5, buttonsPanelRect.y, 14, 14);

            GUI.Box(buttonsPanelRect, "", EditorCustomStyles.boxBottomPanel);
            GUI.Label(addButtonRect, addButton, EditorCustomStyles.labelCentered);

            if (GUI.Button(buttonsPanelRect, GUIContent.none, GUIStyle.none))
            {
                // create the menu and add items to it
                GenericMenu menu = new GenericMenu();

                foreach (Type type in availableTypes)
                {
                    string componentName = FormatOfferName(type);

                    if (ComponentExists(type))
                    {
                        menu.AddDisabledItem(new GUIContent(componentName), false);
                    }
                    else
                    {
                        menu.AddItem(new GUIContent(componentName), false, () => AddComponent(type));
                    }
                }

                // display the menu
                menu.ShowAsContext();

                GUIUtility.ExitGUI();

                return;
            }

            foreach (var property in ungroupedProperties)
            {
                EditorGUILayout.PropertyField(property);
            }

            foreach (var property in eventsProperties)
            {
                EditorGUILayout.PropertyField(property);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private string FormatOfferName(Type type)
        {
            return type.Name.AddSpaces();
        }

        private bool ComponentExists(Type type)
        {
            for(int i = 0; i < rewards.Length; i++)
            {
                if (rewards[i].GetType() == type)
                    return true;
            }

            return false;
        }

        private void AddComponent(Type componentType)
        {
            RewardsHolder rewardsHolder = (RewardsHolder)target;

            Component component = Undo.AddComponent(rewardsHolder.gameObject, componentType);
            component.hideFlags = HideFlags.HideInInspector;

            UpdateOffers();

            EditorUtility.SetDirty(rewardsHolder);
        }
    }
}
