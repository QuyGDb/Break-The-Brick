using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class MissionUIPanel : MonoBehaviour
    {
        [SerializeField] Button clickHandler;

        [Space]
        [SerializeField] GameObject previewIconObject;
        [SerializeField] GameObject missionCompletePanel;
        [SerializeField] TextMeshProUGUI rewardText;
        [SerializeField] GameObject backgroundObj;

        [Header("Missions")]
        [SerializeField] MissionCollectUICase collectMission;
        [SerializeField] MissionUnlockUICase buildMission;
        [SerializeField] MissionTaskUICase taskMission;

        private Mission activeMission;

        private MissionUICase[] missionUICases;
        private MissionUICase activeUICase;

        private RectTransform floatingCloundSpawn;
        private RectTransform floatingCloundTarget;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Initialise()
        {
            gameObject.SetActive(true);

            // Initialise mission cases
            missionUICases = new MissionUICase[] { collectMission, buildMission, taskMission };

            for (int i = 0; i < missionUICases.Length; i++)
            {
                missionUICases[i].Initialise(this);
                missionUICases[i].ParentObject.SetActive(false);
            }

            clickHandler.onClick.AddListener(OnMissionClicked);

            UIGame gameUI = UIController.GetPage<UIGame>();

            // Creating positions for currency reward cloud
            GameObject spawn = new GameObject("[floating cloud spawn]");
            spawn.transform.position = rewardText.transform.position;
            spawn.transform.SetParent(gameUI.transform);
            floatingCloundSpawn = spawn.AddComponent<RectTransform>();

            GameObject target = new GameObject("[floating cloud target]");
            target.transform.SetParent(gameUI.transform);
            floatingCloundTarget = target.AddComponent<RectTransform>();
            floatingCloundTarget.anchorMin = Vector2.one;
            floatingCloundTarget.anchorMax = Vector2.one;
            // this position is relayted to the top right corner of the screen
            floatingCloundTarget.anchoredPosition = new Vector2(-100f, -250f);
        }

        private void Update()
        {
            if (activeMission != null)
            {
                if (activeMission.MissionStage == Mission.Stage.Active)
                {
                    if (activeMission.IsDirty)
                    {
                        if (activeUICase != null)
                        {
                            activeUICase.UpdateUI();
                        }
                    }
                }
            }
        }

        private void OnMissionClicked()
        {
            if (activeMission != null)
            {
                AudioController.PlaySound(AudioController.AudioClips.buttonSound);

                if (activeMission.MissionStage == Mission.Stage.Active)
                {
                    activeMission.DoCameraPreview();

#if MODULE_HAPTIC
                    Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

                }
                else if (activeMission.MissionStage == Mission.Stage.Finished && activeMission.RewardType == MissionRewardType.Resources)
                {
                    MissionsController.CompleteMission();

#if MODULE_HAPTIC
                    Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif

                }
            }
        }

        public void ActivateMission(Mission mission)
        {
            // if panel was disabled - run appear animation
            if (!backgroundObj.activeSelf)
            {
                backgroundObj.transform.localScale = Vector3.zero;
                backgroundObj.transform.DOScale(1f, 0.2f).SetEasing(Ease.Type.BackOut);
            }

            backgroundObj.SetActive(true);
            previewIconObject.SetActive(true);
            missionCompletePanel.SetActive(false);

            if (activeMission != null)
            {
                // Unload previous mission
                activeMission.OnStageChanged -= OnStageChanged;
            }

            if (activeUICase != null)
            {
                // Disable previous mission type UI
                activeUICase.ParentObject.SetActive(false);
                activeUICase.Disable();
            }

            activeMission = mission;
            activeMission.OnStageChanged += OnStageChanged;

            if (activeMission.MissionStage == Mission.Stage.Finished)
            {
                ActivateReward();
            }
            else
            {
                // Enable panel
                ActivateUIPanel(mission.MissionUIType, mission);

                activeUICase.SetTitle(string.Format(activeMission.Title));
                activeUICase.UpdateUI();
            }
        }

        private void OnStageChanged(Mission.Stage previousStage, Mission.Stage currentStage)
        {
            if (activeUICase != null)
            {
                if (currentStage == Mission.Stage.Finished)
                {
                    ActivateReward();

                    // Disable previous mission type UI
                    activeUICase.ParentObject.SetActive(false);
                    activeUICase.Disable();
                }
                else
                {
                    activeUICase.UpdateUI();
                }
            }
        }

        private void ActivateReward()
        {
            if (activeMission != null)
            {
                previewIconObject.SetActive(false);

                if (!missionCompletePanel.activeSelf)
                {
                    missionCompletePanel.transform.localScale = Vector3.zero;
                    missionCompletePanel.transform.DOScale(1f, 0.15f).SetEasing(Ease.Type.BackOut);
                }

                missionCompletePanel.SetActive(true);

                rewardText.text = string.Empty;

                if (activeMission.RewardType == MissionRewardType.Resources)
                {
                    Mission.ResourceRewardData reward = activeMission.ResourceReward;
                    if (reward != null && reward.Amount > 0)
                    {
                        rewardText.text = string.Format("{0} <sprite name={1}>", reward.Amount, reward.CurrencyType);
                    }
                }
                else
                {
                    rewardText.text = activeMission.RewardType == MissionRewardType.Tool ? "TOOL UNLOCKED" : "NEW BUILDING";
                }
            }
        }

        public void SetTitle(string title)
        {
            if (activeUICase != null)
            {
                activeUICase.SetTitle(title);
            }
        }

        public void OnRewardClaimed()
        {
            DisableUIPanels();

            if (activeMission != null && activeMission.RewardType == MissionRewardType.Resources)
            {
                FloatingCloud.SpawnCurrency(activeMission.ResourceReward.CurrencyType.ToString(), floatingCloundSpawn, floatingCloundTarget, 10, "");
            }

            if (missionCompletePanel.activeSelf)
            {
                missionCompletePanel.transform.DOScale(0f, 0.15f).SetEasing(Ease.Type.BackIn).OnComplete(() =>
                {
                    missionCompletePanel.SetActive(false);
                    missionCompletePanel.transform.localScale = Vector3.one;
                });
            }

            backgroundObj.SetActive(false);
        }

        private void DisableUIPanels()
        {
            for (int i = 0; i < missionUICases.Length; i++)
            {
                missionUICases[i].ParentObject.SetActive(false);
            }
        }

        private void ActivateUIPanel(MissionUICase.Type panelType, Mission mission)
        {
            for (int i = 0; i < missionUICases.Length; i++)
            {
                if (missionUICases[i].MissionType == panelType)
                {
                    missionUICases[i].ParentObject.SetActive(true);
                    missionUICases[i].Activate(mission);

                    activeUICase = missionUICases[i];

                    break;
                }
            }
        }
    }

    [System.Serializable]
    public abstract class MissionUICase
    {
        [SerializeField]
        protected Type missionType;
        public Type MissionType => missionType;

        [SerializeField]
        protected GameObject parentObject;
        public GameObject ParentObject => parentObject;

        [SerializeField] protected TextMeshProUGUI titleText;

        protected MissionUIPanel uiPanel;
        protected Mission mission;

        public virtual void Initialise(MissionUIPanel uiPanel)
        {
            this.uiPanel = uiPanel;
        }

        public virtual void Activate(Mission mission)
        {
            this.mission = mission;
        }

        public virtual void Disable()
        {

        }

        public virtual void UpdateUI()
        {

        }

        public virtual void SetTitle(string title)
        {
            titleText.text = title;
        }

        public enum Type
        {
            Collect = 0,
            Build = 1,
            Task = 2,
        }
    }

    [System.Serializable]
    public class MissionCollectUICase : MissionUICase
    {
        [Space]
        [SerializeField] RectTransform panelRectTransform;
        [SerializeField] TextMeshProUGUI descriptionText;

        public override void Initialise(MissionUIPanel uiPanel)
        {
            this.uiPanel = uiPanel;
        }

        public override void Activate(Mission mission)
        {
            this.mission = mission;

            uiPanel.SetTitle(mission.Title);
        }

        public override void UpdateUI()
        {
            descriptionText.text = mission.GetFormattedProgress();

            mission.OnUIUpdated();
        }

        public override void SetTitle(string title)
        {
            titleText.text = title;
        }
    }

    [System.Serializable]
    public class MissionUnlockUICase : MissionUICase
    {
        [Space]
        [SerializeField] RectTransform panelRectTransform;

        [Space]
        [SerializeField] GameObject resourcePrefab;
        [SerializeField] Transform resourcesContainer;

        [Space]
        [SerializeField] Color defaultAmountColor;
        [SerializeField] Color activeAmountColor;

        [Space]
        [SerializeField] GameObject constructionProgressPanel;
        [SerializeField] TMP_Text constructionProgressText;
        [SerializeField] Image constructionIconImage;

        private Pool resourcePool;

        private MissionBuildRequiredResource[] resourcesUI;
        private IUnlockingMission unlockingMission;

        public override void Initialise(MissionUIPanel uiPanel)
        {
            this.uiPanel = uiPanel;

            resourcePool = new Pool(new PoolSettings(resourcePrefab.name, resourcePrefab, 3, true, resourcesContainer));
        }

        public override void Activate(Mission mission)
        {
            this.mission = mission;

            unlockingMission = (IUnlockingMission)mission;

            uiPanel.SetTitle(mission.Title);
            constructionProgressPanel.SetActive(false);

            // if there is purchase stage
            if (unlockingMission.LinkedBuilding.CanBePurchased)
            {
                RedrawRequiredResources();

                unlockingMission.LinkedBuilding.SubscribeOnResourcePlaced(RedrawRequiredResources);
                unlockingMission.LinkedBuilding.SubscribeOnPurchased(OnPurchased);
            }
            else
            {
                OnPurchased();
            }
        }

        private void RedrawRequiredResources()
        {
            resourcePool.ReturnToPoolEverything();

            ResourcesList currentCostLeft = unlockingMission.LinkedBuilding.CostLeft;

            resourcesUI = new MissionBuildRequiredResource[currentCostLeft.Count];
            for (int i = 0; i < resourcesUI.Length; i++)
            {
                GameObject resouceObject = resourcePool.GetPooledObject();
                resouceObject.SetActive(true);

                int current = 0;
                int amount = 0;

                for (int k = 0; k < currentCostLeft.Count; k++)
                {
                    if (currentCostLeft[i].currency == currentCostLeft[k].currency)
                    {
                        current = currentCostLeft[k].amount;
                    }
                }

                if (current > 0)
                {
                    amount = current;
                }
                else
                {
                    amount = 0;
                }

                resourcesUI[i] = resouceObject.GetComponent<MissionBuildRequiredResource>();
                resourcesUI[i].Initialise(currentCostLeft[i].currency, amount);
            }
        }

        public override void Disable()
        {
            CurrenciesController.UnsubscribeGlobalCallback(OnCurrencyAmountChanged);
            unlockingMission.LinkedBuilding.UnsubscribeOnPurchased(OnPurchased);
            unlockingMission.LinkedBuilding.UnsubscribeOnResourcePlaced(RedrawRequiredResources);
            unlockingMission.LinkedBuilding.UnsubscribeOnConstructed(OnConstructed);
            unlockingMission.LinkedBuilding.UnsubscribeOnGotHit(OnGotHit);

            resourcePool.ReturnToPoolEverything();
        }

        private void OnCurrencyAmountChanged(Currency currency, int amountDifference)
        {
            RedrawRequiredResources();
        }

        private void OnPurchased()
        {
            // if there is a construction stage
            if (unlockingMission.LinkedBuilding.SubscribeOnConstructed(OnConstructed))
            {
                constructionProgressPanel.SetActive(true);

                OnGotHit();
                unlockingMission.LinkedBuilding.SubscribeOnGotHit(OnGotHit);
                constructionIconImage.sprite = unlockingMission.LinkedBuilding.GetConstrutionIcon();
            }
        }

        private void OnGotHit()
        {
            constructionProgressText.text = unlockingMission.LinkedBuilding.HitsMade + "/" + unlockingMission.LinkedBuilding.ConstructionHitsRequired;
        }

        private void OnConstructed()
        {
            constructionProgressPanel.SetActive(false);
        }

        public override void UpdateUI()
        {
            RedrawRequiredResources();

            mission.OnUIUpdated();
        }

        public override void SetTitle(string title)
        {
            titleText.text = title;

            Vector2 prefferedValue = titleText.GetPreferredValues(title);
            panelRectTransform.sizeDelta = new Vector2(prefferedValue.x + 80, panelRectTransform.sizeDelta.y);
        }
    }

    [System.Serializable]
    public class MissionTaskUICase : MissionUICase
    {
        [Space]
        [SerializeField] TextMeshProUGUI descriptionText;

        public override void Initialise(MissionUIPanel uiPanel)
        {
            this.uiPanel = uiPanel;
        }

        public override void Activate(Mission mission)
        {
            this.mission = mission;
            // making default title empty, as description text will take the entire panel
            uiPanel.SetTitle(string.Empty);
            descriptionText.text = mission.Title;
        }

        public override void UpdateUI()
        {
            mission.OnUIUpdated();
        }

        public override void SetTitle(string title)
        {
            titleText.text = string.Empty;
        }
    }
}