using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerControls : MonoBehaviour
{
    private PlayerAnimation playerAnimation;
    private PlayerAtributes playerAtributes;
    [SerializeField] private float countdownTime = 0.5f;
    private float currentCountdown = 0f;
    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
    }
    void Update()
    {
        if (currentCountdown > 0f)
        {
            currentCountdown -= Time.deltaTime;
        }

#if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began && currentCountdown <= 0f)
                {
                    playerAnimation.TriggerChopAnim();
                    currentCountdown = countdownTime;
                    playerAtributes.TrackChopCount();
                }
            }
#endif

#if UNITY_STANDALONE_WIN
        if (Input.GetMouseButtonDown(0) && currentCountdown <= 0f)
        {
            playerAnimation.TriggerChopAnim();
            currentCountdown = countdownTime;
            playerAtributes.TrackChopCount();
        }
#endif
    }
}

