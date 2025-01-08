using UnityEngine;

namespace Watermelon
{
    public sealed class GDPRLoadingTask : LoadingTask
    {
        private GDPRPanel gdprPanel;

        public override void Activate()
        {
            GameObject gdprPanelPrefab = AdsManager.Settings.GDPRPanelPrefab;
            if(gdprPanelPrefab != null)
            {
                isActive = true;

                GameObject gdprPanelObject = GameObject.Instantiate(AdsManager.Settings.GDPRPanelPrefab);
                gdprPanelObject.transform.ResetGlobal();

                gdprPanel = gdprPanelObject.GetComponent<GDPRPanel>();
                gdprPanel.Initialise(this);
            }
            else
            {
                Debug.LogError("[Ads Manager]: GDPR panel prefab is missing!");

                CompleteTask();
            }
        }
    }
}
