using UnityEngine;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    [System.Serializable]
    public class PlayerInventory
    {
        private int maxCapacity;
        public int MaxCapacity => maxCapacity;

        private int currentCapacity;
        public int CurrentCapacity => currentCapacity;

        private Currency[] currencies;

        private CapacityUpgrade characterCapacityUpgrade;

        private PlayerBehavior playerBehavior;

        private InventorySave inventorySave;
        public InventorySave InventorySave => inventorySave;

        public event SimpleCallback CapacityChanged;

        public void Initialise(PlayerBehavior playerBehavior)
        {
            this.playerBehavior = playerBehavior;

            inventorySave = SaveController.GetSaveObject<InventorySave>("inventorySave");

            // Get upgrade
            characterCapacityUpgrade = GlobalUpgradesController.GetUpgrade<CapacityUpgrade>(GlobalUpgradeType.Capacity);

            // Get all inventory currencies
            currencies = System.Array.FindAll(CurrenciesController.Currencies, x => x.Data.UseInventory);
            foreach(Currency currency in currencies)
            {
                currency.OnCurrencyChanged += OnCurrencyAmountChanged;
            }

            GlobalUpgradesEventsHandler.OnUpgraded += OnInventoryUpgraded;

            RecalculateUpgrade();
            RecalculateCapacity();
        }

        public void Unload()
        {
            foreach (Currency currency in currencies)
            {
                currency.OnCurrencyChanged -= OnCurrencyAmountChanged;
            }

            GlobalUpgradesEventsHandler.OnUpgraded -= OnInventoryUpgraded;
        }

        private void OnCurrencyAmountChanged(Currency currency, int amountDifference)
        {
            if (currency != null && currency.Data.UseInventory)
            {
                RecalculateCapacity();
            }
        }

        public void RecalculateCapacity()
        {
            currentCapacity = 0;

            for (int i = 0; i < currencies.Length; i++)
            {
                currentCapacity += currencies[i].Amount;
            }

            CapacityChanged?.Invoke();
        }

        public bool IsFull()
        {
            return currentCapacity >= maxCapacity;
        }

        public bool TryToAdd(CurrencyType currencyType, int amount, bool adsReward = false)
        {
            Currency currency = CurrenciesController.GetCurrency(currencyType);
            if (currency != null)
            {
                if (currency.Data.UseInventory)
                {
                    if (!adsReward)
                    {
                        if (currentCapacity + amount <= maxCapacity)
                        {
                            // Add items to inventory
                            CurrenciesController.Add(currencyType, amount);

                            RecalculateCapacity();

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Add items to inventory
                        CurrenciesController.Add(currencyType, amount);

                        RecalculateCapacity();

                        return true;
                    }
                }
                else
                {
                    Debug.LogError("[Inventory]: For non-inventory currencies use default CurrenciesController methods!");

                    return false;
                }
            }

            Debug.LogError(string.Format("[Inventory]: Currency type - {0} is missing!", currencyType));

            return false;
        }

        public bool TryToAdd(CurrencyType currencyType, ref int amount, bool adsReward = false)
        {
            Currency currency = CurrenciesController.GetCurrency(currencyType);
            if (currency != null)
            {
                if (currency.Data.UseInventory)
                {
                    if (!adsReward)
                    {
                        if (currentCapacity + amount > maxCapacity)
                        {
                            int availableAmount = maxCapacity - currentCapacity;
                            if (availableAmount > 0)
                            {
                                // Add items to inventory
                                CurrenciesController.Add(currencyType, availableAmount);

                                amount -= availableAmount;

                                RecalculateCapacity();

                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            // Add items to inventory
                            CurrenciesController.Add(currencyType, amount);

                            amount = 0;

                            RecalculateCapacity();

                            return true;
                        }
                    }
                    else
                    {
                        // Add items to inventory
                        CurrenciesController.Add(currencyType, amount);

                        amount = 0;

                        RecalculateCapacity();

                        return true;
                    }
                }
                else
                {
                    Debug.LogError("[Inventory]: For non-inventory currencies use default CurrenciesController methods!");

                    return false;
                }
            }

            Debug.LogError(string.Format("[Inventory]: Currency type - {0} is missing!", currencyType));

            return false;
        }

        private void OnInventoryUpgraded(GlobalUpgradeType upgradeType, AbstactGlobalUpgrade upgrade)
        {
            if (upgradeType == GlobalUpgradeType.Capacity)
            {
                RecalculateUpgrade();
                CapacityChanged?.Invoke();
            }
        }

        private void RecalculateUpgrade()
        {
            CapacityUpgrade.CapacityUpgradeStage stage = characterCapacityUpgrade.GetCurrentStage();

            maxCapacity = stage.Capacity;

#if UNITY_EDITOR
            // Only editor feature: use Actions/Infinite Inventory - to make inventory infinite for testing purposes
            if(InventoryActionMenu.IsInventoryInfinite())
            {
                maxCapacity = int.MaxValue;
            }
#endif
        }

        public void ClearDev()
        {
            for (int i = 0; i < currencies.Length; i++)
            {
                CurrenciesController.Substract(currencies[i].CurrencyType, currencies[i].Amount);
            }

            RecalculateCapacity();
        }
    }

    public class InventorySave : ISaveObject
    {
        public bool TutorialShown;

        public void Flush()
        {

        }
    }
}