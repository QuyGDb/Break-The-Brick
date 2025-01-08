using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [DefaultExecutionOrder(20)]
    public class ObjectEnablerByMission : MonoBehaviour
    {
        [MissionPicker]
        [SerializeField] string requiredMissionId;

        private Mission missionRef;

        private bool subscribedToTheMission;

        private void OnEnable()
        {
            Tween.NextFrame(() =>
            {
                missionRef = MissionsController.GetMissionById(requiredMissionId);

                if (missionRef == null)
                {
                    DestroyComponent();
                }
                else
                {
                    // do nothing if mission is completed
                    if (missionRef.MissionStage == Mission.Stage.Collected)
                    {
                        missionRef = null;
                        DestroyComponent();
                    }
                    // do nothing if object was already disabled by something (so it will be enabled by the other system as well)
                    else if (!gameObject.activeSelf)
                    {
                        missionRef = null;
                        DestroyComponent();
                    }
                    // subscribing to the mission update events
                    else
                    {
                        if (!subscribedToTheMission)
                            missionRef.OnStageChanged += OnStageChanged;

                        gameObject.SetActive(false);

                        subscribedToTheMission = true;
                    }
                }
            });
        }

        private void OnStageChanged(Mission.Stage previousStage, Mission.Stage currentStage)
        {
            if (currentStage == Mission.Stage.Collected)
            {
                if (missionRef != null)
                    missionRef.OnStageChanged -= OnStageChanged;

                if (gameObject != null)
                    gameObject.SetActive(true);

                DestroyComponent();
            }
        }

        private void DestroyComponent()
        {
            if (missionRef != null && subscribedToTheMission)
            {
                missionRef.OnStageChanged -= OnStageChanged;
            }
        }
    }
}
