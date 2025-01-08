using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public class LabelWidthScope : GUI.Scope
    {
        private readonly float defaultWidth;

        public LabelWidthScope(float width)
        {
            defaultWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = width;
        }

        protected override void CloseScope()
        {
            EditorGUIUtility.labelWidth = defaultWidth;
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