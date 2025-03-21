using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlatformRotationController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] private AnimationCurve rotationSpeedPlatformCurve;

    private bool isRotating = true;
    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChange += HandleGameState;
        StaticEventHandler.OnRotatePlatform += StopRotatePlatform;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChange -= HandleGameState;
        StaticEventHandler.OnRotatePlatform -= StopRotatePlatform;
    }

    private void StopRotatePlatform(bool isRotate)
    {
        isRotating = isRotate;
    }

    private void HandleGameState(GameState gameState)
    {
        if (gameState == GameState.FirstPerson)
        {
            rotationSpeed = rotationSpeedPlatformCurve.Evaluate(Settings.FirstPersonLevel);
        }
        if (gameState == GameState.ThirdPerson)
        {
            rotationSpeed = rotationSpeedPlatformCurve.Evaluate(Settings.ThirdPersonLevel);
        }
    }

    private void Update()
    {
        if (GameManager.Instance.gameState == GameState.Lose || GameManager.Instance.gameState == GameState.Win)
            return;
        if (isRotating)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
