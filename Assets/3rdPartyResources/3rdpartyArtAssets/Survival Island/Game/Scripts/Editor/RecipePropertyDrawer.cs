using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(Recipe))]
    public class RecipePropertyDrawer : UnityEditor.PropertyDrawer
    {
        private const string COMPONENTS_PROPERTY_PATH = "components";
        private const string RESULT_PROPERTY_PATH = "resultResourceType";
        private const string TYPE_PROPERTY_PATH = "resourceType";
        private const string AMOUNT_PROPERTY_PATH = "amount";
        private const string ITEM_LABEL = "Ingredient ";
        private const string RESULT_LABEL = "Result";
        private SerializedProperty tempProperty;
        private ReorderableList reorderableList;
        private int labelWidth = 66;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            tempProperty = property;
            reorderableList = new ReorderableList(property.serializedObject, property.FindPropertyRelative(COMPONENTS_PROPERTY_PATH), true, true, true, true);
            reorderableList.drawHeaderCallback = DrawHeaderCallback;
            reorderableList.drawElementCallback = DrawElementCallback;
            return reorderableList.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            reorderableList.DoList(position);
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty cureentProperty = tempProperty.FindPropertyRelative(COMPONENTS_PROPERTY_PATH).GetArrayElementAtIndex(index);
            SerializedProperty amountProperty = cureentProperty.FindPropertyRelative(AMOUNT_PROPERTY_PATH);
            SerializedProperty typeProperty = cureentProperty.FindPropertyRelative(TYPE_PROPERTY_PATH);
            Rect temp = new Rect(rect);
            temp.width = labelWidth;
            EditorGUI.LabelField(temp, ITEM_LABEL);
            temp.x = labelWidth + 8 + rect.x;
            temp.width = ((rect.width - labelWidth - 12) / 2f);

            temp.yMin += 2;
            temp.yMax -= 2;
            typeProperty.intValue = (int)((CurrencyType)EditorGUI.EnumPopup(temp, (CurrencyType)typeProperty.intValue));
            temp.x += temp.width + 8;
            
            amountProperty.intValue = EditorGUI.IntField(temp, amountProperty.intValue);
        }

        private void DrawHeaderCallback(Rect rect)
        {
            Rect temp = new Rect(rect);
            temp.x += 12;
            temp.width = labelWidth;
            SerializedProperty resultProperty = tempProperty.FindPropertyRelative(RESULT_PROPERTY_PATH);
            GUI.Label(temp, RESULT_LABEL);
            temp.x = rect.x + labelWidth + 8;
            temp.xMax = rect.xMax;
            resultProperty.intValue = (int)((CurrencyType)EditorGUI.EnumPopup(temp, (CurrencyType)resultProperty.intValue));
        }
    }
}
