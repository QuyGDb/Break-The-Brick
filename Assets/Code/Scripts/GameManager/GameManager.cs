using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonobehaviour<GameManager>
{
    public GameState gameState;
    [HideInInspector] public Action<GameState> OnGameStateChange;
    private CameraManager cameraManager;
    public PlayerManager playerManager;

    protected override void Awake()
    {
        base.Awake();
        cameraManager = GetComponent<CameraManager>();
    }
    private void Start()
    {
        HandleGameState(GameState.Start);
    }
    public void HandleGameState(GameState gameState)
    {
        this.gameState = gameState;
        switch (gameState)
        {
            case GameState.Start:
                StartCoroutine(StartPlayMode());
                break;
            case GameState.FirstPerson:
                Debug.Log("First Person");
                cameraManager.StartFirstPersonMode();
                break;
            case GameState.ThirdPerson:
                Debug.Log("Third Person");
                cameraManager.StartThirdPersonMode();
                break;
            case GameState.Win:

                break;
            case GameState.Lose:
                break;
            default:
                break;
        }
        OnGameStateChange?.Invoke(gameState);
    }

    private IEnumerator StartPlayMode()
    {
        yield return null;
        if (Settings.playMode == PlayMode.FirstPerson)
        {
            // Start First Person Mode
            HandleGameState(GameState.FirstPerson);
        }
        else if (Settings.playMode == PlayMode.ThirdPerson)
        {
            // Start Third Person Mode
            HandleGameState(GameState.ThirdPerson);
        }
    }
}