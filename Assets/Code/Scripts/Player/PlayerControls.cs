using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControls : MonoBehaviour
{
    private PlayerAnimation playerAnimation;
    private PlayerManager playerManager;
    [SerializeField] private float countdownTime = 1.25f;
    [SerializeField] private float currentCountdown = 0f;
    public bool isChop = true;
    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        playerManager = GetComponent<PlayerManager>();
    }
    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChange += HandleGameState;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChange -= HandleGameState;
    }
    private void HandleGameState(GameState state)
    {
        if (state == GameState.Win || state == GameState.Lose)
        {
            isChop = true;
        }
    }
    void Update()
    {
        if (isChop) return;
        bool isMouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool isTouchStarted = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if ((isMouseClicked || isTouchStarted) && currentCountdown <= 0f)
        {
            DoChop();
        }
        if (currentCountdown > 0f)
            currentCountdown -= Time.deltaTime;
    }
    private void DoChop()
    {
        Settings.isTrigger = true;
        currentCountdown = countdownTime;
        playerAnimation.TriggerChopAnim();
        if (playerManager != null)
            playerManager.TrackChopCount();
    }
}

