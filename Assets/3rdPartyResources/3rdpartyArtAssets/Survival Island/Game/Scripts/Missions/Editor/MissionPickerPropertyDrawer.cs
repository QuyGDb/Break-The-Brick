using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(MissionPickerAttribute))]
    public class MissionPickerPropertyDrawer : PropertyDrawer
    {
        private MissionsHolder missionsHolder;
        private Mission[] missions;
        private GUIContent[] displayedOptions;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "Incorect property type!", MessageType.Error);

                return;
            }

            if (missionsHolder == null)
            {
                missionsHolder = GameObject.FindObjectOfType<MissionsHolder>();
                if(missionsHolder != null)
                {
                    missions = missionsHolder.GetComponentsInChildren<Mission>();
                    displayedOptions = new GUIContent[missions.Length];

                    for(int i = 0; i < missions.Length; i++)
                    {
                        displayedOptions[i] = new GUIContent(missions[i].name);
                    }
                }
                else
                {
                    EditorGUI.HelpBox(position, "MissionsHolder can't be found!", MessageType.Error);

                    return;
                }
            }

            EditorGUI.BeginProperty(position, label, property);

            if (missions.Length > 0)
            {
                int selectedMission = System.Array.FindIndex(missions, x => x.ID == property.stringValue);

                int tempIndex = EditorGUI.Popup(position, label, selectedMission, displayedOptions);
                if (selectedMission != tempIndex)
                {
                    property.stringValue = missions[tempIndex].ID;
                }
            }
            else
            {
                EditorGUI.HelpBox(position, "Missions can't be found on the scene!", MessageType.Error);
            }

            EditorGUI.EndProperty();
        }
    }
}
