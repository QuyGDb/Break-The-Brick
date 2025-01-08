using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    public class EnergyController : MonoBehaviour
    {
        [SerializeField, OnOff] bool isEnergyEnabled;

        [SerializeField, ShowIf("isEnergyEnabled")] EnergySystemDatabase energySystemDatabase;
        public static EnergySystemDatabase Data => instance.energySystemDatabase;

        public static float LowEnergySpeedCoef => IsEnergySystemEnabled ? EnergyPoints == 0 ? Data.LowEnergySpeedMult : 1f : 1f;
        public static float LowEnergyHittingSpeedCoef => IsEnergySystemEnabled ? EnergyPoints == 0 ? Data.LowEnergyHarvestSpeedMult : 1f : 1f;

        public static float EnergyPoints
        {
            get => save.energyPoints;
            private set
            {
                save.energyPoints = Mathf.Clamp(value, 0, Data.MaxEnergyPoints);
                SaveController.MarkAsSaveIsRequired();

                OnEnergyChanged?.Invoke();
            }
        }

        private static readonly int FLOATING_TEXT_HASH = "Floating".GetHashCode();
        private static float notAccountedEnergyPoints;

        public static event SimpleCallback OnEnergyChanged;

        private static EnergySave save;
        private static EnergyController instance;

        public static bool IsEnergySystemEnabled => instance.isEnergyEnabled;

        private static List<CurrencyType> foodResourceTypes = new List<CurrencyType>();
        public static bool IsFoorResource(CurrencyType resource) => foodResourceTypes.Contains(resource);

        private static FloatingTextBehaviour foodConsumedText;

        private static int lastDisplayedFoodPoints;

        public void Initialise()
        {
            instance = this;
            save = SaveController.GetSaveObject<EnergySave>("Energy");

            // works only in editor
            if (EnergyActionMenu.IsEnergyDisabled())
            {
                EnergyPoints = Data.MaxEnergyPoints;
            }

            Data.Initialise();

            if (!isEnergyEnabled)
                return;

            for (int i = 0; i < Data.FoodItems.Count; i++)
            {
                CurrenciesController.GetCurrency(Data.FoodItems[i].type).OnCurrencyChanged += OnFoodCurrencyChanged;

                foodResourceTypes.Add(Data.FoodItems[i].type);
            }

            ResourceSourceBehavior.OnFirstTimeHit += OnFirstResourceHit;
            PlayerBehavior.OnResourceWillBeReceived += OnResorceWillBeReceivedReceived;

            Tween.DelayedCall(0.1f, DisableFoodItemsCurrencyUI);
        }

        private void DisableFoodItemsCurrencyUI()
        {
            CurrenciesUIController currenciesUIController = UIController.GetPage<UIGame>().CurrenciesUIController;
            for (int i = 0; i < Data.FoodItems.Count; i++)
            {
                currenciesUIController.DisableCurrency(Data.FoodItems[i].type);
            }
        }

        private void OnResorceWillBeReceivedReceived(FlyingResourceBehavior resource)
        {
            if (Data.IsFoodItem(resource.ResourceType))
            {
                FoodItemData itemData = Data.GetItemData(resource.ResourceType);

                notAccountedEnergyPoints += itemData.energyPointsRestoring;

                // restricting players ability to consume more food items
                if (EnergyPoints + notAccountedEnergyPoints >= Data.MaxEnergyPoints)
                {
                    UpdatePlayerFoodReceiveAbility();
                }
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < Data.FoodItems.Count; i++)
            {
                CurrenciesController.GetCurrency(Data.FoodItems[i].type).OnCurrencyChanged -= OnFoodCurrencyChanged;
            }

            ResourceSourceBehavior.OnFirstTimeHit -= OnFirstResourceHit;
            PlayerBehavior.OnResourceWillBeReceived -= OnResorceWillBeReceivedReceived;
        }

        public static void Unload()
        {
            if (instance == null)
                return;

            if (!IsEnergySystemEnabled)
                return;

            ResourceSourceBehavior.OnFirstTimeHit -= instance.OnFirstResourceHit;
            PlayerBehavior.OnResourceWillBeReceived -= instance.OnResorceWillBeReceivedReceived;
        }

        private void OnFoodCurrencyChanged(Currency currency, int difference)
        {
            if (difference > 0)
            {
                int newPoints = Data.GetItemData(currency.CurrencyType).energyPointsRestoring;

                notAccountedEnergyPoints -= newPoints;

                RestoreEnergyPoints(newPoints);

                if (foodConsumedText == null)
                {
                    lastDisplayedFoodPoints = newPoints;
                    foodConsumedText = FloatingTextController.SpawnFloatingText(FLOATING_TEXT_HASH, $"+{newPoints} ENERGY <sprite name={currency.CurrencyType}>", PlayerBehavior.InstanceTransform.position.AddToY(2f), Quaternion.identity, Color.white).GetComponent<FloatingTextBehaviour>();
                    foodConsumedText.OnAnimationCompleted += OnFoodConsumedTextAnimationCompleted;
                }
                else
                {
                    lastDisplayedFoodPoints += newPoints;
                    foodConsumedText.SetText($"+{lastDisplayedFoodPoints} ENERGY <sprite name={currency.CurrencyType}>");
                }

                CurrenciesController.Substract(currency.CurrencyType, difference);

#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

                AudioController.PlaySound(AudioController.AudioClips.boost);
            }
        }

        private void OnFoodConsumedTextAnimationCompleted()
        {
            if (foodConsumedText != null)
            {
                foodConsumedText.OnAnimationCompleted -= OnFoodConsumedTextAnimationCompleted;
                foodConsumedText = null;
            }
        }

        private static void UpdatePlayerFoodReceiveAbility()
        {
            if (EnergyPoints + notAccountedEnergyPoints >= Data.MaxEnergyPoints)
            {
                PlayerBehavior.GetBehavior().RemoveAcceptableResoruces(foodResourceTypes);
            }
            else
            {
                PlayerBehavior.GetBehavior().AddAcceptableResoruces(foodResourceTypes);
            }
        }

        private void OnFirstResourceHit()
        {
            RemoveEnergyPoints(Data.EnergyCostForHarvesting);
        }

        public static void RestoreEnergyPoints(float poinsAmount)
        {
            EnergyPoints += poinsAmount;
        }

        public static void RemoveEnergyPoints(float pointsAmount)
        {
            // works only in editor
            if (EnergyActionMenu.IsEnergyDisabled())
                return;

            bool wasFullEnergyPointsBefore = EnergyPoints == Data.MaxEnergyPoints;

            EnergyPoints -= pointsAmount;

            // restoring players ability to consume energy items
            if (wasFullEnergyPointsBefore)
            {
                UpdatePlayerFoodReceiveAbility();
            }
        }

        public static void OnConstructionHit()
        {
            RemoveEnergyPoints(Data.EnergyCostForBuilding);
        }


        [System.Serializable]
        private class EnergySave : ISaveObject
        {
            public float energyPoints;

            public void Flush()
            {
            }
        }
    }
}