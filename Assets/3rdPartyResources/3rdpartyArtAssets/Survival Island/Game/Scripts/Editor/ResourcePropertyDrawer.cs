using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(Resource))]
    public class ResourcePropertyDrawer : UnityEditor.PropertyDrawer
    {
        private const string CURRENCY_PROPERTY_PATH = "currency";
        private const string AMOUNT_PROPERTY_PATH = "amount";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect temp = new Rect(position);
            temp.yMin += 2;
            temp.yMax -= 2;

            temp.width = (position.width / 2) - 4;
            SerializedProperty currencyProperty = property.FindPropertyRelative(CURRENCY_PROPERTY_PATH);
            SerializedProperty amountProperty = property.FindPropertyRelative(AMOUNT_PROPERTY_PATH);

            if (currencyProperty.hasMultipleDifferentValues)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.EnumPopup(temp, (CurrencyType)currencyProperty.intValue);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                currencyProperty.intValue = (int)((CurrencyType)EditorGUI.EnumPopup(temp, (CurrencyType)currencyProperty.intValue));
            }
            

            temp.x += temp.width + 8;

            if (amountProperty.hasMultipleDifferentValues)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.IntField(temp, amountProperty.intValue);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                amountProperty.intValue = EditorGUI.IntField(temp, amountProperty.intValue);
            }

            
        }
    }
}
