using System;
using System.Collections.Generic;
using System.Reflection;

namespace Watermelon
{
    public sealed class UnwrapNestedGUIRenderer : GUIRenderer
    {
        private GUIRenderer[] renderers;

        private FieldInfo fieldInfo;
        private object targetObject;
        private WatermelonEditor editor;

        private PropertyCondition propertyCondition;

        private List<Type> nestedTypes;

        public UnwrapNestedGUIRenderer(WatermelonEditor editor, FieldInfo fieldInfo, GUIRenderer[] renderers, object targetObject, List<Type> nestedTypes)
        {
            this.editor = editor;
            this.targetObject = targetObject;
            this.fieldInfo = fieldInfo;
            this.nestedTypes = nestedTypes;
            this.renderers = renderers;

            TabAttribute = (TabAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(TabAttribute));
            GroupAttribute = (GroupAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(GroupAttribute));

            // Check if visible
            ConditionAttribute showIfAttribute = PropertyUtility.GetAttribute<ConditionAttribute>(fieldInfo);

            if (showIfAttribute != null)
            {
                propertyCondition = CustomAttributesDatabase.GetConditionAttribute(showIfAttribute.GetType());

                UpdateVisabilityState();
            }
            else
            {
                IsVisible = renderers.IsAnyObjectVisible();
            }
        }

        public override void OnGUI()
        {
            if (!IsVisible) return;

            foreach (GUIRenderer renderer in renderers)
            {
                renderer.OnGUI();
            }
        }

        public override void OnGUIChanged()
        {
            foreach (GUIRenderer renderer in renderers)
            {
                renderer.OnGUIChanged();
            }

            UpdateVisabilityState();
        }

        private void UpdateVisabilityState()
        {
            if (propertyCondition != null)
            {
                // Check if visible
                if (propertyCondition.CanBeDrawn(editor, fieldInfo, targetObject, nestedTypes))
                {
                    IsVisible = renderers.IsAnyObjectVisible();
                }
                else
                {
                    IsVisible = false;
                }
            }
            else
            {
                IsVisible = renderers.IsAnyObjectVisible();
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