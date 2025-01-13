using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    RayfireBomb bomb;
    WaitForSeconds wait = new WaitForSeconds(0.1f);
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
        StopAllCoroutines();
        StartCoroutine(BrickDestroyCoroutine());
    }
    private IEnumerator BrickDestroyCoroutine()
    {
        yield return wait;

        bomb.Explode(0);

    }
}
