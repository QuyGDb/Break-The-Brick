using UnityEngine;

namespace Watermelon
{
    public class UnlockableToolsController : MonoBehaviour
    {
        private static readonly int FLOATING_TEXT_HASH = FloatingTextController.GetHash("UnlockableTool");

        private const float FLOATING_TEXT_DELAY = 3.0f;
        private const string FLOATING_TEXT_MESSAGE = "Required!";

        private static UnlockableToolsController instance;

        [SerializeField] UnlockableToolsDatabase database;
        [SerializeField] Color floatingTextColor = Color.red;

        private static UnlockableTool[] registeredUnlockableTools;
        public static UnlockableTool[] RegisteredUnlockableTools => registeredUnlockableTools;

        private static float nextMessageTime;

        public void Initialise()
        {
            instance = this;

            registeredUnlockableTools = database.UnlockableTools;

            foreach(UnlockableTool unlockableTool in registeredUnlockableTools)
            {
                unlockableTool.Initialise();
            }
        }

        public static bool IsToolUnlocked(InteractionAnimationType toolType)
        {
            foreach (UnlockableTool unlockableTool in registeredUnlockableTools)
            {
                if (unlockableTool.ToolType == toolType)
                    return unlockableTool.IsUnlocked;
            }

            return true;
        }

        public static UnlockableTool GetUnlockableTool(InteractionAnimationType toolType)
        {
            foreach (UnlockableTool unlockableTool in registeredUnlockableTools)
            {
                if (unlockableTool.ToolType == toolType)
                {
                    return unlockableTool;
                }
            }

            return null;
        }

        public static void UnlockTool(InteractionAnimationType toolType)
        {
            foreach (UnlockableTool unlockableTool in registeredUnlockableTools)
            {
                if (unlockableTool.ToolType == toolType)
                {
                    unlockableTool.Unlock();

                    return;
                }
            }
        }

        public static FloatingTextBaseBehaviour ShowMessage(InteractionAnimationType toolType, Vector3 position, Quaternion rotation)
        {
            if (nextMessageTime > Time.time)
                return null;

            UnlockableTool unlockableTool = GetUnlockableTool(toolType);

            if (unlockableTool == null)
                return null;

            return ShowMessage(unlockableTool, position, rotation);
        }

        public static FloatingTextBaseBehaviour ShowMessage(UnlockableTool unlockableTool, Vector3 position, Quaternion rotation)
        {
            if (nextMessageTime > Time.time)
                return null;

            nextMessageTime = Time.time + FLOATING_TEXT_DELAY;

            FloatingTextBaseBehaviour floatingText = FloatingTextController.SpawnFloatingText(FLOATING_TEXT_HASH, FLOATING_TEXT_MESSAGE, position, rotation, instance.floatingTextColor);
            
            UnlockableToolFloatingText unlockableToolFloatingText = (UnlockableToolFloatingText)floatingText;
            if(unlockableToolFloatingText != null)
            {
                unlockableToolFloatingText.Initialise(unlockableTool);
            }
            
            return floatingText;
        }
    }
}
