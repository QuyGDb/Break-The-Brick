using UnityEngine;

namespace Watermelon
{
    [RegisterModule("Game Loading Settings", true)]
    public class GameLoadingInitModule : InitModule
    {
        public override string ModuleName => "Game Loading Settings";

        [Tooltip("If manual mode is enabled, the loading screen will be active until GameLoading.MarkAsReadyToHide method has been called.")]
        [SerializeField] bool manualControlMode;
        [SerializeField] GameObject loadingCanvasObject;

        public override void CreateComponent(GameObject holderObject)
        {
            if(loadingCanvasObject != null)
            {
                GameObject tempObject = Instantiate(loadingCanvasObject);
                tempObject.transform.ResetGlobal();
            }

            if (manualControlMode)
                GameLoading.EnableManualControlMode();
        }
    }
}
