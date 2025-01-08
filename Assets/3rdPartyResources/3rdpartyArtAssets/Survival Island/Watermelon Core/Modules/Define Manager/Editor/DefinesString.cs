using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System;

namespace Watermelon
{
    public class DefinesString
    {
        private string defineLine;
        private List<string> defineList;

        public DefinesString()
        {
            defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            defineList = new List<string>(defineLine.Split(';'));
        }

        public bool HasDefine(string define)
        {
            return defineList.FindIndex(x => x == define) != -1;
        }

        public void RemoveDefine(string define)
        {
            int defineIndex = defineList.FindIndex(x => x == define);
            if (defineIndex == -1)
                return;

            defineList.RemoveAt(defineIndex);
        }

        public void AddDefine(string define)
        {
            int defineIndex = defineList.FindIndex(x => x == define);
            if (defineIndex != -1)
                return;

            defineList.Add(define);
        }

        public string GetDefineLine()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string define in defineList)
            {
                sb.Append(define);
                sb.Append(";");
            }

            return sb.ToString();
        }

        public bool HasChanges()
        {
            return defineLine != GetDefineLine();
        }

        public void ApplyDefines()
        {
            string newDefineLine = GetDefineLine();

            if (defineLine != newDefineLine)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), newDefineLine);
            }
        }
    }
}