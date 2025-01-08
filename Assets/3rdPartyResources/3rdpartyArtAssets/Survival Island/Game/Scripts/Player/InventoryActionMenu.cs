using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon
{
    public static class InventoryActionMenu
    {
        private const string MenuName = "Actions/Infinite Inventory";
        private const string SettingName = "IsInventoryInfinite";

        public static bool IsInventoryInfinite()
        {
#if UNITY_EDITOR
            return IsInventoryInfinitePrefs;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        private static bool IsInventoryInfinitePrefs
        {
            get { return EditorPrefs.GetBool(SettingName, false); }
            set { EditorPrefs.SetBool(SettingName, value); }
        }

        [MenuItem(MenuName, priority = 202)]
        private static void ToggleAction()
        {
            bool devState = IsInventoryInfinitePrefs;
            IsInventoryInfinitePrefs = !devState;
        }

        [MenuItem(MenuName, true, priority = 202)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked(MenuName, IsInventoryInfinitePrefs);

            return !Application.isPlaying;
        }
#endif
    }
}