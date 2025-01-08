using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Editor Custom Styles", menuName = "Core/Editor/Editor Custom Styles")]
    public class EditorCustomStylesData : ScriptableObject
    {
        [SerializeField] GUISkin defaultGUISkin;
        [SerializeField] GUISkin proGUISkin;

        [Space]
        [SerializeField] Texture2D[] icons;

        [SerializeField] Texture2D missingIcon;
        public Texture2D MissingIcon => missingIcon;

        private Color defaultIconColor = Color.black;
        private Color darkIconColor = Color.white;

        public Color IconColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                    return darkIconColor;

                return defaultIconColor;
            }
        }

        public GUISkin Skin
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                    return proGUISkin;

                return defaultGUISkin;
            }
        }

        public Texture2D GetIcon(string name)
        {
            for(int i = 0; i < icons.Length; i++)
            {
                if (icons[i].name == name)
                    return icons[i];
            }

            return missingIcon;
        }
    }
}
