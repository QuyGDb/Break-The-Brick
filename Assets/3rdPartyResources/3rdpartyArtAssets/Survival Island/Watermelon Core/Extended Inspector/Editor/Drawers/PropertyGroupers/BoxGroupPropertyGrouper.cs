using UnityEngine;

namespace Watermelon
{
    [PropertyGrouper(typeof(BoxGroupAttribute))]
    public class BoxGroupPropertyGrouper : PropertyGrouper
    {
        public override void BeginGroup(WatermelonEditor editor, string groupID, string label)
        {
            EditorGUILayoutCustom.BeginBoxGroup(label);
        }

        public override void EndGroup()
        {
            EditorGUILayoutCustom.EndBoxGroup();
        }
    }
}
