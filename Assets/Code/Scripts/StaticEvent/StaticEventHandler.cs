using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticEventHandler
{
    public static Action<float> OnBrickDestroy;

    public static void CallOnBrickDestroy(float percentage)
    {
        OnBrickDestroy?.Invoke(percentage);
    }

    public static Action OnBrickDie;

    public static void CallOnBrickDie()
    {
        OnBrickDie?.Invoke();
    }
    public static Action<int, int> OnBrickCount;

    public static void CallOnBrickCount(int count, int maxBrick)
    {
        OnBrickCount?.Invoke(count, maxBrick);
    }
    public static Action<int, int> OnChopCount;
    public static void CallOnChopCount(int count, int maxChop)
    {
        OnChopCount?.Invoke(count, maxChop);
    }

    public static Action<bool> OnRotatePlatform;

    public static void CallOnRotatePlatform(bool isRotate)
    {
        OnRotatePlatform?.Invoke(isRotate);
    }

    public static Action<PlayerManager> OnPlayerManager;

    public static void CallOnPlayerManager(PlayerManager playerManager)
    {
        OnPlayerManager?.Invoke(playerManager);
    }
}
