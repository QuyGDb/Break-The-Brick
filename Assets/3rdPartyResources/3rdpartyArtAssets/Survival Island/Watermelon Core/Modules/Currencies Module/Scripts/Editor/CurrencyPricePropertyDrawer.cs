using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(CurrencyPrice))]
    public class CurrencyPricePropertyDrawer : UnityEditor.PropertyDrawer
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

            SerializedProperty priceProperty = property.FindPropertyRelative("price");

            EditorGUI.PrefixLabel(new Rect(x, y, width, height), new GUIContent(property.displayName));
            EditorGUI.PropertyField(new Rect(x + offset, y, width, height), property.FindPropertyRelative("currencyType"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(x + offset + offset, y, width, height), priceProperty, GUIContent.none);

            if (priceProperty.intValue < 0)
                priceProperty.intValue = 0;

            EditorGUI.EndProperty();
        }
    }
}
