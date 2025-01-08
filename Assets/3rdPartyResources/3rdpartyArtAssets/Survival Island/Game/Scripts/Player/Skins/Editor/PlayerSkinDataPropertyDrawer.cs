using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(PlayerSkinData))]
    public class PlayerSkinDataPropertyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty prefabProperty = property.FindPropertyRelative("prefab");
            SerializedProperty idProperty = property.FindPropertyRelative("id");

            position.width -= 60;

            Rect propertyPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(propertyPosition, idProperty);

            propertyPosition.y += EditorGUIUtility.singleLineHeight + 2;

            EditorGUI.PropertyField(propertyPosition, prefabProperty);

            Rect boxRect = new Rect(position.x + propertyPosition.width + 2, position.y, 58, 58);
            GUI.Box(boxRect, GUIContent.none);

            Object prefabObject = prefabProperty.objectReferenceValue;
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

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Mathf.Clamp(EditorGUIUtility.singleLineHeight * 2 + 2, 58, float.MaxValue);
        }
    }
}
