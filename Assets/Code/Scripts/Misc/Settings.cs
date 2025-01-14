
using UnityEngine;
public static class Settings
{
    #region ANIMATOR PARAMETERS
    // Animator parameters - Player
    public static int isChop = Animator.StringToHash("isChop");
    #endregion

    #region Play Mode
    public static PlayMode playMode;
    #endregion

    #region Audio
    public const float musicFadeOutTime = 1f;
    public const float musicFadeInTime = 1f;

    #endregion
}
