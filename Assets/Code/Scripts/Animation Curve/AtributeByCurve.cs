using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurveSO", menuName = "ScriptableObjects/nCurveSO", order = 1)]
public class AtributeByCurve : ScriptableObject
{
    public AnimationCurve shakeCameraCurve;
    public int firstPersonLevel;
    public int thirdPersonLevel;

    public AnimationCurve speedCurve;
    public int speedLevel;

}
