using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Energy System Database", menuName = "Content/Energy System Database")]
    public class EnergySystemDatabase : ScriptableObject
    {
        [SerializeField] float maxEnergyPoints;
        public float MaxEnergyPoints => maxEnergyPoints;

        [Tooltip("Amount of energy points spent to harvest entire resource entity (spent on the first hit)")]
        [SerializeField] float energyCostForHarvesting;
        public float EnergyCostForHarvesting => energyCostForHarvesting;

        [Tooltip("Amount of energy points spent to make one hamer hit")]
        [SerializeField] float energyCostForBuilding;
        public float EnergyCostForBuilding => energyCostForBuilding;

        [Tooltip("Value multiplied to speed when have no energy")]
        [SerializeField, Range(0f, 1f)] float lowEnergySpeedMult;
        public float LowEnergySpeedMult => lowEnergySpeedMult;

        [Tooltip("Value multiplied to harvest damage when have no energy")]
        [SerializeField, Range(0f, 1f)] float lowEnergyHarvestSpeedMult;
        public float LowEnergyHarvestSpeedMult => lowEnergyHarvestSpeedMult;

        public List<FoodItemData> foodItems;
        public List<FoodItemData> FoodItems => foodItems;


        private Dictionary<CurrencyType, FoodItemData> typeToDataDict = new Dictionary<CurrencyType, FoodItemData>();

        public void Initialise()
        {
            for (int i = 0; i < foodItems.Count; i++)
            {
                typeToDataDict.Add(foodItems[i].type, foodItems[i]);
            }
        }

        public bool IsFoodItem(CurrencyType type)
        {
            return typeToDataDict.ContainsKey(type);
        }

        public FoodItemData GetItemData(CurrencyType type)
        {
            if (typeToDataDict.ContainsKey(type))
                return typeToDataDict[type];

            Debug.LogError("Type " + type + " is not registered as an food item!");
            return new FoodItemData()
            {
                type = type,
                energyPointsRestoring = 0,
            };
        }
    }

    [System.Serializable]
    public class FoodItemData
    {
        public CurrencyType type;
        public  int energyPointsRestoring;
    }
}