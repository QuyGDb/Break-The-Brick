#pragma warning disable CS0414 

using UnityEngine;

namespace Watermelon
{
    public class MissionsController : MonoBehaviour
    {
        [SerializeField] bool autoCompleteMissions = true;
        public static bool AutoCompleteMissions => instance.autoCompleteMissions;

        private static Mission[] missions;

        private static Mission activeMission;
        public static Mission ActiveMission => activeMission;

        private static MissionUIPanel missionUIPanel;
        private static UIMissionRewardPopUp missionRewardPopUp;

        private static TweenCase completeTweenCase;

        public static event SimpleCallback OnNextMissionStarted;
        public static event SimpleCallback OnMissionFinished;

        private static MissionsController instance;

        private static bool isInitialised => missions != null;
        private static bool fistTimeLoadingMission = false;

        private void Awake()
        {
            instance = this;
        }

        public void Initialise(Mission[] missions)
        {
            if (missions == null)
                return;

            // if missions are disabled using Actions menu - works only in the editor
            if (MissionsActionMenu.AreMissionsDisabled())
                return;

            MissionsController.missions = missions;

            // Get game ui and initialise missions panel
            UIGame gameUI = UIController.GetPage<UIGame>();

            missionUIPanel = gameUI.MissionUIPanel;
            missionUIPanel.Initialise();

            missionRewardPopUp = gameUI.MissionRewardPopUp;

            for (int i = 0; i < missions.Length; i++)
            {
                missions[i].Initialise();
            }
        }

        private static void Activate(int index)
        {
            if (activeMission != null)
                activeMission.Deactivate();

            activeMission = missions[index];
            activeMission.Activate();

            missionUIPanel.ActivateMission(activeMission);

#if UNITY_EDITOR
            if (fistTimeLoadingMission)
            {
                WorldController.UpdateWorldSave(activeMission.gameObject.name);
                SavePresets.CreateSave(WorldController.CurrentWorld.Scene.Name + " " + activeMission.gameObject.name, "Missions", activeMission.ID);
            }
#endif
        }

        public bool DoesNextMissionExist()
        {
            int activationMissionIndex = 0;

            for (int i = 0; i < missions.Length; i++)
            {
                if (missions[i].MissionStage == Mission.Stage.Collected)
                    activationMissionIndex = i + 1;
            }

            if (missions.IsInRange(activationMissionIndex))
            {
                if (activeMission != missions[activationMissionIndex])
                {
                    return true;
                }
            }

            return false;
        }

        public static void CompleteMission()
        {
            completeTweenCase.KillActive();

            if (activeMission != null)
            {
                if (activeMission.MissionStage == Mission.Stage.Finished)
                {
                    activeMission.ApplyReward(1.0f);

                    missionUIPanel.OnRewardClaimed();
                    missionRewardPopUp.Hide();

                    fistTimeLoadingMission = true;

                    Tween.DelayedCall(3f, () =>
                    {
                        ActivateNextMission();
                    });
                }
            }
        }

        public static void AutoCompleteMission(float duration)
        {
            completeTweenCase.KillActive();

            // autocomplete is available only for mission with resources
            if (activeMission != null && activeMission.RewardType == MissionRewardType.Resources)
            {
                completeTweenCase = Tween.DelayedCall(duration, () =>
                {
                    CompleteMission();
                });
            }
        }

        public static int GetLastCompletedMissionIndex()
        {
            int activationMissionIndex = 0;

            for (int i = 0; i < missions.Length; i++)
            {
                if (missions[i].MissionStage == Mission.Stage.Collected)
                    activationMissionIndex = i + 1;
            }

            return activationMissionIndex;
        }

        public static Mission GetMissionById(string id)
        {
            if (missions == null)
                return null;

            return missions.Find(m => m.ID.Equals(id));
        }

        public static void ActivateNextMission()
        {
            if (!isInitialised)
                return;

            int activationMissionIndex = 0;

            for (int i = 0; i < missions.Length; i++)
            {
                if (missions[i].MissionStage == Mission.Stage.Collected)
                    activationMissionIndex = i + 1;
            }

            if (missions.IsInRange(activationMissionIndex))
            {
                if (activeMission != missions[activationMissionIndex])
                {
                    if (activationMissionIndex == 0)
                    {
                        fistTimeLoadingMission = true;
                    }

                    Activate(activationMissionIndex);

                    OnNextMissionStarted?.Invoke();
                }
            }
            // if last mission is completed
            else
            {
                missionUIPanel.gameObject.SetActive(false);
            }
        }

        public static void MissionFinished()
        {
            OnMissionFinished?.Invoke();

            Tween.DelayedCall(0.5f, () =>
            {
                missionRewardPopUp.OnMissionFinished(activeMission);
            });
        }

        public static void Unload()
        {
            if (!isInitialised)
                return;

            activeMission?.Deactivate();
            missions = null;
        }
    }

    [System.Serializable]
    public abstract class MissionSave : ISaveObject
    {
        [SerializeField] Mission.Stage missionStage;
        public Mission.Stage MissionStage => missionStage;

        [SerializeField] string missionID;
        public string MissionID => missionID;

        [System.NonSerialized]
        private Mission mission;

        public virtual void Flush()
        {
            missionStage = mission.MissionStage;
        }

        public void LinkMission(Mission mission)
        {
            this.mission = mission;
            missionID = mission.ID;
        }
    }
}
