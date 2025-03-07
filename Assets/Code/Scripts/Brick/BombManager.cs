using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    RayfireBomb bomb;
    WaitForSeconds waitForExplode = new WaitForSeconds(0.05f);
    WaitForSeconds waitForSetActive = new WaitForSeconds(2f);
    [SerializeField] private Vector3 fisrtPositionOfBomb;
    [SerializeField] private Vector3 secondPositionOfBomb;
    [SerializeField] SoundEffectSO broken;
    private void Awake()
    {
        bomb = GetComponent<RayfireBomb>();
    }


    public void HandleBrickDestroy(float percentage)
    {

        if (percentage <= 0)
        {
            transform.localPosition = secondPositionOfBomb;
        }
        StopAllCoroutines();
        StartCoroutine(BrickDestroyCoroutine(percentage));
    }
    private IEnumerator BrickDestroyCoroutine(float percentage)
    {
        yield return waitForExplode;
        bomb.Explode(0);
        SoundEffectManager.Instance.PlaySoundEffect(broken);
        yield return waitForSetActive;
        if (percentage <= 0)
        {
            transform.parent.gameObject.SetActive(false);
        }
    }
}
