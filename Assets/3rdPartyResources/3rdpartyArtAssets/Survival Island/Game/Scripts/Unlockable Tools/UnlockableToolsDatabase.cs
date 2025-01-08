using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Unlockable Tools Database", menuName = "Content/Unlockable Tools")]
    public class UnlockableToolsDatabase : ScriptableObject
    {
        [SerializeField] UnlockableTool[] unlockableTools;
        public UnlockableTool[] UnlockableTools => unlockableTools;
    }
}
