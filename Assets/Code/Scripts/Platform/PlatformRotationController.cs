using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlatformRotationController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] private AnimationCurve rotationPlatformCurve;

    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChange += HandleGameState;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChange -= HandleGameState;
    }

    private void HandleGameState(GameState gameState)
    {
        if (gameState == GameState.FirstPerson)
        {
            rotationSpeed = rotationPlatformCurve.Evaluate(Settings.firstPersonLevel);
            Debug.Log("Rotation Speed: " + rotationSpeed);
        }
        if (gameState == GameState.ThirdPerson)
        {
            rotationSpeed = rotationPlatformCurve.Evaluate(Settings.thirdPersonLevel);
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
