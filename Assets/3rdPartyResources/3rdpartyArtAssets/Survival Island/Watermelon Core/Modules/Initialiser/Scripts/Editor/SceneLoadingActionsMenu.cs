using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Watermelon
{
    public static class SceneLoadingActionsMenu
    {
        [MenuItem("Actions/Game Scene", priority = 100)]
        private static void GameScene()
        {
            EditorSceneManager.OpenScene(@"Assets\Project Data\Game\Scenes\Game.unity");
        }

        [MenuItem("Actions/Game Scene", true)]
        private static bool GameSceneValidation()
        {
            return !Application.isPlaying;
        }
    }
}