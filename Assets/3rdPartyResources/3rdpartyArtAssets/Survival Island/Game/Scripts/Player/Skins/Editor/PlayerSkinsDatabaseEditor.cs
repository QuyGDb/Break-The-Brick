using UnityEditor;

namespace Watermelon
{
    [CustomEditor(typeof(PlayerSkinsDatabase))]
    public class PlayerSkinsDatabaseEditor : WatermelonEditor
    {
        private SerializedProperty skinsProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            skinsProperty = serializedObject.FindProperty("skins");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int tempArraySize = skinsProperty.arraySize;

            base.OnInspectorGUI();

            if (skinsProperty.arraySize > tempArraySize)
            {
                SerializedProperty addedElement = skinsProperty.GetArrayElementAtIndex(skinsProperty.arraySize - 1);

                addedElement.FindPropertyRelative("id").stringValue = "";
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
