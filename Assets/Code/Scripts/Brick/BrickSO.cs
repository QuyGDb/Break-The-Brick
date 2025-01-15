using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BrickSO", menuName = "ScriptableObjects/BrickSO", order = 1)]
public class BrickSO : ScriptableObject
{
    public float topPosition;
    public float bottomPosition;
    public float bottomOffset;
}
