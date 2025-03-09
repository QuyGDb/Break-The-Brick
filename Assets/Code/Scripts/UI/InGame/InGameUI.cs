using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private Slider brickSlider;
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private Transform chopPanel;
    [SerializeField] private TextMeshProUGUI statusEndgame;
    [SerializeField] private TextMeshProUGUI brickCountText;
    [SerializeField] private TextMeshProUGUI chopCountText;
    [SerializeField] private List<Image> levelImage;
    [SerializeField] private Button backToMenu;
    [SerializeField] private SoundEffectSO exitSoundEffect;
    private int numberOfBrick;
    private int brickCountValue;
    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChange += HandleGameState;
        StaticEventHandler.OnChopCount += UpdateChopCount;
        StaticEventHandler.OnBrickCount += UpdateBrickCount;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChange -= HandleGameState;
        StaticEventHandler.OnChopCount -= UpdateChopCount;
        StaticEventHandler.OnBrickCount -= UpdateBrickCount;
    }

    private void UpdateBrickCount(int brickCount, int maxBrick)
    {
        brickCountValue = brickCount;
        numberOfBrick = maxBrick;
    }

    private void Awake()
    {
        backToMenu.onClick.AddListener(() =>
        {
            SoundEffectManager.Instance.PlaySoundEffect(exitSoundEffect);
            SceneManager.LoadScene(0);
        });
    }
    private void UpdateChopCount(int chopCount, int maxChop)
    {
        chopCountText.text = $"{chopCount}/{maxChop}";
        chopPanel.DOShakeScale(duration: 1f, strength: new Vector3(1f, 1f, 0f), vibrato: 8, randomness: 60);
    }

    private void HandleGameState(GameState state)
    {

        if (state == GameState.Win)
        {
            ShowEndgamePanel();
            statusEndgame.text = "You Win!";
            brickCountText.text = $"Brick Count: {brickCountValue}/{numberOfBrick}";
            ShowStartIcon(state);
            if (GameManager.Instance.previousGameState == GameState.FirstPerson)
            {
                if (Settings.FirstPersonLevel <= 10)
                {
                    Settings.FirstPersonLevel++;
                    PlayerPrefs.SetInt("FirstPersonLevel", Settings.FirstPersonLevel);
                }
            }
            if (GameManager.Instance.previousGameState == GameState.ThirdPerson)
            {
                if (Settings.ThirdPersonLevel <= 10)
                {
                    Settings.ThirdPersonLevel++;
                    PlayerPrefs.SetInt("ThirdPersonLevel", Settings.ThirdPersonLevel);
                }
            }
        }
        if (state == GameState.Lose)
        {
            ShowEndgamePanel();
            statusEndgame.text = "You Lose!";
            brickCountText.text = $"Brick Count: {brickCountValue}/{numberOfBrick}";
            ShowStartIcon(state);
        }
    }
    private void ShowEndgamePanel()
    {
        brickSlider.DOValue(0, 1f);
        endGamePanel.SetActive(true);
        endGamePanel.transform.DOScale(new Vector3(0.6f, 0.6f, 0.6f), 1f).SetEase(Ease.OutBounce);
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

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(brickSlider), brickSlider);
        HelperUtilities.ValidateCheckNullValue(this, nameof(endGamePanel), endGamePanel);
        HelperUtilities.ValidateCheckNullValue(this, nameof(chopPanel), chopPanel);
        HelperUtilities.ValidateCheckNullValue(this, nameof(statusEndgame), statusEndgame);
        HelperUtilities.ValidateCheckNullValue(this, nameof(brickCountText), brickCountText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(chopCountText), chopCountText);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(levelImage), levelImage);
        HelperUtilities.ValidateCheckNullValue(this, nameof(backToMenu), backToMenu);
        HelperUtilities.ValidateCheckNullValue(this, nameof(exitSoundEffect), exitSoundEffect);
    }
#endif
    #endregion

}
