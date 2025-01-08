using System.Collections.Generic;

namespace Watermelon
{
    [System.Serializable]
    public class WorldGlobalSave : ISaveObject
    {
        public List<int> alreadyVisitedWorlds = new List<int>();

        public string worldID;
        public string activeMissionName;

        public WorldGlobalSave()
        {
            alreadyVisitedWorlds = new List<int>();
        }

        public void Flush()
        {

        }

        public bool IsWorldAlreadyVisited(int worldId)
        {
            // -1 means world was not visited before
            return alreadyVisitedWorlds.IndexOf(worldId) != -1;
        }

        public void OnWorldFirstTimeVisited(int worldId)
        {
            if (!IsWorldAlreadyVisited(worldId))
                alreadyVisitedWorlds.Add(worldId);
        }
    }
}