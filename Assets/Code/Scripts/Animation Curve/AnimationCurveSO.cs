using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationCurveSO", menuName = "ScriptableObjects/AnimationCurveSO", order = 1)]
public class AnimationCurveSO : ScriptableObject
{
    public AnimationCurve shakeCameraCurve;
    public AnimationCurve speedCurve;
    public AnimationCurve attackCurve;

}
