using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(IAPSettings))]
    public class IAPSettingsEditor : Editor
    {
        private readonly Vector2 ENUM_WINDOW_SIZE = new Vector2(300, 320);
        private const string ENUM_WINDOW_TITLE = "[IAP Manager]: Product Type";
        private const string TYPES_FILE_NAME = "MD_ProductKeyType";
        private const string REGEX_ID_PATTERN = @"^([a-z][a-z0-9]*[_\.]{1}[a-z0-9]+)+$";
        private const string ID_EXPLANATION = "The ID may only contain lowercase letters, numbers, underscores, or periods. Its recomended to use the reverse domain name style (for example, com.companyname.application.productid) when creating your identifier.";
        private const string ID_EMPTY = "ID can`t be empty.";
        private SerializedProperty storeItemsProperty;
        private SerializedProperty selectedProperty;

        private IEnumerable<SerializedProperty> settingsProperties;

        private GUIContent settingsContent;
        private GUIContent warningContent;

        private Regex regex;
        private Color backupColor;

        private GenericMenu menu;

        protected void OnEnable()
        {
            regex = new Regex(REGEX_ID_PATTERN);

            //obtain store items
            storeItemsProperty = serializedObject.FindProperty("storeItems");

            settingsProperties = serializedObject.GetPropertiesByGroup("Settings");

            settingsContent = new GUIContent(EditorCustomStyles.GetIcon("icon_menu"));
            warningContent = new GUIContent(EditorCustomStyles.GetIcon("icon_warning"));

            menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Product"), false, () => AddNewItem());
            menu.AddItem(new GUIContent("Open Types Manager"), false, () => OpenTypesWindow());
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayoutCustom.BeginBoxGroup("Settings");

            foreach (SerializedProperty property in settingsProperties)
            {
                EditorGUILayout.PropertyField(property);
            }

            EditorGUILayoutCustom.EndBoxGroup();

            EditorGUILayoutCustom.BeginMenuBoxGroup("Products", menu);

            if (storeItemsProperty.arraySize > 0)
            {
                SerializedProperty tempSerializedProperty;
                string tempTitle = string.Empty;
                bool adroidIdValid;
                bool iosIdValid;

                for (int i = 0; i < storeItemsProperty.arraySize; i++)
                {
                    tempSerializedProperty = storeItemsProperty.GetArrayElementAtIndex(i);
                    SerializedProperty androidIDProperty = tempSerializedProperty.FindPropertyRelative("androidID");
                    SerializedProperty iOSIDProperty = tempSerializedProperty.FindPropertyRelative("iOSID");
                    SerializedProperty productKeyTypeProperty = tempSerializedProperty.FindPropertyRelative("productKeyType");
                    SerializedProperty productTypeProperty = tempSerializedProperty.FindPropertyRelative("productType");

                    ProductKeyType productKeyType = (ProductKeyType)productKeyTypeProperty.intValue;
                    ProductType productType = (ProductType)productTypeProperty.intValue;

                    tempTitle = string.Format("{0} ({1})", productKeyType, productType);
                    if (string.IsNullOrEmpty(tempTitle))
                        tempTitle = "Draft";

                    Rect rect;
                    adroidIdValid = regex.IsMatch(androidIDProperty.stringValue);
                    iosIdValid = regex.IsMatch(iOSIDProperty.stringValue);
                    tempSerializedProperty.isExpanded = EditorGUILayoutCustom.BeginFoldoutBoxGroup(tempSerializedProperty.isExpanded, tempTitle, out rect);

                    if(!(adroidIdValid && iosIdValid))
                    {
                        backupColor = GUI.color; 

                        GUI.color = Color.red;
                        EditorGUI.LabelField(new Rect(rect.x + rect.width - 20, rect.y, 18, 18), warningContent);
                        GUI.color = backupColor;
                    }

                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    {
                        tempSerializedProperty.isExpanded = !tempSerializedProperty.isExpanded;
                    }

                    if (tempSerializedProperty.isExpanded)
                    {
                        backupColor = GUI.backgroundColor;

                        EditorGUI.BeginChangeCheck();

                        if (!adroidIdValid)
                        {
                            GUI.backgroundColor = Color.red;
                            EditorGUILayout.PropertyField(androidIDProperty, new GUIContent("Android ID"));
                            GUI.backgroundColor = backupColor;
                            
                            if(androidIDProperty.stringValue.Length == 0)
                            {
                                EditorGUILayout.HelpBox(ID_EMPTY, MessageType.Error);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox(ID_EXPLANATION, MessageType.Error);
                            }
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(androidIDProperty, new GUIContent("Android ID"));
                        }



                        if (!iosIdValid)
                        {
                            GUI.backgroundColor = Color.red;
                            EditorGUILayout.PropertyField(iOSIDProperty, new GUIContent("iOS ID"));
                            GUI.backgroundColor = backupColor;

                            if (iOSIDProperty.stringValue.Length == 0)
                            {
                                EditorGUILayout.HelpBox(ID_EMPTY, MessageType.Error);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox(ID_EXPLANATION, MessageType.Error);
                            }

                        }
                        else
                        {
                            EditorGUILayout.PropertyField(iOSIDProperty, new GUIContent("iOS ID"));
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            androidIDProperty.stringValue = androidIDProperty.stringValue.Trim();
                            iOSIDProperty.stringValue = iOSIDProperty.stringValue.Trim();
                        }
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(productKeyTypeProperty);
                        if (GUILayout.Button(settingsContent, GUILayout.Width(18), GUILayout.Height(18)))
                        {
                            OpenTypesWindow();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(tempSerializedProperty.FindPropertyRelative("productType"));

                        serializedObject.ApplyModifiedProperties();

                        GUILayout.Space(2);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Delete", GUILayout.Width(80)))
                        {
                            if (EditorUtility.DisplayDialog("Remove", "Are you sure you want to remove this item?", "Remove", "Cancel"))
                            {
                                DeleteItem(i);

                                GUIUtility.ExitGUI();

                                Event.current.Use();

                                return;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayoutCustom.EndFoldoutBoxGroup();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Products list is empty!", MessageType.Info);
            }

            EditorGUILayoutCustom.EndBoxGroup();

            GUILayout.FlexibleSpace();

            serializedObject.ApplyModifiedProperties();
        }

        private void OpenTypesWindow()
        {
            IAPTypesWindow window = (IAPTypesWindow)EditorWindow.GetWindow(typeof(IAPTypesWindow), true);
            window.titleContent = new GUIContent(ENUM_WINDOW_TITLE);
            window.minSize = ENUM_WINDOW_SIZE;
            window.maxSize = ENUM_WINDOW_SIZE;
            window.ShowUtility();
        }

        private void DeleteItem(int index)
        {
            storeItemsProperty.RemoveFromVariableArrayAt(index);

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateItemSelection(int i)
        {
            selectedProperty = storeItemsProperty.GetArrayElementAtIndex(i);
            selectedProperty.isExpanded = true;
        }

        private void AddNewItem()
        {
            int newItemIndex = storeItemsProperty.arraySize;

            storeItemsProperty.arraySize++;

            SerializedProperty newSerializedProperty = storeItemsProperty.GetArrayElementAtIndex(newItemIndex);
            newSerializedProperty.FindPropertyRelative("androidID").stringValue = "";
            newSerializedProperty.FindPropertyRelative("iOSID").stringValue = "";
            newSerializedProperty.FindPropertyRelative("productType").enumValueIndex = 0;

            serializedObject.ApplyModifiedProperties();

            UpdateItemSelection(storeItemsProperty.arraySize - 1);
        }

        private class IAPTypesWindow : EditorWindow
        {
            private List<EnumData> enumDataList;

            private string newTypeName;
            private Rect itemTypeRect;
            private string itemTypeNewName;
            private int selectedItemType;

            private string filePath;

            private Vector2 scrollView = Vector2.zero;

            protected void OnEnable()
            {
                //get enumData
                enumDataList = new List<EnumData>();
                int[] values = (int[])System.Enum.GetValues(typeof(ProductKeyType));
                string[] names = System.Enum.GetNames(typeof(ProductKeyType));

                for (int i = 0; i < values.Length; i++)
                {
                    enumDataList.Add(new EnumData(values[i], names[i]));
                }

                // finding path for enum generation         
                MonoScript iapTypesScript = EditorUtils.GetAssetByName<MonoScript>(TYPES_FILE_NAME);
                filePath = EditorUtils.projectFolderPath + AssetDatabase.GetAssetPath(iapTypesScript);

                selectedItemType = -1;
                newTypeName = "";
                itemTypeNewName = "";
            }

            private void OnGUI()
            {
                EditorGUILayout.BeginVertical(EditorCustomStyles.Skin.box);
                EditorGUILayout.BeginHorizontal();

                newTypeName = EditorGUILayout.TextField(newTypeName);

                bool uniqueName = enumDataList.FindIndex(x => x.name == newTypeName) == -1;

                if (GUILayout.Button("Add") && newTypeName != "" && uniqueName)
                {
                    enumDataList.Add(new EnumData(enumDataList.Count, newTypeName));

                    RegenerateEnum();

                    newTypeName = "";
                    GUI.FocusControl(null);
                }

                EditorGUILayout.EndHorizontal();

                if (!uniqueName)
                {
                    EditorGUILayout.HelpBox("Product type name should be unique", MessageType.Warning);
                }
                EditorGUILayout.EndVertical();

                scrollView = EditorGUILayout.BeginScrollView(scrollView);
                EditorGUILayout.BeginVertical(EditorCustomStyles.Skin.box);

                //Draw existing items
                if(enumDataList.Count > 0)
                {
                    for (int i = 0; i < enumDataList.Count; i++)
                    {
                        itemTypeRect = EditorGUILayout.BeginVertical(EditorCustomStyles.Skin.box);

                        if (i != selectedItemType)
                        {
                            EditorGUILayout.LabelField(enumDataList[i].name);
                        }
                        else
                        {
                            itemTypeNewName = EditorGUILayout.TextField(itemTypeNewName);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("Update name", GUILayout.Width(100)))
                            {
                                enumDataList[i].name = itemTypeNewName;
                                RegenerateEnum();
                            }

                            if (GUILayout.Button("Delete", GUILayout.Width(70)))
                            {
                                if (EditorUtility.DisplayDialog("Remove this item?", "Are you sure you want to remove " + enumDataList[i].name + " item?", "Remove", "Cancel"))
                                {
                                    enumDataList.RemoveAt(i);
                                    RegenerateEnum();

                                    GUI.FocusControl(null);
                                    selectedItemType = -1;
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        if (GUI.Button(itemTypeRect, GUIContent.none, GUIStyle.none))
                        {
                            if (i == selectedItemType)
                            {
                                selectedItemType = -1;
                            }
                            else
                            {
                                itemTypeNewName = enumDataList[i].name;
                                selectedItemType = i;
                            }
                        }

                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Enum is empty!", MessageType.Info);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }

            private void RegenerateEnum()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("namespace Watermelon");
                sb.AppendLine("{");
                sb.AppendLine("    public enum ProductKeyType");
                sb.AppendLine("    {");
                for (int i = 0; i < enumDataList.Count; i++)
                {
                    sb.AppendLine("        " + enumDataList[i].name + " = " + enumDataList[i].value + ",");
                }
                sb.AppendLine("    }");
                sb.AppendLine("}");

                Debug.Log("FilePath:" + filePath);
                File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);

                AssetDatabase.Refresh();
            }

            private class EnumData
            {
                public int value;
                public string name;

                public EnumData(int value, string name)
                {
                    this.value = value;
                    this.name = name;
                }
            }
        }
    }
}

// -----------------
// IAP Manager v 1.1
// -----------------