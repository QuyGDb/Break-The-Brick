using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(WeightedList<>.WeightedItem<>), true)]
    public class WeightedListItemPropertyDrawer : UnityEditor.PropertyDrawer
    {
        private const int ColumnCount = 3;
        private const int GapSize = 4;
        private const int GapCount = ColumnCount - 1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float x = position.x;
            float y = position.y;
            float width = (position.width - GapCount * GapSize) / ColumnCount;
            float height = EditorGUIUtility.singleLineHeight;
            float offset = width + GapSize;

            SerializedProperty weightProperty = property.FindPropertyRelative("weight");

            EditorGUI.PrefixLabel(new Rect(x, y, width, height), new GUIContent(property.displayName));
            EditorGUI.PropertyField(new Rect(x + offset, y, width + 30, height), property.FindPropertyRelative("item"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(x + offset + offset + 30, y, width - 30, height), weightProperty, GUIContent.none);

            if (weightProperty.intValue < 0)
                weightProperty.intValue = 1;

            EditorGUI.EndProperty();
        }
    }
}
