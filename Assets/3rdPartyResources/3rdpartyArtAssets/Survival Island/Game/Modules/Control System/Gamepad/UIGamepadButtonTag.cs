using System;

namespace Watermelon
{
    [Flags]
    public enum UIGamepadButtonTag
    {
        None = 0,
        Game = 1,
        Inventory = 2,
        Upgrades = 4,
        IAPStore = 8,
        Popup = 16,
        MainMenu = 32,
    }
}