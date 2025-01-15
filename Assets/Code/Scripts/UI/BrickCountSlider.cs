using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BrickCountSlider : MonoBehaviour
{
    Slider brickSlider;
    [SerializeField] Image[] iconArr = new Image[6];
    private void Awake()
    {
        brickSlider = GetComponentInChildren<Slider>();
    }
    private void OnEnable()
    {
        StaticEventHandler.OnBrickCount += UpdateBrickCount;
        GameManager.Instance.OnGameStateChange += HandleGameState;
    }
    private void OnDisable()
    {
        StaticEventHandler.OnBrickCount -= UpdateBrickCount;
        GameManager.Instance.OnGameStateChange -= HandleGameState;
    }
    private void Start()
    {
        foreach (var icon in iconArr)
        {
            icon.enabled = false;
        }
    }
    private void HandleGameState(GameState state)
    {
        if (state == GameState.Win || state == GameState.Lose)
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateBrickCount(int count, int maxBrick)
    {

        brickSlider.DOValue(count, 1f).OnComplete(() =>
        {
            for (int i = 0; i < count; i++)
            {
                iconArr[i].enabled = true;
            }
            for (int i = count; i < iconArr.Length; i++)
            {
                iconArr[i].enabled = false;
            }
        });
    }
}
