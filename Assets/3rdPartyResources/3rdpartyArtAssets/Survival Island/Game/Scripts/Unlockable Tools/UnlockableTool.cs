using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class UnlockableTool
    {
        private const string SAVE_FORMAT = "unlockableTool_{0}";

        [SerializeField] InteractionAnimationType toolType;
        public InteractionAnimationType ToolType => toolType;

        [SerializeField] string customName;
        public string CustomName => customName;

        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        public bool IsUnlocked => save.IsUnlocked;

        private UnlockableToolSave save;

        public event SimpleCallback ToolUnlocked;

        public void Initialise()
        {
            save = SaveController.GetSaveObject<UnlockableToolSave>(string.Format(SAVE_FORMAT, toolType));
        }

        public void Unlock()
        {
            save.IsUnlocked = true;

            ToolUnlocked?.Invoke();
        }

        public void Lock()
        {
            save.IsUnlocked = false;
        }

        public string GetToolName()
        {
            if (!string.IsNullOrEmpty(customName))
                return customName;

            return toolType.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}) - IsUnlocked:{2}", toolType, !string.IsNullOrEmpty(customName) ? customName : toolType, save != null ? save.IsUnlocked : "?");
        }
    }
}
