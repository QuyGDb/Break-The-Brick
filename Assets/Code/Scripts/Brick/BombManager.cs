using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    RayfireBomb bomb;
    WaitForSeconds wait = new WaitForSeconds(0.05f);
    WaitForSeconds wait1 = new WaitForSeconds(2f);
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
        yield return wait;
        bomb.Explode(0);
        // n?u không start scene menu, soundmanager không ?c t?o , l?i nên sau yield không ???c g?i
        SoundEffectManager.Instance.PlaySoundEffect(broken);
        yield return wait1;
        Debug.Log("Brick Destroyed" + percentage);

        if (percentage <= 0)
        {
            Debug.Log("Brick Destroyed");
            transform.parent.gameObject.SetActive(false);
        }
    }
}
