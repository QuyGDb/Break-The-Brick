using UnityEditor;

namespace Watermelon
{
    [CustomEditor(typeof(WorldsDatabase))]
    public class WorldsDatabaseEditor : WatermelonEditor
    {
        private SerializedProperty worldsProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            worldsProperty = serializedObject.FindProperty("worlds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int tempArraySize = worldsProperty.arraySize;

            EditorGUILayout.PropertyField(worldsProperty, true);

            if(worldsProperty.arraySize > tempArraySize)
            {
                SerializedProperty addedElement = worldsProperty.GetArrayElementAtIndex(worldsProperty.arraySize - 1);
                addedElement.FindPropertyRelative("id").stringValue = "";
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
