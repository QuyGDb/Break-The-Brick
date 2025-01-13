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
            rotationSpeed = rotationPlatformCurve.Evaluate(GameLevel.Instance.firstPersonLevel);
        }
        if (gameState == GameState.ThirdPerson)
        {
            rotationSpeed = rotationPlatformCurve.Evaluate(GameLevel.Instance.thirdPersonLevel);
        }
    }

    private void Update()
    {
        if (GameManager.Instance.gameState == GameState.Lose || GameManager.Instance.gameState == GameState.Win)
        {
            return;
        }
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
