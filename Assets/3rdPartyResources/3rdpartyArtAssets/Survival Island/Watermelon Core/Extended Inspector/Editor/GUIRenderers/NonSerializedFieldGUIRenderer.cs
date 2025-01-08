using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Watermelon
{
    public sealed class NonSerializedFieldGUIRenderer : GUIRenderer
    {
        private FieldInfo fieldInfo;
        private GUIContent labelContent;
        private object target;

        public NonSerializedFieldGUIRenderer(FieldInfo fieldInfo, Object target)
        {
            this.fieldInfo = fieldInfo;
            this.target = target;

            TabAttribute = (TabAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(TabAttribute));
            GroupAttribute = (GroupAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(GroupAttribute));

            OrderAttribute orderAttribute = (OrderAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(OrderAttribute));
            if (orderAttribute != null)
            {
                Order = orderAttribute.Order;
            }
            else
            {
                Order = GUIRenderer.ORDER_NON_SERIALIZED;
            }

            string label = fieldInfo.Name;

            LabelAttribute labelAttribute = (LabelAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(LabelAttribute));
            if (labelAttribute != null)
            {
                label = labelAttribute.Label;
            }

            labelContent = new GUIContent(label);
        }

        public override void OnGUI()
        {
            object value = fieldInfo.GetValue(target);

            if (value == null)
            {
                string warning = "Unsuported field type";

                EditorGUILayout.HelpBox(warning, MessageType.Warning);

                Debug.LogWarning(warning);

                return;
            }

            bool propertyStatus = false;

            LabelWidthAttribute labelWidthAttribute = PropertyUtility.GetAttribute<LabelWidthAttribute>(fieldInfo);
            if (labelWidthAttribute != null)
            {
                using (new LabelWidthScope(labelWidthAttribute.Width))
                {
                    propertyStatus = EditorDrawUtility.DrawLayoutField(value, labelContent);
                }
            }
            else
            {
                propertyStatus = EditorDrawUtility.DrawLayoutField(value, labelContent);
            }

            if(!propertyStatus)
            {
                string warning = "Unsuported field type";

                EditorGUILayout.HelpBox(warning, MessageType.Warning);

                Debug.LogWarning(warning);
            }
        }
    }
}

// -----------------
// Watermelon Editor v1.1
// -----------------

// Changelog
// v 1.1
// • Removed bottle icon
// • Added copy icon
// • Added Unique ID editor module
// • Added custom SceneSaving callback
// v 1.0
// • Basic logic