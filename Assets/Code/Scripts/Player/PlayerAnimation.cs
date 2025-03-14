using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;


public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerManager playerManager;
    [SerializeField] private SoundEffectSO cream;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerManager = GetComponent<PlayerManager>();
    }
    private void Start()
    {
        if (playerManager != null)
            animator.speed = playerManager.atributes.speed;
    }
    public void TriggerChopAnim()
    {
        animator.SetTrigger(Settings.isChop);
        SoundEffectManager.Instance.PlaySoundEffect(cream);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(cream), cream);
    }
#endif
    #endregion
}
