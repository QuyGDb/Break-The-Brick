using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerBaseAtribubesSO", menuName = "ScriptableObjects/PlayerBaseAtribubes", order = 1)]
public class PlayerBaseAtribubesSO : ScriptableObject
{
    public float baseMoneyForAtributes;
    public float multiplierForUpgradeCost;
    public float multiplierForIncome;
    public float multiplierForAttack;
    public AnimationCurve speedCurve;

}
