using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickDamage : MonoBehaviour
{
    public float moveUpDistance = 2f;
    public float scaleUpFactor = 2f;
    public float duration = 0.5f;
    public float shakeMagnitude = 0.1f;
    public float shakeDuration = 0.5f;

    private void OnEnable()
    {
        // Sequence to combine animations
        Sequence mySequence = DOTween.Sequence();

        // Move up with ease
        mySequence.Append(transform.DOMoveY(transform.position.y + moveUpDistance, duration)
            .SetEase(Ease.OutQuad));

        // Scale up with ease
        mySequence.Join(transform.DOScale(transform.localScale * scaleUpFactor, duration)
            .SetEase(Ease.OutBack));

        // Shake position
        mySequence.Join(transform.DOShakePosition(shakeDuration, shakeMagnitude));

        // On complete, destroy the GameObject
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
