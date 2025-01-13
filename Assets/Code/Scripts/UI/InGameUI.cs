using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private Slider brickSlider;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TextMeshProUGUI statusEndgame;
    [SerializeField] private TextMeshProUGUI brickCount;
    [SerializeField] private List<Image> levelImage;
    private int numberOfBrick;
    private int brickCountValue;
    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChange += HandleGameState;
        StaticEventHandler.OnBrickCount += UpdateBrickCount;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChange -= HandleGameState;
        StaticEventHandler.OnBrickCount -= UpdateBrickCount;
    }

    private void HandleGameState(GameState state)
    {
        if (state == GameState.Win)
        {
            ShowEndgamePanel();
            statusEndgame.text = "You Win!";
            brickCount.text = $"Brick Count: {brickCountValue}/{numberOfBrick}";
            ShowStartIcon(state);
        }
        if (state == GameState.Lose)
        {
            ShowEndgamePanel();
            statusEndgame.text = "You Lose!";
            brickCount.text = $"Brick Count: {brickCountValue}/{numberOfBrick}";
            ShowStartIcon(state);
        }
    }
    private void ShowEndgamePanel()
    {
        brickSlider.DOValue(0, 1f);
        endGamePanel.SetActive(true);
        brickSlider.gameObject.SetActive(false);
        endGamePanel.transform.DOScale(new Vector3(0.6f, 0.6f, 0.6f), 1f).SetEase(Ease.OutSine);
    }
    private void UpdateBrickCount(int count, int maxBrick)
    {
        brickCountValue = count;
        numberOfBrick = maxBrick;
        brickSlider.DOValue(count, 1f);
    }
    private void ShowStartIcon(GameState gameState)
    {
        if (gameState == GameState.Win)
        {
            if (brickCountValue == numberOfBrick)
            {
                foreach (var item in levelImage)
                {
                    item.enabled = true;
                }
            }
            if (brickCountValue == numberOfBrick - 1)
            {
                levelImage[0].enabled = true;
                levelImage[1].enabled = true;
            }
            if (brickCountValue == numberOfBrick - 2)
            {
                levelImage[0].enabled = true;
            }
        }
        if (gameState == GameState.Lose)
        {
            foreach (var item in levelImage)
            {
                item.enabled = true;
                item.DOColor(Color.black, 1f);
            }
        }
    }
}
