using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    RayfireBomb bomb;

    private void Awake()
    {
        bomb = GetComponent<RayfireBomb>();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnBrickDestroy += HandleBrickDestroy;

    }

    private void OnDisable()
    {
        StaticEventHandler.OnBrickDestroy -= HandleBrickDestroy;
    }

    private void HandleBrickDestroy()
    {
        bomb.Explode(100);
    }
}
