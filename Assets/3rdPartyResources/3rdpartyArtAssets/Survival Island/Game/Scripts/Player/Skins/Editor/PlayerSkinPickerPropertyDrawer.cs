using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(PlayerSkinPickerAttribute))]
    public class PlayerSkinPickerPropertyDrawer : UnityEditor.PropertyDrawer
    {
        private PlayerSkinsDatabase skinsDatabase;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "Incorect property type!", MessageType.Error);

                EditorGUI.EndProperty();

                return;
            }

            if (skinsDatabase == null)
            {
                skinsDatabase = EditorUtils.GetAsset<PlayerSkinsDatabase>();
            }

            position.width -= 60;

            EditorGUI.LabelField(position, label);

            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;

            Rect propertyPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            if (skinsDatabase == null)
            {
                DrawBlock(propertyPosition, "Skins database is missing!", null, null);

                EditorGUI.EndProperty();

                return;
            }

            if (string.IsNullOrEmpty(property.stringValue))
            {
                DrawBlock(propertyPosition, "Skin isn't selected!", null, property);

                EditorGUI.EndProperty();

                return;
            }

            PlayerSkinData skinData = skinsDatabase.Skins.Find(x => x.ID == property.stringValue);
            if (skinData == null)
            {
                DrawBlock(propertyPosition, "Skin isn't selected!", null, property);

                EditorGUI.EndProperty();

                return;
            }

            DrawBlock(propertyPosition, property.stringValue, skinData.Prefab, property);

            EditorGUI.EndProperty();
        }

        private void DrawBlock(Rect propertyPosition, string idText, GameObject prefabObject, SerializedProperty property)
        {
            float defaultYPosition = propertyPosition.y;

            EditorGUI.LabelField(propertyPosition, "Selected skin:");

            using (new EditorGUI.DisabledScope(disabled: true))
            {
                propertyPosition.y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.LabelField(propertyPosition, idText);

                propertyPosition.y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.ObjectField(propertyPosition, prefabObject, typeof(GameObject), false);
            }

            Rect boxRect = new Rect(propertyPosition.x + propertyPosition.width + 2, defaultYPosition, 58, 58);

            GUI.Box(boxRect, GUIContent.none);

            if(prefabObject != null)
            {
                Texture2D previewTexture = AssetPreview.GetAssetPreview(prefabObject);
                if(previewTexture != null)
                {
                    GUI.DrawTexture(new Rect(boxRect.x + 2, boxRect.y + 2, 55, 55), previewTexture);
                }
            }
            else
            {
                GUI.DrawTexture(new Rect(boxRect.x + 2, boxRect.y + 2, 55, 55), EditorCustomStyles.GetMissingIcon());
            }

            if(property != null)
            {
                if (GUI.Button(new Rect(propertyPosition.x + propertyPosition.width + 5, defaultYPosition + 40, 53, 16), new GUIContent("Select")))
                {
                    PlayerSkinPickerWindow.PickSkin(property, skinsDatabase);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Mathf.Clamp(EditorGUIUtility.singleLineHeight * 3 + 2, 58, float.MaxValue);
        }
    }
}
