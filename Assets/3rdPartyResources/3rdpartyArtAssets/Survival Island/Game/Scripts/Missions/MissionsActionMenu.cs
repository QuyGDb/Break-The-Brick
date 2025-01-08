using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon
{
    public class MissionsActionMenu
    {
        private const string MenuName = "Actions/Turn Missions Off";
        private const string SettingName = "AreMissionsDisabled";

        public static bool AreMissionsDisabled()
        {
#if UNITY_EDITOR
            return AreMissionsDisabledPrefs;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        private static bool AreMissionsDisabledPrefs
        {
            get { return EditorPrefs.GetBool(SettingName, false); }
            set { EditorPrefs.SetBool(SettingName, value); }
        }

        [MenuItem(MenuName, priority = 201)]
        private static void ToggleAction()
        {
            bool devPanelState = AreMissionsDisabledPrefs;
            AreMissionsDisabledPrefs = !devPanelState;
        }

        [MenuItem(MenuName, true, priority = 201)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked(MenuName, AreMissionsDisabledPrefs);

            return !Application.isPlaying;
        }
#endif
    }
}