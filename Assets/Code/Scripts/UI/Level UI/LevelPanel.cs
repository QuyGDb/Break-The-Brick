using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPanel : MonoBehaviour
{
    [SerializeField] private GameObject levelBtnPrefab;
    private int levelCount = 10;
    private Button[] levelBtns = new Button[10];
    private TextMeshProUGUI[] levelBtnTexts = new TextMeshProUGUI[10];
    private LevelButton[] levelButtons = new LevelButton[10];
    [SerializeField] private Transform levelContainer;
    [SerializeField] private Button NextLevels;
    [SerializeField] private Button PreviousLevels;
    private int currentPanelIndex;
    private int levelPanelCount;
    private void Awake()
    {
        InitializeButton();
        // GameManager.Instance.OnGameStateChange += HandleGameState;
    }

    private void OnDestroy()
    {
        //  GameManager.Instance.OnGameStateChange -= HandleGameState;
    }

    private void HandleGameState(GameState state)
    {
        if (state == GameState.FirstPerson)
        {
            if (PlayerPrefs.HasKey("FirstPersonLevel"))
                Settings.FirstPersonLevel = PlayerPrefs.GetInt("FirstPersonLevel");
            else
                Settings.FirstPersonLevel = 1;
            ProcessLevelEvent(CalculatelevelPanelCount(Settings.FirstPersonLevel));
            CalculatePanelIndex(Settings.FirstPersonLevel);
            CalculateLevelQuantityInLevelButton(currentPanelIndex);
        }
        if (state == GameState.ThirdPerson)
        {
            if (PlayerPrefs.HasKey("ThirdPersonLevel"))
                Settings.ThirdPersonLevel = PlayerPrefs.GetInt("ThirdPersonLevel");
            else
                Settings.ThirdPersonLevel = 1;
            ProcessLevelEvent(CalculatelevelPanelCount(Settings.ThirdPersonLevel));
            CalculatePanelIndex(Settings.ThirdPersonLevel);
            CalculateLevelQuantityInLevelButton(currentPanelIndex);
        }
    }

    private void InitializeButton()
    {
        for (int i = 0; i < levelCount; i++)
        {
            GameObject levelBtn = Instantiate(levelBtnPrefab, levelContainer);
            levelBtns[i] = levelBtn.GetComponent<Button>();
            levelBtnTexts[i] = levelBtn.GetComponentInChildren<TextMeshProUGUI>();
            levelButtons[i] = levelBtn.GetComponent<LevelButton>();
        }
    }
    private void Start()
    {
        NextLevels.onClick.AddListener(NextLevelsOnClick);
        PreviousLevels.onClick.AddListener(PreviousLevelsOnClick);
    }
    private int CalculatelevelPanelCount(int currentLevel)
    {
        float fractionalPart = (currentLevel / (float)levelCount) - currentLevel / levelCount;
        if (fractionalPart == 0)
            return 10;
        float currentLevelIndexFloat = fractionalPart * levelCount;
        return Mathf.RoundToInt(currentLevelIndexFloat);
    }
    private void CalculatePanelIndex(int currentLevel)
    {
        levelPanelCount = Mathf.CeilToInt((float)currentLevel / levelCount);
        currentPanelIndex = levelPanelCount;
    }
    private void CalculateLevelQuantityInLevelButton(int currentPanelIndex)
    {
        for (int i = 0; i < levelCount; i++)
        {
            levelBtnTexts[i].text = (i + 1 + ((currentPanelIndex - 1) * levelCount)).ToString();
        }
    }

    private void ProcessLevelEvent(int currentLevelIndex)
    {
        for (int i = 0; i < levelCount; i++)
        {
            if (i < currentLevelIndex)
            {
                levelBtns[i].interactable = true;
                levelButtons[i].lockImage.gameObject.SetActive(false);
            }
            else
            {
                levelBtns[i].interactable = false;
                levelButtons[i].lockImage.gameObject.SetActive(true);
            }
        }

    }
    private void NextLevelsOnClick()
    {
        if (currentPanelIndex < levelPanelCount)
        {
            currentPanelIndex++;
            levelContainer.transform.DOScale(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                levelContainer.transform.DOScale(new Vector3(1, 1, 1), 0.2f).SetEase(Ease.OutBounce);
            });
            if (currentPanelIndex == levelPanelCount)
            {
                if (GameManager.Instance.gameState == GameState.FirstPerson)
                    ProcessLevelEvent(CalculatelevelPanelCount(Settings.FirstPersonLevel));
                else
                    ProcessLevelEvent(CalculatelevelPanelCount(Settings.ThirdPersonLevel));
            }
            else
                ProcessLevelEvent(currentPanelIndex * levelCount);
            CalculateLevelQuantityInLevelButton(currentPanelIndex);
        }
    }

    private void PreviousLevelsOnClick()
    {
        if (currentPanelIndex > 1)
        {
            currentPanelIndex--;
            levelContainer.transform.DOScale(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                levelContainer.transform.DOScale(new Vector3(1, 1, 1), 0.2f).SetEase(Ease.OutBounce);
            });
            ProcessLevelEvent(currentPanelIndex * levelCount);
            CalculateLevelQuantityInLevelButton(currentPanelIndex);
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(levelBtnPrefab), levelBtnPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(levelContainer), levelContainer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(NextLevels), NextLevels);
        HelperUtilities.ValidateCheckNullValue(this, nameof(PreviousLevels), PreviousLevels);
    }
#endif
    #endregion
}
