using System;
using UnityEngine;

public class GameManager : SingletonMonobehaviourPersistent<GameManager>
{
    public GameState gameState;
    [HideInInspector] public Action<GameState> OnGameStateChange;
    private CameraManager cameraManager;
    [SerializeField] private MusicTrackSO ingameMusic;

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
                break;
            case GameState.FirstPerson:
                cameraManager.StartFirstPersonMode();
                MusicManager.Instance.PlayMusic(ingameMusic);
                break;
            case GameState.ThirdPerson:
                cameraManager.StartThirdPersonMode();
                MusicManager.Instance.PlayMusic(ingameMusic);
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


}