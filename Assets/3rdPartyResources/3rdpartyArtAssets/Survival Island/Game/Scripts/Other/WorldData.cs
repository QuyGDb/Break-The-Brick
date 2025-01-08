using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class WorldData
    {
        [SerializeField] SceneObject scene;
        public SceneObject Scene => scene;

        [UniqueID]
        [SerializeField] string id;
        public string ID => id;
    }
}
