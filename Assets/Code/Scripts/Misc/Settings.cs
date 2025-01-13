
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

    public static int firstPersonLevel = 1;
    public static int thirdPersonLevel = 1;

}
