using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    RayfireBomb bomb;
    WaitForSeconds wait = new WaitForSeconds(0.05f);
    WaitForSeconds wait1 = new WaitForSeconds(1f);
    [SerializeField] private Vector3 fisrtPositionOfBomb;
    [SerializeField] private Vector3 secondPositionOfBomb;
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

    private void HandleBrickDestroy(float percentage)
    {
        if (percentage <= 0)
        {
            transform.localPosition = secondPositionOfBomb;
        }
        StopAllCoroutines();
        StartCoroutine(BrickDestroyCoroutine());
    }
    private IEnumerator BrickDestroyCoroutine()
    {
        yield return wait;

        bomb.Explode(0);
        yield return wait1;
        gameObject.SetActive(false);

    }
}
