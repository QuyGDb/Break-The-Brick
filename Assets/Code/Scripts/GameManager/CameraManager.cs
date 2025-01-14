using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera firstPersonCamera;
    Vector3 firstPersonCameraPosition;
    public Camera thirdPersonCamera;
    public Vector3 thirdPersonCameraPosition;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] float strength = 1f;
    [SerializeField] int vibrato = 10;
    float randomness = 90f;
    private void OnEnable()
    {
        StaticEventHandler.OnBrickDestroy += StaticEventHandler_OnBrickDestroy;
    }
    private void OnDisable()
    {
        StaticEventHandler.OnBrickDestroy -= StaticEventHandler_OnBrickDestroy;
    }
    private void Awake()
    {
        firstPersonCameraPosition = firstPersonCamera.transform.position;
        thirdPersonCameraPosition = thirdPersonCamera.transform.position;
    }

    private void StaticEventHandler_OnBrickDestroy(float percentage)
    {
        if (GameManager.Instance.gameState == GameState.FirstPerson)
        {
            ShakeCamera(firstPersonCamera);
        }
        else if (GameManager.Instance.gameState == GameState.ThirdPerson)
        {
            ShakeCamera(thirdPersonCamera);
        }
    }
    public void ShakeCamera(Camera camera)
    {

        camera.transform.position = transform.position;
        camera.transform.DOShakePosition(duration, strength, vibrato, randomness).OnComplete(() =>
        {
            if (camera == firstPersonCamera)
            {
                camera.transform.position = firstPersonCameraPosition;
            }
            else if (camera == thirdPersonCamera)
            {
                camera.transform.position = thirdPersonCameraPosition;
            }
        });
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
