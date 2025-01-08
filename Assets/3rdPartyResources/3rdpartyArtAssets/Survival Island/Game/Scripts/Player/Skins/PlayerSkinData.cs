using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class PlayerSkinData
    {
        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [UniqueID]
        [SerializeField] string id;
        public string ID => id;

        public bool IsUnlocked => save.IsUnlocked;

        private PlayerSkinSave save;

        public void Initialise()
        {
            save = SaveController.GetSaveObject<PlayerSkinSave>(id);
        }

        public void Unlock()
        {
            save.IsUnlocked = true;
        }
    }
}
