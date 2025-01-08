using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public class PlayerSkinPickerWindow : EditorWindow
    {
        private SerializedProperty property;
        private PlayerSkinsDatabase database;

        private Vector2 scrollView;

        private GUIStyle boxStyle;

        private string selectedID;

        private static PlayerSkinPickerWindow window;

        public static void PickSkin(SerializedProperty property, PlayerSkinsDatabase database)
        {
            if(window != null)
            {
                window.Close();
            }

            window = EditorWindow.GetWindow<PlayerSkinPickerWindow>(true);
            window.titleContent = new GUIContent("Skins Picker");
            window.property = property;
            window.database = database;
            window.selectedID = property.stringValue;
            window.scrollView = window.CalculateScrollView();
            window.Show();
        }

        private Vector2 CalculateScrollView()
        {
            if(string.IsNullOrEmpty(selectedID))
                return Vector2.zero;

            int elementIndex = System.Array.FindIndex(database.Skins, x => x.ID == selectedID);

            return new Vector2(0, elementIndex * 65);
        }

        private void OnEnable()
        {
            boxStyle = new GUIStyle(EditorCustomStyles.Skin.box);
            boxStyle.overflow = new RectOffset(0, 0, 0, 0);
        }

        private void OnGUI()
        {
            if(property == null)
            {
                Close();

                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(-14); 

            scrollView = EditorGUILayout.BeginScrollView(scrollView);

            //Draw existing items
            if (!database.Skins.IsNullOrEmpty())
            {
                for (int i = 0; i < database.Skins.Length; i++)
                {
                    Color defaultColor = GUI.backgroundColor;

                    if (selectedID == database.Skins[i].ID)
                    {
                        GUI.backgroundColor = Color.yellow;
                    }

                    Rect elementRect = EditorGUILayout.BeginVertical(boxStyle, GUILayout.MinHeight(58), GUILayout.ExpandHeight(false));

                    GUILayout.Space(58);

                    if (GUI.Button(elementRect, GUIContent.none, GUIStyle.none))
                    {
                        if (selectedID == database.Skins[i].ID)
                        {
                            Close();

                            return;
                        }

                        selectedID = database.Skins[i].ID;

                        SelectSkin(selectedID);
                    }

                    using (new EditorGUI.DisabledScope(disabled: true))
                    {
                        elementRect.x += 4;
                        elementRect.width -= 8;
                        elementRect.y += 4;
                        elementRect.height -= 8;

                        DrawElement(elementRect, database.Skins[i], i);
                    }

                    EditorGUILayout.EndVertical();

                    GUI.backgroundColor = defaultColor;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Skins list is empty!", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void SelectSkin(string ID)
        {
            property.serializedObject.Update();
            property.stringValue = ID;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void DrawElement(Rect rect, PlayerSkinData skinData, int index)
        {
            float defaultYPosition = rect.y;

            rect.width -= 60;

            Rect propertyPosition = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(propertyPosition, string.Format("Skin #{0}", (index + 1)));

            propertyPosition.y += EditorGUIUtility.singleLineHeight + 2;

            EditorGUI.LabelField(propertyPosition, skinData.ID);

            propertyPosition.y += EditorGUIUtility.singleLineHeight + 2;

            EditorGUI.ObjectField(propertyPosition, skinData.Prefab, typeof(GameObject), false);

            Rect boxRect = new Rect(rect.x + propertyPosition.width + 2, defaultYPosition, 58, 58);
            GUI.Box(boxRect, GUIContent.none);

            Object prefabObject = skinData.Prefab;
            if (prefabObject != null)
            {
                Texture2D previewTexture = AssetPreview.GetAssetPreview(prefabObject);
                if (previewTexture != null)
                {
                    GUI.DrawTexture(new Rect(boxRect.x + 2, boxRect.y + 2, 55, 55), previewTexture);
                }
            }
            else
            {
                GUI.DrawTexture(new Rect(boxRect.x + 2, boxRect.y + 2, 55, 55), EditorCustomStyles.GetMissingIcon());
            }
        }
    }
}
