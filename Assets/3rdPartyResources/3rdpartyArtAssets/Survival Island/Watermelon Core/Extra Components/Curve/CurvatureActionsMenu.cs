using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon
{
    public class CurvatureActionsMenu
    {
        private const string MenuName = "Actions/Disable Curvature";
        private const string SettingName = "IsCurvatureDisabled";

        public static bool IsCurvatureDisabled()
        {
#if UNITY_EDITOR
            return IsCurvatureDisabledPrefs;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        private static bool IsCurvatureDisabledPrefs
        {
            get { return EditorPrefs.GetBool(SettingName, false); }
            set { EditorPrefs.SetBool(SettingName, value); }
        }

        [MenuItem(MenuName, priority = 202)]
        private static void CurvatureToggleAction()
        {
            bool state = IsCurvatureDisabledPrefs;
            IsCurvatureDisabledPrefs = !state;
        }

        [MenuItem(MenuName, true, priority = 202)]
        private static bool CurvatureToggleActionValidate()
        {
            Menu.SetChecked(MenuName, IsCurvatureDisabledPrefs);

            return !Application.isPlaying;
        }
#endif

    }
}