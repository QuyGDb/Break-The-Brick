using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationCurveSO", menuName = "ScriptableObjects/AnimationCurveSO", order = 1)]
public class AnimationCurveSO : ScriptableObject
{
    [SerializeField] AnimationCurve curve;
}
