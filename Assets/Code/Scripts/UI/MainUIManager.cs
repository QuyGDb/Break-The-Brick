using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    [SerializeField] private Button firstPersonButton;
    [SerializeField] private Button thirdPersonButton;

    private void Start()
    {
        firstPersonButton.onClick.AddListener(() =>
        {
            SetPlayMode(PlayMode.FirstPerson);
            StartMainScene();

        });
        thirdPersonButton.onClick.AddListener(() =>
        {
            SetPlayMode(PlayMode.ThirdPerson);
            StartMainScene();
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
