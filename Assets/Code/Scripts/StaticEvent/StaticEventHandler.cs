using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticEventHandler
{
    public static Action OnBrickDestroy;

    public static void CallOnBrickDestroy()
    {
        OnBrickDestroy?.Invoke();
    }
}
