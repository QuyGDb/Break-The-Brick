using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerControls : MonoBehaviour
{
    private PlayerAnimation playerAnimation;
    private PlayerManager playerManager;
    [SerializeField] private float countdownTime = 1.25f;
    [SerializeField] private float currentCountdown = 0f;
    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        playerManager = GetComponent<PlayerManager>();
    }
    void Update()
    {

        if (Input.GetMouseButtonDown(0) && currentCountdown <= 0f)
        {
            Test();
        }
        if (currentCountdown > 0f)
            currentCountdown -= Time.deltaTime;
    }
    private void Test()
    {
        Settings.isTrigger = true;
        currentCountdown = countdownTime;
        playerAnimation.TriggerChopAnim();
        playerManager.TrackChopCount();
    }
}

