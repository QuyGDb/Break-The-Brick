using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;


public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerManager playerManager;
    [SerializeField] private SoundEffectSO cream;
    [SerializeField] private AtributeByCurve speedCurveSO;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerManager = GetComponent<PlayerManager>();
    }
    private void Start()
    {
        //animator.speed = playerManager.speed;
        animator.speed = speedCurveSO.speedCurve.Evaluate(speedCurveSO.speedLevel);
    }
    public void TriggerChopAnim()
    {
        animator.SetTrigger(Settings.isChop);
        SoundEffectManager.Instance.PlaySoundEffect(cream);
    }


}
