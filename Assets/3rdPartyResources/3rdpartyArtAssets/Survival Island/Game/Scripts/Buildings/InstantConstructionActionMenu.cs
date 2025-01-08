using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Watermelon
{
    public class InstantConstructionActionMenu : MonoBehaviour
    {
        private const string MenuName = "Actions/Instant Construction";
        private const string SettingName = "IsConstructionInstant";

        public static bool IsConstructionInstant()
        {
#if UNITY_EDITOR
            return IsConstructionInstantPrefs;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        private static bool IsConstructionInstantPrefs
        {
            get { return EditorPrefs.GetBool(SettingName, false); }
            set { EditorPrefs.SetBool(SettingName, value); }
        }

        [MenuItem(MenuName, priority = 202)]
        private static void ToggleAction()
        {
            bool devState = IsConstructionInstantPrefs;
            IsConstructionInstantPrefs = !devState;
        }

        [MenuItem(MenuName, true, priority = 202)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked(MenuName, IsConstructionInstantPrefs);

            return true;
        }

 

#endif
    }
}
