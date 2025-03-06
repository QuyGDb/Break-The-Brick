using System;
using UnityEngine;

public class GameManager : SingletonMonobehaviourPersistent<GameManager>
{
    public GameState previousGameState;
    public GameState gameState;
    [HideInInspector] public Action<GameState> OnGameStateChange;
    [SerializeField] private MusicTrackSO ingameMusic;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        HandleGameState(GameState.Start);
    }
    public void HandleGameState(GameState gameState)
    {
        previousGameState = this.gameState;
        this.gameState = gameState;
        switch (gameState)
        {
            case GameState.Start:
                break;
            case GameState.FirstPerson:
                MusicManager.Instance.PlayMusic(ingameMusic);
                break;
            case GameState.ThirdPerson:
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