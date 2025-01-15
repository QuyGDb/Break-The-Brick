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
    [SerializeField] private Image settingPanel;
    [SerializeField] private SoundEffectSO buttonClick;
    [SerializeField] private MusicTrackSO musicMenu;


    private void Start()
    {
        MusicManager.Instance.PlayMusic(musicMenu);
        firstPersonButton.onClick.AddListener(() =>
        {
            SetPlayMode(PlayMode.FirstPerson);
            SoundEffectManager.Instance.PlaySoundEffect(buttonClick);
            StartMainScene();

        });
        thirdPersonButton.onClick.AddListener(() =>
        {
            SoundEffectManager.Instance.PlaySoundEffect(buttonClick);
            SetPlayMode(PlayMode.ThirdPerson);
            StartMainScene();
        });
        settingButton.onClick.AddListener(() =>
        {
            SoundEffectManager.Instance.PlaySoundEffect(buttonClick);
            if (settingPanel.gameObject.activeSelf)
            {
                settingPanel.gameObject.SetActive(false);
            }
            else
            {
                settingPanel.transform.DOScale(new Vector3(0.8f, 0.6f, 0.6f), 1f).SetEase(Ease.OutBounce);
                settingPanel.gameObject.SetActive(true);
            }
        });
    }

    private void SetPlayMode(PlayMode playMode)
    {
        Settings.playMode = playMode;
    }

    public void StartMainScene()
    {

        SceneManager.LoadScene("MainScene");
        SceneManager.UnloadSceneAsync(0);
    }
}
