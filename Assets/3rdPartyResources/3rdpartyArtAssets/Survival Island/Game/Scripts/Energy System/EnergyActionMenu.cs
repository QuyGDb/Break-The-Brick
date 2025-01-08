using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Watermelon
{

    public static class EnergyActionMenu
    {
        private const string MenuName = "Actions/Energy System/Disable Energy";
        private const string SettingName = "IsEnergyDisabled";

        public static bool IsEnergyDisabled()
        {
#if UNITY_EDITOR
            return IsEnergyDisabledPrefs;
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        private static bool IsEnergyDisabledPrefs
        {
            get { return EditorPrefs.GetBool(SettingName, false); }
            set { EditorPrefs.SetBool(SettingName, value); }
        }

        [MenuItem(MenuName, priority = 203)]
        private static void ToggleAction()
        {
            bool devState = IsEnergyDisabledPrefs;
            IsEnergyDisabledPrefs = !devState;
        }

        [MenuItem(MenuName, true, priority = 203)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked(MenuName, IsEnergyDisabledPrefs);

            return !Application.isPlaying;
        }

        [MenuItem("Actions/Energy System/Increase", priority = 204)]
        private static void AddFood()
        {
            EnergyController.RestoreEnergyPoints(EnergyController.Data.MaxEnergyPoints * 0.4f);
        }

        [MenuItem("Actions/Energy System/Decrease", priority = 204)]
        private static void RemoveFood()
        {
            EnergyController.RemoveEnergyPoints(EnergyController.Data.MaxEnergyPoints * 0.4f);
        }

        [MenuItem("Actions/Energy System/Increase", true)]
        [MenuItem("Actions/Energy System/Decrease", true)]
        private static bool IncreaseDecreaseValidation()
        {
            return Application.isPlaying;
        }

#endif
    }
}