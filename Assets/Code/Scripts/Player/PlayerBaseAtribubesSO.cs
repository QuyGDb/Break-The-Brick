using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerBaseAtribubesSO", menuName = "ScriptableObjects/PlayerBaseAtribubes", order = 1)]
public class PlayerBaseAtribubesSO : ScriptableObject
{
    public float baseAttack;
    public float baseSpeed;
    public float baseIncome;
    public float baseMoneyForAtributes;
    public float startRatio;
    public float progressionRateForUpgradeCost;
    public float progressionRateForIncome;
    public float progressionRateForSpeed;
    public float progressionRateForAttack;
    public int levelThresholdForUpgradeCost;
    public int levelThresholdForIncome;
    public int levelThresholdForSpeed;
    public int levelThresholdForAttack;


}
