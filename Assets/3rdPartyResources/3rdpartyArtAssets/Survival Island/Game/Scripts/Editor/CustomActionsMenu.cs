using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menues customization tutorial
// https://hugecalf-studios.github.io/unity-lessons/lessons/editor/menuitem/#:~:text=To%20disable%20a%20menu%20item,return%20false%20from%20this%20method.

namespace Watermelon
{
    public static class CustomActionsMenu
    {
        #region Resources

        [MenuItem("Actions/Get Resources/None", priority = 23)]
        private static void Reset()
        {
            Currency[] currencies = CurrenciesController.Currencies;
            foreach (var currency in currencies)
            {
                CurrenciesController.Set(currency.CurrencyType, 0);
            }
        }

        [MenuItem("Actions/Get Resources/All", priority = 24)]
        private static void GetAllResources()
        {
            Currency[] currencies = CurrenciesController.Currencies;
            foreach (var currency in currencies)
            {
                CurrenciesController.Set(currency.CurrencyType, 10000);
            }
        }

        [MenuItem("Actions/Get Resources/Coins", priority = 24)]
        private static void GetCoins()
        {
            CurrenciesController.Set(CurrencyType.Coins, 10000);
        }

        [MenuItem("Actions/Get Resources/Wood", priority = 24)]
        private static void GetWood()
        {
            CurrenciesController.Set(CurrencyType.Wood, 2000);
        }

        [MenuItem("Actions/Get Resources/Stone", priority = 25)]
        private static void GetStone()
        {
            CurrenciesController.Set(CurrencyType.Stone, 2000);
        }

        [MenuItem("Actions/Get Resources/Fiber", priority = 26)]
        private static void GetFiber()
        {
            CurrenciesController.Set(CurrencyType.Fiber, 2000);
        }

        [MenuItem("Actions/Get Resources/Rope", priority = 26)]
        private static void GetRope()
        {
            CurrenciesController.Set(CurrencyType.Rope, 2000);
        }

        [MenuItem("Actions/Get Resources/Planks", priority = 26)]
        private static void GetPlanks()
        {
            CurrenciesController.Set(CurrencyType.Planks, 2000);
        }

        [MenuItem("Actions/Get Resources/Bricks", priority = 26)]
        private static void GetBricks()
        {
            CurrenciesController.Set(CurrencyType.Bricks, 2000);
        }

        [MenuItem("Actions/Get Resources/Berries", priority = 26)]
        private static void GetBerries()
        {
            CurrenciesController.Set(CurrencyType.Berries, 2000);
        }

        [MenuItem("Actions/Get Resources/Coconuts", priority = 26)]
        private static void GetCoconuts()
        {
            CurrenciesController.Set(CurrencyType.Coconut, 2000);
        }

        [MenuItem("Actions/Get Resources/Pumpkins", priority = 26)]
        private static void GetPumpkins()
        {
            CurrenciesController.Set(CurrencyType.Pumpkin, 2000);
        }

        [MenuItem("Actions/Get Resources/Fish", priority = 26)]
        private static void GetFish()
        {
            CurrenciesController.Set(CurrencyType.Fish, 2000);
        }

        [MenuItem("Actions/Get Resources/Scissors", priority = 26)]
        private static void GetScissors()
        {
            CurrenciesController.Set(CurrencyType.Scissors, 2000);
        }
        #endregion

        #region Tools

        [MenuItem("Actions/Tools/Unlock Sickle", priority = 26)]
        private static void UnlockSickle()
        {
            UnlockableToolsController.UnlockTool(InteractionAnimationType.Cutting);
        }

        [MenuItem("Actions/Tools/Unlock Fishing Rod", priority = 26)]
        private static void UnlockFishingRod()
        {
            UnlockableToolsController.UnlockTool(InteractionAnimationType.Fishing);
        }

        #endregion

        [MenuItem("Actions/World 1 Scene", priority = 102)]
        private static void World1Scene()
        {
            EditorSceneManager.OpenScene(@"Assets\Project Data\Game\Scenes\Worlds\World 1 [Full Demo].unity");
        }

        [MenuItem("Actions/World 2 Scene", priority = 102)]
        private static void World2Scene()
        {
            EditorSceneManager.OpenScene(@"Assets\Project Data\Game\Scenes\Worlds\World 2 [Alt Demo].unity");
        }
        
        [MenuItem("Actions/Get Resources/None", true)]
        [MenuItem("Actions/Get Resources/All", true)]
        [MenuItem("Actions/Get Resources/Coins", true)]
        [MenuItem("Actions/Get Resources/Wood", true)]
        [MenuItem("Actions/Get Resources/Stone", true)]
        [MenuItem("Actions/Get Resources/Fiber", true)]
        [MenuItem("Actions/Get Resources/Rope", true)]
        [MenuItem("Actions/Get Resources/Berries", true)]
        [MenuItem("Actions/Get Resources/Planks", true)]
        [MenuItem("Actions/Get Resources/Bricks", true)]
        [MenuItem("Actions/Get Resources/Coconuts", true)]
        [MenuItem("Actions/Get Resources/Pumpkins", true)]
        [MenuItem("Actions/Get Resources/Fish", true)]
        [MenuItem("Actions/Get Resources/Scissors", true)]
        private static bool IsGamePlaying()
        {
            return Application.isPlaying;
        }

        [MenuItem("Actions/World 1 Scene", true)]
        [MenuItem("Actions/World 2 Scene", true)]
        private static bool IsGameNotPlaying()
        {
            return !Application.isPlaying;
        }
    }
}