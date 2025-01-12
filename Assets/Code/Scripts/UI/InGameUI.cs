using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private Slider brickSlider;
    [SerializeField] private GameObject endGamePanel;
    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChange += HandleGameState;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChange -= HandleGameState;
    }

    private void HandleGameState(GameState state)
    {
        if (state == GameState.Win)
        {
            brickSlider.DOValue(0, 1f);
            endGamePanel.SetActive(true);
            brickSlider.gameObject.SetActive(false);
            endGamePanel.transform.DOScale(new Vector3(0.6f, 0.6f, 0.6f), 1f).SetEase(Ease.OutSine);
        }
    }

    private void UpdateBrickCount()
    {
        brickSlider.DOValue(Settings.brickCount, 1f);
    }
}
