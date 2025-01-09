using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PlayerControls : MonoBehaviour
    {
        private PlayerAnimation playerAnimation;

        private void Awake()
        {
            playerAnimation = GetComponent<PlayerAnimation>();
        }
        void Update()
        {
            if (Input.touchCount > 0)
            {
                // Check if the first touch has begun
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    playerAnimation.TriggerChopAnim();
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                playerAnimation.TriggerChopAnim();
            }
        }
    }
}
