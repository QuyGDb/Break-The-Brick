using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Watermelon
{
    public static class EditorGUILayoutCustom
    {
        private const float HEADER_HEIGHT = 16;

        public static string FileField(GUIContent content, string value, string directory = "", string extension = "")
        {
            string tempValue = value;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(content);
            EditorGUILayout.LabelField(new GUIContent(value, value), GUILayout.MaxWidth(40));
            if (GUILayout.Button("•", EditorStyles.miniButton, GUILayout.Width(14)))
            {
                tempValue = EditorUtility.OpenFilePanel("Select file path", directory, extension);
            }
            EditorGUILayout.EndHorizontal();

            return tempValue;
        }

        public static string FolderField(GUIContent content, string value, string folder = "", string defaultName = "")
        {
            string tempValue = value;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(content);
            EditorGUILayout.LabelField(new GUIContent(value, value), GUILayout.MaxWidth(40));
            if (GUILayout.Button("•", EditorStyles.miniButton, GUILayout.Width(14)))
            {
                tempValue = EditorUtility.OpenFolderPanel("Select folder path", folder, defaultName);
            }
            EditorGUILayout.EndHorizontal();

            return tempValue;
        }

        public static bool ChangedToggle(ref bool variable, GUIContent content)
        {
            bool value = EditorGUILayout.Toggle(content, variable);
            if (value != variable)
            {
                variable = value;

                return true;
            }

            return false;
        }

        public static bool ChangedFoldout(ref bool variable, GUIContent content)
        {
            bool value = EditorGUILayout.Foldout(variable, content, true);
            if (value != variable)
            {
                variable = value;

                return true;
            }

            return false;
        }

        public static Type TypeField(string content, Type type, Type assemblyType = null)
        {
            if (assemblyType == null)
                assemblyType = typeof(MonoBehaviour);

            Assembly assembly = Assembly.GetAssembly(assemblyType);
            Type[] types = assembly.GetTypes();
            string[] variableNames = types.Select(x => x.Name).ToArray();

            int selectedItem = Array.FindIndex(variableNames, x => x == type.Name);

            selectedItem = EditorGUILayout.Popup(content, selectedItem, variableNames);

            return types[selectedItem];
        }

        public static string FieldNameLayout(Type type, ref int popupIndex, string label)
        {
            string[] variableNames = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(x => x.Name).ToArray();

            popupIndex = EditorGUILayout.Popup(label, popupIndex, variableNames);

            return variableNames[popupIndex];
        }

        public static string FieldName(Rect rect, Type type, ref int popupIndex, string label)
        {
            string[] variableNames = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(x => x.Name).ToArray();

            popupIndex = EditorGUI.Popup(rect, label, popupIndex, variableNames);

            return variableNames[popupIndex];
        }

        public static void ShowList(SerializedProperty list, Action<SerializedProperty> action)
        {
            if (!list.isArray)
            {
                EditorGUILayout.HelpBox(list.name + " is neither an array nor a list!", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));

            for (int i = 0; i < list.arraySize; i++)
            {
                action(list.GetArrayElementAtIndex(i));
            }

            if (GUILayout.Button("Add", EditorStyles.miniButton))
            {
                list.arraySize += 1;
            }
        }

        public static object UniversalField(object value, Type type, string title = "")
        {
            if (type == typeof(string))
            {
                return EditorGUILayout.TextField(new GUIContent(title), (string)value);
            }
            else if (type == typeof(bool))
            {
                return EditorGUILayout.Toggle(new GUIContent(title), (bool)value);
            }
            else if (type == typeof(int))
            {
                return EditorGUILayout.IntField(new GUIContent(title), (int)value);
            }
            else if (type == typeof(float))
            {
                return EditorGUILayout.FloatField(new GUIContent(title), (System.Single)value);
            }
            else if (type == typeof(Type))
            {
                return EditorGUILayoutCustom.TypeField(title, (Type)value);
            }
            else if (type == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(new GUIContent(title), (Vector2)value);
            }
            else if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(new GUIContent(title), (Vector3)value);
            }
            else if (type.IsEnum)
            {
                return EditorGUILayout.EnumPopup(new GUIContent(title), (Enum)value);
            }
            else if (type.IsSubclassOf(typeof(Object)))
            {
                return EditorGUILayout.ObjectField(new GUIContent(title), (Object)value, type, true);
            }
            else if (type.IsSerializable && !type.IsArray && !type.IsGenericType && type != typeof(object))
            {
                foreach (var property in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.IsPublic || x.GetCustomAttributes(typeof(SerializeField), false).Length > 0))
                {
                    EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(property.Name) + ": ", GUILayout.ExpandWidth(true));

                    try
                    {
                        property.SetValue(value, Convert.ChangeType(UniversalField(property.GetValue(value), property.FieldType), property.FieldType));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }
                }

                return value;
            }

            return null;
        }

        public static void DrawCompileWindow(Rect editorRect)
        {
            if (EditorApplication.isCompiling)
            {
                Rect rect = new Rect(editorRect.x, editorRect.y, editorRect.width, editorRect.height);
                GUI.Box(rect, "Compiling..", EditorCustomStyles.boxCompiling);
            }
        }

        public static void DrawScript<T>(T script) where T : MonoBehaviour
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(script), typeof(T), false);
            GUI.enabled = true;
        }

        public static void LineSpacer()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.Height(16), GUILayout.ExpandWidth(true));
        }

        public static Rect BeginBoxGroup(string title)
        {
            Rect rect = EditorGUILayout.BeginVertical(EditorCustomStyles.boxGroupBackground);

            if(!string.IsNullOrEmpty(title))
            {
                Header(rect, title);
            }

            EditorGUILayout.BeginVertical(EditorCustomStyles.boxContent);

            return rect;
        }

        public static bool BeginExpandBoxGroup(string title, bool value)
        {
            Rect rect = EditorGUILayout.BeginVertical(EditorCustomStyles.boxGroupBackground);

            bool expandValue = HeaderExpand(rect, title, value, EditorCustomStyles.foldoutArrowDown, EditorCustomStyles.foldoutArrowRight);

            EditorGUILayout.BeginVertical(value ? EditorCustomStyles.boxContent : GUIStyle.none);

            return expandValue;
        }

        public static bool BeginToggleBoxGroup(string title, bool value)
        {
            Rect rect = EditorGUILayout.BeginVertical(EditorCustomStyles.boxGroupBackground);

            bool expandValue = HeaderToggle(rect, title, value);

            EditorGUILayout.BeginVertical(value ? EditorCustomStyles.boxContent : GUIStyle.none);

            return expandValue;
        }

        public static bool BeginButtonBoxGroup(string title, GUIContent buttonContent, GUIStyle buttonStyle, bool disableState)
        {
            Rect rect = EditorGUILayout.BeginVertical(EditorCustomStyles.boxGroupBackground);

            bool isClicked = HeaderButton(rect, title, buttonContent, buttonStyle, disableState);

            EditorGUILayout.BeginVertical(EditorCustomStyles.boxContent);

            return isClicked;
        }

        public static void BeginMenuBoxGroup(string title, GenericMenu genericMenu)
        {
            Rect rect = EditorGUILayout.BeginVertical(EditorCustomStyles.boxGroupBackground);

            GUILayout.Space(30);

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, 30);

            EditorGUI.LabelField(headerRect, title, EditorCustomStyles.boxHeader);

            GUIStyle buttonStyle = EditorCustomStyles.buttonHover;
            GUIContent buttonContent = EditorCustomStyles.menuContent;

            Vector2 buttonSize = buttonStyle.CalcSize(buttonContent);
            float sizeMultiplier = buttonSize.y / 18;

            buttonSize /= sizeMultiplier;

            float yOffset = (headerRect.height - buttonSize.y) / 2;

            Rect buttonRect = new Rect(headerRect.x + headerRect.width - 8 - buttonSize.x, headerRect.y + yOffset, buttonSize.x, buttonSize.y);
            if (GUI.Button(buttonRect, buttonContent, buttonStyle))
            {
                genericMenu.ShowAsContext();
            }

            EditorGUILayout.BeginVertical(EditorCustomStyles.boxContent);
        }

        public static void EndBoxGroup()
        {
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        public static bool BeginFoldoutBoxGroup(bool value, string title)
        {
            GUIStyle guiStyle = EditorCustomStyles.box;

            Rect blockRect = EditorGUILayout.BeginVertical(value ? guiStyle : GUIStyle.none, GUILayout.MinHeight(30));

            Rect headerRect = new Rect(blockRect.x, blockRect.y, blockRect.width, 30);

            EditorGUI.LabelField(headerRect, title, EditorCustomStyles.boxHeader);
            EditorGUI.LabelField(new Rect(headerRect.x + headerRect.width - 24, headerRect.y + 7, 14, 14), value ? EditorCustomStyles.foldoutArrowDown : EditorCustomStyles.foldoutArrowRight);

            GUILayout.Space(30);

            if (GUI.Button(headerRect, GUIContent.none, GUIStyle.none))
            {
                value = !value;
            }

            return value;
        }

        public static bool BeginFoldoutBoxGroup(bool value, string title, out Rect foldoutRect)
        {
            Rect blockRect = EditorGUILayout.BeginVertical(EditorCustomStyles.box);

            GUI.Box(new Rect(blockRect.x - 12, blockRect.y, blockRect.width + 12, 30), GUIContent.none);

            foldoutRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(14));
            bool expandValue = EditorGUILayout.Foldout(value, title);

            EditorGUILayout.EndHorizontal();

            if(value)
            {
                GUILayout.Space(12);
            }

            if (GUI.Button(foldoutRect, GUIContent.none, GUIStyle.none))
            {
                expandValue = !expandValue;
            }

            return expandValue;
        }

        public static void EndFoldoutBoxGroup()
        {
            EditorGUILayout.EndVertical();
        }

        public static bool HeaderToggle(string title, bool value)
        {
            GUILayout.Space(4);

            Rect headerRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true), GUILayout.Height(24));

            EditorGUI.LabelField(headerRect, title, EditorCustomStyles.boxHeader);

            value = EditorGUI.Toggle(new Rect(headerRect.x + headerRect.width - 40, headerRect.y + 4, 32, 16), value, EditorCustomStyles.Skin.toggle);

            GUILayout.Space(24);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);

            if (GUI.Button(headerRect, GUIContent.none, GUIStyle.none))
            {
                value = !value;
            }

            return value;
        }

        public static bool HeaderToggle(Rect rect, string title, bool value)
        {
            GUILayout.Space(30);

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, 30);

            EditorGUI.LabelField(headerRect, title, EditorCustomStyles.boxHeader);

            value = EditorGUI.Toggle(new Rect(headerRect.x + headerRect.width - 38, headerRect.y + 7, 32, 16), value, EditorCustomStyles.Skin.toggle);

            if (GUI.Button(headerRect, GUIContent.none, GUIStyle.none))
            {
                value = !value;
            }

            return value;
        }

        public static void Header(Rect rect, string title)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, 30), title, EditorCustomStyles.boxHeader);

            GUILayout.Space(30);
        }

        public static void Header(string title)
        {
            GUILayout.Space(4);

            EditorGUILayout.LabelField(title, EditorCustomStyles.boxHeader, GUILayout.ExpandHeight(true), GUILayout.Height(24));

            GUILayout.Space(4);
        }

        public static bool HeaderExpand(Rect rect, string title, bool value, GUIContent activeContent, GUIContent disableContent)
        {
            GUILayout.Space(30);

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, 30);

            EditorGUI.LabelField(headerRect, new GUIContent(title, value ? EditorCustomStyles.foldoutArrowDown.image : EditorCustomStyles.foldoutArrowRight.image), EditorCustomStyles.boxHeader);

            if (GUI.Button(headerRect, GUIContent.none, GUIStyle.none))
            {
                value = !value;
            }

            return value;
        }

        public static bool HeaderExpand(string title, bool value, GUIContent activeContent, GUIContent disableContent)
        {
            GUILayout.Space(4);

            Rect headerRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true), GUILayout.Height(24));

            EditorGUI.LabelField(headerRect, title, EditorCustomStyles.boxHeader);

            float offsetY = (headerRect.height - 14) / 2;

            EditorGUI.LabelField(new Rect(headerRect.x + headerRect.width - 24, headerRect.y + offsetY, 14, 14), value ? EditorCustomStyles.foldoutArrowDown : EditorCustomStyles.foldoutArrowRight);

            GUILayout.Space(24);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);

            if (GUI.Button(headerRect, GUIContent.none, GUIStyle.none))
            {
                value = !value;
            }

            return value;
        }

        public static bool HeaderButton(Rect rect, string title, GUIContent buttonContent, GUIStyle buttonStyle, bool disableState)
        {
            bool isClicked = false;

            GUILayout.Space(30);

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, 30);

            EditorGUI.LabelField(headerRect, title, EditorCustomStyles.boxHeader);

            using (new EditorGUI.DisabledScope(disableState))
            {
                Vector2 buttonSize = buttonStyle.CalcSize(buttonContent);
                float sizeMultiplier = buttonSize.y / 18;

                buttonSize /= sizeMultiplier;

                float yOffset = (headerRect.height - buttonSize.y) / 2;

                Rect buttonRect = new Rect(headerRect.x + headerRect.width - 8 - buttonSize.x, headerRect.y + yOffset, buttonSize.x, buttonSize.y);
                if (GUI.Button(buttonRect, buttonContent, buttonStyle))
                {
                    isClicked = true;
                }
            }

            return isClicked;
        }

        public static bool HeaderButton(string title, GUIContent buttonContent, GUIStyle buttonStyle, bool disableState)
        {
            bool isClicked = false;

            GUILayout.Space(4);

            Rect headerRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true), GUILayout.Height(24));

            EditorGUI.LabelField(headerRect, title, EditorCustomStyles.boxHeader);

            using (new EditorGUI.DisabledScope(disableState))
            {
                Vector2 buttonSize = buttonStyle.CalcSize(buttonContent);
                float sizeMultiplier = buttonSize.y / 18;

                buttonSize /= sizeMultiplier;

                if (GUI.Button(new Rect(headerRect.x + headerRect.width - 8 - buttonSize.x, headerRect.y + 3, buttonSize.x, buttonSize.y), buttonContent, buttonStyle))
                {
                    isClicked = true;
                }
            }

            GUILayout.Space(24);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);

            return isClicked;
        }

        public static void DrawAllProperties(SerializedObject serializedObject, bool skipFirst = true)
        {
            var prop = serializedObject.GetIterator();

            if(skipFirst)
            {
                //Ignore script field
                prop.NextVisible(true);
            }

            while (prop.NextVisible(false))
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }
    }
}