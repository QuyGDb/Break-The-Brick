using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Mục đích để demo các level
/// </summary>
[CreateAssetMenu(fileName = "CurveSO", menuName = "ScriptableObjects/CurveSO", order = 1)]
public class AtributeByCurve : ScriptableObject
{
    public AnimationCurve shakeCameraCurve;
    public int firstPersonLevel;
    public int thirdPersonLevel;

    public AnimationCurve speedCurve;
    public int speedLevel;

}
