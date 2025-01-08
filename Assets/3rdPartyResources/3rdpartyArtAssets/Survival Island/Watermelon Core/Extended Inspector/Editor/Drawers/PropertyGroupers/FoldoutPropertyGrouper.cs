using UnityEditor;

namespace Watermelon
{
    [PropertyGrouper(typeof(FoldoutAttribute))]
    public class FoldoutPropertyGrouper : PropertyGrouper
    {
        public override void BeginGroup(WatermelonEditor editor, string groupID, string label)
        {
            EditorFoldoutBool foldoutBool = editor.GetFoldout(groupID);

            foldoutBool.Value = EditorGUILayout.Foldout(foldoutBool.Value, !string.IsNullOrEmpty(label) ? label : groupID, true);

            EditorGUI.indentLevel++;

        }

        public override void EndGroup()
        {
            EditorGUI.indentLevel--;
        }

        public override bool DrawRenderers(WatermelonEditor editor, string groupID) => editor.GetFoldout(groupID).Value;
    }
}
