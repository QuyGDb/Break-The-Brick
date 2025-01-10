using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
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
