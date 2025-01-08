using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    public sealed class HelpButtonGUIRenderer : GUIRenderer
    {
        private HelpButtonAttribute helpButtonAttribute;

        private GUIContent buttonContent;

        public HelpButtonGUIRenderer(HelpButtonAttribute helpButtonAttribute)
        {
            this.helpButtonAttribute = helpButtonAttribute;

            Order = GUIRenderer.ORDER_HELP_BUTTON;

            buttonContent = new GUIContent(helpButtonAttribute.Name, EditorCustomStyles.GetIcon("icon_link"), helpButtonAttribute.URL);
        }

        public override void OnGUI()
        {
            if (GUILayout.Button(buttonContent, EditorCustomStyles.button, GUILayout.Height(22)))
            {
                Application.OpenURL(helpButtonAttribute.URL);
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