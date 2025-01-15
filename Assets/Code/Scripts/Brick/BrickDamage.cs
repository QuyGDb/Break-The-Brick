using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickDamage : MonoBehaviour
{
    public float moveUpDistance = 3f;
    public float scaleUpFactor = 2f;
    public float duration = 0.5f;
    public float shakeMagnitude = 0.2f;
    public float shakeDuration = 0.75f;

    private void OnEnable()
    {
        // Create a sequence to combine multiple animations
        Sequence mySequence = DOTween.Sequence();

        // Move the GameObject upwards quickly with a smooth easing
        mySequence.Append(transform.DOMoveY(transform.position.y + moveUpDistance, duration * 0.5f)
            .SetEase(Ease.OutQuad));

        // Scale the GameObject up rapidly with a more pronounced "springy" easing
        mySequence.Join(transform.DOScale(transform.localScale * (scaleUpFactor * 1.5f), duration * 0.5f)
            .SetEase(Ease.OutBack));

        // Add a stronger and faster shaking effect to simulate a powerful explosion
        mySequence.Join(transform.DOShakePosition(shakeDuration * 0.5f, shakeMagnitude * 1.5f));

        // After the sequence completes, destroy the GameObject
        mySequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });


    }

    private void InitializeGameObject()
    {
        gameObject.SetActive(true);
    }
}
