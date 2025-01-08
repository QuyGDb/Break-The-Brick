using UnityEditor;

namespace Watermelon
{
    [PropertyGrouper(typeof(HorizontalGroupAttribute))]
    public class HorizontalGroupPropertyGrouper : PropertyGrouper
    {
        public override void BeginGroup(WatermelonEditor editor, string groupID, string label)
        {
            EditorGUILayout.BeginHorizontal();
        }

        public override void EndGroup()
        {
            EditorGUILayout.EndHorizontal();
        }
    }
}
