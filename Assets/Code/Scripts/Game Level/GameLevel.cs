using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevel : SingletonMonobehaviour<GameLevel>
{
    public int firstPersonLevel;
    public int thirdPersonLevel;
    protected override void Awake()
    {
        base.Awake();
        LoadLevelFromPlayerPrefs();
    }
    private void LoadLevelFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("firstPersonLevel"))
        {
            firstPersonLevel = PlayerPrefs.GetInt("firstPersonLevel");
        }
        else
        {
            firstPersonLevel = 1;
        }
        if (PlayerPrefs.HasKey("thirdPersonLevel"))
        {
            thirdPersonLevel = PlayerPrefs.GetInt("thirdPersonLevel");
        }
        else
        {
            thirdPersonLevel = 1;
        }
    }
    public void UpdateFirstPersonLevel()
    {
        firstPersonLevel++;
        PlayerPrefs.SetInt("firstPersonLevel", firstPersonLevel);
        PlayerPrefs.Save();
    }
    public void UpdateThirdPersonLevel()
    {
        thirdPersonLevel++;
        PlayerPrefs.SetInt("thirdPersonLevel", thirdPersonLevel);
        PlayerPrefs.Save();
    }
}
