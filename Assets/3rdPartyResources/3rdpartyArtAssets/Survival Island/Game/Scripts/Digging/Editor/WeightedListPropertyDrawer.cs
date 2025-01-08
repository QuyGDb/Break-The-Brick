using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(WeightedList<>), true)]
    public class WeightedListPropertyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty arrayProperty = property.FindPropertyRelative("items");
            if (arrayProperty == null || !arrayProperty.isArray)
                return;

            SerializedProperty arraySize = property.FindPropertyRelative("items.Array.size");


            EditorGUI.BeginProperty(position, label, arraySize);

            EditorGUI.PropertyField(position, arrayProperty, label);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int height = 18;

            SerializedProperty arrayProperty = property.FindPropertyRelative("items");
            if(arrayProperty.isExpanded)
            {
                SerializedProperty arraySizeProperty = property.FindPropertyRelative("items.Array.size");

                if (arraySizeProperty != null)
                {
                    int arraySize = arraySizeProperty.intValue;
                    if(arraySize > 0)
                    {
                        height += 20 * arraySizeProperty.intValue;
                    }
                    else
                    {
                        height += 20;
                    }
                }

                height += 32;
            }

            return height;
        }
    }
}
