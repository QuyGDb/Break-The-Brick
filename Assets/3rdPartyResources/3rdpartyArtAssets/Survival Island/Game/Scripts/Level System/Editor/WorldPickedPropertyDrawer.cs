using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(WorldPickerAttribute))]
    public class WorldPickedPropertyDrawer : UnityEditor.PropertyDrawer
    {
        private WorldsDatabase worldsDatabase;
        private GUIContent[] worlds;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.HelpBox(position, "Incorect property type!", MessageType.Error);

                return;
            }

            if(worldsDatabase == null)
            {
                worldsDatabase = EditorUtils.GetAsset<WorldsDatabase>(); 
                
                worlds = new GUIContent[worldsDatabase.Worlds.Length];
                for (int i = 0; i < worlds.Length; i++)
                {
                    worlds[i] = new GUIContent(string.Format("#{0} - {1}", (i + 1), worldsDatabase.Worlds[i].Scene.Name));
                }

                if (!worlds.IsInRange(property.intValue))
                {
                    Debug.LogError("Incorrect world index!", property.serializedObject.targetObject);
                }
            }

            property.intValue = EditorGUI.Popup(position, label, property.intValue, worlds);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}
