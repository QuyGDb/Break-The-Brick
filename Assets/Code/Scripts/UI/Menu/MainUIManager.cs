using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    [SerializeField] private Button firstPersonButton;
    [SerializeField] private Button thirdPersonButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private Image settingPanel;
    [SerializeField] private Button exitSettingButton;
    [SerializeField] private SoundEffectSO buttonClick;
    [SerializeField] private MusicTrackSO musicMenu;


    private void Start()
    {
        exitSettingButton.onClick.AddListener(() =>
        {
            settingPanel.gameObject.SetActive(!settingPanel.gameObject.activeSelf);
        });
        MusicManager.Instance.PlayMusic(musicMenu);
        firstPersonButton.onClick.AddListener(() =>
        {
            SetPlayMode(PlayMode.FirstPerson);
            SoundEffectManager.Instance.PlaySoundEffect(buttonClick);
            ToggleSettingPanel();
        });
        thirdPersonButton.onClick.AddListener(() =>
        {
            SoundEffectManager.Instance.PlaySoundEffect(buttonClick);
            SetPlayMode(PlayMode.ThirdPerson);
            ToggleSettingPanel();
        });
        settingButton.onClick.AddListener(() =>
        {
            SoundEffectManager.Instance.PlaySoundEffect(buttonClick);
            if (settingPanel.gameObject.activeSelf)
            {

                settingPanel.gameObject.SetActive(false);
                settingPanel.transform.DOScale(new Vector3(0f, 0f, 0f), 1f).SetEase(Ease.OutBounce);
            }
            else if (!settingPanel.gameObject.activeSelf)
            {
                settingPanel.gameObject.SetActive(true);
                settingPanel.transform.DOScale(new Vector3(0.8f, 0.6f, 0.6f), 1f).SetEase(Ease.OutBounce);
            }
        });
    }

    private void SetPlayMode(PlayMode playMode)
    {
        StopAllCoroutines();
        StartCoroutine(CallSetPlayerMode(playMode));
    }

    IEnumerator CallSetPlayerMode(PlayMode playMode)
    {
        yield return null;
        GameManager.Instance.HandleGameState(gameState: playMode == PlayMode.FirstPerson ? GameState.FirstPerson : GameState.ThirdPerson);
    }
    public void ToggleSettingPanel()
    {
        if (levelPanel.gameObject.activeSelf)
        {
            levelPanel.gameObject.SetActive(false);
            levelPanel.transform.DOScale(new Vector3(0f, 0f, 0f), 1f).SetEase(Ease.OutBounce);
        }
        else if (!levelPanel.gameObject.activeSelf)
        {
            levelPanel.gameObject.SetActive(true);
            levelPanel.transform.DOScale(new Vector3(1f, 1f, 1f), 1f).SetEase(Ease.OutBounce);
        }
    }
}


