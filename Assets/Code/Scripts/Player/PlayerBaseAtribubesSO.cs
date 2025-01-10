using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerBaseAtribubesSO", menuName = "ScriptableObjects/PlayerBaseAtribubes", order = 1)]
public class PlayerBaseAtribubesSO : ScriptableObject
{
    public float baseAttack;
    public float baseSpeed;
    public float baseIncome;
    public float atkGrowthRate;
    public float speedGrowthRate;
    public float moneyGrowthRate;
    public float incomeGrowthRate;
    public float baseMoneyForAtributes;

}
