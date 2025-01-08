using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public sealed class GroupGUIRenderer : GUIRenderer
    {
        private List<GUIRenderer> renderers;
        private PropertyGrouper propertyGrouper;
        private GroupAttribute groupAttribute;
        private WatermelonEditor editor;

        public string GroupID => groupAttribute.ID;
        public string ParentPath { get; private set; }

        public GroupGUIRenderer(WatermelonEditor editor, GroupAttribute groupAttribute, List<GUIRenderer> renderers)
        {
            this.editor = editor;
            this.renderers = renderers;
            this.groupAttribute = groupAttribute;

            ParentPath = PropertyUtility.GetSubstringBeforeLastSlash(groupAttribute.ID);

            propertyGrouper = CustomAttributesDatabase.GetGroupAttribute(groupAttribute.GetType());

            Order = groupAttribute.Order;

            IsVisible = renderers.IsAnyObjectVisible();

            if (groupAttribute.GetType() == typeof(BoxFoldoutAttribute))
            {
                BoxFoldoutAttribute foldoutAttribute = (BoxFoldoutAttribute)groupAttribute;

                // Override default state
                editor.GetFoldout(groupAttribute.ID, foldoutAttribute.DefaultState);
            }
        }

        public void AddRenderer(GUIRenderer renderer)
        {
            renderers.Add(renderer);
        }

        public override void OnGUI()
        {
            if (!IsVisible) return;

            propertyGrouper.BeginGroup(editor, groupAttribute.ID, groupAttribute.Label);

            if (propertyGrouper.DrawRenderers(editor, groupAttribute.ID))
            {
                foreach (GUIRenderer renderer in renderers)
                {
                    renderer.OnGUI();
                }
            }

            propertyGrouper.EndGroup();
        }

        public override void OnGUIChanged()
        {
            foreach (GUIRenderer renderer in renderers)
            {
                renderer.OnGUIChanged();
            }

            IsVisible = renderers.IsAnyObjectVisible();
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