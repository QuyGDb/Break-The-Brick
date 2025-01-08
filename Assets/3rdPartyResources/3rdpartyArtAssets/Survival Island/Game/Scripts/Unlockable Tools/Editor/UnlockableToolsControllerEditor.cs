using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    [CustomEditor(typeof(UnlockableToolsController))]
    public class UnlockableToolsControllerEditor : WatermelonEditor
    {
        private bool foldout;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(Application.isPlaying)
            {
                foldout = EditorGUILayout.Foldout(foldout, "Unlockable Tools");
                if (foldout)
                {
                    UnlockableTool[] unlockableTools = UnlockableToolsController.RegisteredUnlockableTools;
                    if(!unlockableTools.IsNullOrEmpty())
                    {
                        foreach (var tool in UnlockableToolsController.RegisteredUnlockableTools)
                        {
                            bool isUnlocked = tool.IsUnlocked;
                            if (GUILayout.Button(string.Format("{0} ({1})", tool.GetToolName(), isUnlocked ? "Unlocked" : "Locked")))
                            {
                                if (!isUnlocked)
                                {
                                    tool.Unlock();
                                }
                                else
                                {
                                    tool.Lock();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
