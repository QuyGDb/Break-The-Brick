using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;

    private void OnEnable()
    {
        StaticEventHandler.OnBrickDestroy += StaticEventHandler_OnBrickDestroy;
    }
    private void OnDisable()
    {
        StaticEventHandler.OnBrickDestroy -= StaticEventHandler_OnBrickDestroy;
    }

    private void StaticEventHandler_OnBrickDestroy()
    {
        if (GameManager.Instance.gameState == GameState.FirstPerson)
        {
            HelperUtilities.ShakeCamera(firstPersonCamera);
        }
        else if (GameManager.Instance.gameState == GameState.ThirdPerson)
        {
            HelperUtilities.ShakeCamera(thirdPersonCamera);
        }
    }

    public void StartFirstPersonMode()
    {
        firstPersonCamera.enabled = true;
        thirdPersonCamera.enabled = false;
    }
    public void StartThirdPersonMode()
    {
        firstPersonCamera.enabled = false;
        thirdPersonCamera.enabled = true;
    }


}
