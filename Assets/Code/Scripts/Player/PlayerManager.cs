using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public PlayerControls playerControls;
    [HideInInspector] public PlayerAtributes playerAtributes;
    [HideInInspector] public PlayerAnimation playerAnimation;

    private void Awake()
    {
        playerControls = GetComponent<PlayerControls>();
        playerAtributes = GetComponent<PlayerAtributes>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }
    private void OnTriggerEnter(Collider other)
    {
        //Shatter brick    
    }
}
