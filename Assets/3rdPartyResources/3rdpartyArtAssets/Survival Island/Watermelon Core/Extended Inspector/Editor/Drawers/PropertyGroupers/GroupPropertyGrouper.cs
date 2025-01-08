using UnityEditor;

namespace Watermelon
{
    [PropertyGrouper(typeof(GroupAttribute))]
    public class GroupPropertyGrouper : PropertyGrouper
    {
        public override void BeginGroup(WatermelonEditor editor, string groupID, string label)
        {
            EditorGUILayout.BeginVertical();
        }

        public override void EndGroup()
        {
            EditorGUILayout.EndVertical();
        }
    }
}
