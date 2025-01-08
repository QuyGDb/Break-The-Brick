using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Worlds Database", menuName = "Content/Worlds Database")]
    public class WorldsDatabase : ScriptableObject
    {
        [SerializeField] WorldData[] worlds;
        public WorldData[] Worlds => worlds;

        public WorldData GetWorldByID(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                // Return first world if ID is empty
                return worlds[0];
            }

            foreach (WorldData world in worlds)
            {
                if (world.ID == id)
                    return world;
            }

            return worlds[0];
        }

        public WorldData GetWorldByIndex(int index)
        {
            if (worlds.IsInRange(index))
                return worlds[index];

            return worlds.GetRandomItem();
        }

        public WorldData GetWorldByName(string name)
        {
            foreach (WorldData world in worlds)
            {
                if (world.Scene.Name == name)
                    return world;
            }

            return worlds[0];
        }

        public bool IsWorldExists(int index)
        {
            if (worlds.IsInRange(index))
                return true;

            return false;
        }

        public string GetWorldSceneName(int worldIndex)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.GetBool("OverrideScene", false))
            {
                string customScene = UnityEditor.EditorPrefs.GetString("CustomSceneName", string.Empty);

                if (customScene != null)
                    return customScene;
            }
#endif

            return worlds[worldIndex % worlds.Length].Scene.Name;
        }
    }
}