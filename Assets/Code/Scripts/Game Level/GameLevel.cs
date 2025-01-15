using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevel : SingletonMonobehaviour<GameLevel>
{
    public int firstPersonLevel;
    public int thirdPersonLevel;
    public AtributeByCurve atributeByCurve;
    protected override void Awake()
    {
        base.Awake();
        firstPersonLevel = atributeByCurve.thirdPersonLevel;
        thirdPersonLevel = atributeByCurve.thirdPersonLevel;
        LoadLevelFromPlayerPrefs();
    }
    private void LoadLevelFromPlayerPrefs()
    {
        //if (PlayerPrefs.HasKey("firstPersonLevel"))
        //{
        //    firstPersonLevel = PlayerPrefs.GetInt("firstPersonLevel");
        //}

        //if (PlayerPrefs.HasKey("thirdPersonLevel"))
        //{
        //    thirdPersonLevel = PlayerPrefs.GetInt("thirdPersonLevel");
        //}

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
