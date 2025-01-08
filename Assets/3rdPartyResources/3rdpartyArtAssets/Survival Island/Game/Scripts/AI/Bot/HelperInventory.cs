using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class HelperInventory
    {
        [SerializeField] int maxCapacity;
        public int MaxCapacity => maxCapacity;

        [SerializeField] CapacityOverride[] capacityOverrides;

        private int currentCapacity;
        public int CurrentCapacity => currentCapacity;

        private Slot[] currencySlots;
        private Dictionary<int, int> currencySlotsLink;

        private HelperBehavior helperBehavior;

        public event SimpleCallback OnCapacityChanged;

        public int FreeSlotsCount => maxCapacity - currentCapacity;
        public bool IsFull => currentCapacity >= maxCapacity;

        public void Initialise(HelperBehavior helperBehavior)
        {
            this.helperBehavior = helperBehavior;

            Currency[] currencies = CurrenciesController.Currencies;
            currencySlotsLink = new Dictionary<int, int>();
            currencySlots = new Slot[currencies.Length];
            for (int i = 0; i < currencySlots.Length; i++)
            {
                int capacity = maxCapacity;
                for(int c = 0; c < capacityOverrides.Length; c++)
                {
                    if (capacityOverrides[c].CurrencyType == currencies[i].CurrencyType)
                    {
                        capacity = capacityOverrides[c].MaxCapacity;

                        break;
                    }
                }

                currencySlots[i] = new Slot(this, currencies[i], capacity);

                currencySlotsLink.Add((int)currencies[i].CurrencyType, i);
            }

            RecalculateCapacity();

            OnCapacityChanged?.Invoke();
        }

        public void Unload()
        {

        }

        public void RecalculateCapacity(bool redraw = true)
        {
            currentCapacity = 0;

            for (int i = 0; i < currencySlots.Length; i++)
            {
                currentCapacity += currencySlots[i].Amount;
            }
        }

        public bool TryToAdd(CurrencyType currencyType, int amount, bool ignoreMaxCapacity = false)
        {
            Slot slot = currencySlots[currencySlotsLink[(int)currencyType]];
            if (slot != null)
            {
                if(!ignoreMaxCapacity)
                {
                    if(currentCapacity + amount <= maxCapacity)
                    {
                        slot.Add(amount, true);

                        OnCapacityChanged?.Invoke();

                        return true;
                    }

                    return false;
                }
                else
                {
                    slot.Add(amount, false);

                    OnCapacityChanged?.Invoke();

                    return true;
                }
            }

            Debug.LogError(string.Format("[Inventory]: Currency type - {0} is missing!", currencyType));

            return false;
        }

        public bool TryToAdd(CurrencyType currencyType, ref int amount, bool ignoreMaxCapacity = false)
        {
            Slot slot = currencySlots[currencySlotsLink[(int)currencyType]];
            if (slot != null)
            {
                if (!ignoreMaxCapacity)
                {
                    if (currentCapacity + amount > maxCapacity)
                    {
                        int availableAmount = maxCapacity - currentCapacity;
                        if (availableAmount > 0)
                        {
                            slot.Add(availableAmount, true);

                            amount -= availableAmount;

                            OnCapacityChanged?.Invoke();

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        slot.Add(amount, true);

                        amount = 0;

                        OnCapacityChanged?.Invoke();

                        return true;
                    }
                }
                else
                {
                    slot.Add(amount, false);

                    amount = 0;

                    OnCapacityChanged?.Invoke();

                    return true;
                }
            }

            Debug.LogError(string.Format("[Inventory]: Currency type - {0} is missing!", currencyType));

            return false;
        }

        public bool HasResource(CurrencyType currencyType)
        {
            Slot slot = currencySlots[currencySlotsLink[(int)currencyType]];
            if(slot != null)
            {
                return slot.Amount > 0;
            }

            return false;
        }

        public bool HasResource(CurrencyType currencyType, int amount)
        {
            Slot slot = currencySlots[currencySlotsLink[(int)currencyType]];
            if (slot != null)
            {
                return slot.Amount >= amount;
            }

            return false;
        }

        public bool HasResource(List<CurrencyType> currencyTypes)
        {
            foreach(CurrencyType currencyType in currencyTypes)
            {
                Slot slot = currencySlots[currencySlotsLink[(int)currencyType]];

                if (slot != null)
                {
                    if (slot.Amount > 0)
                        return true;
                }
            }

            return false;
        }

        public bool HasResource(List<Resource> resources)
        {
            foreach (var resource in resources)
            {
                Slot slot = currencySlots[currencySlotsLink[(int)resource.currency]];

                if (slot != null)
                {
                    if (slot.Amount > 0)
                        return true;
                }
            }

            return false;
        }

        public int GetResourceCount(CurrencyType currencyType)
        {
            Slot slot = currencySlots[currencySlotsLink[(int)currencyType]];
            if (slot != null)
            {
                return slot.Amount;
            }

            return 0;
        }

        public Slot GetResource(CurrencyType currencyType)
        {
            Slot slot = currencySlots[currencySlotsLink[(int)currencyType]];
            if (slot != null)
            {
                return slot;
            }

            return null;
        }

        public IEnumerable<Slot> GetResources()
        {
            foreach(Slot slot in currencySlots)
            {
                if (slot.Amount > 0)
                    yield return slot;
            }
        }

        public string Log()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Helper - ");
            sb.Append(helperBehavior.name);
            sb.AppendLine();
            sb.Append("IsFull: ");
            sb.Append(IsFull);
            sb.AppendLine();
            sb.AppendLine("=================");
            for(int i = 0; i < currencySlots.Length; i++)
            {
                sb.AppendLine(currencySlots[i].ToString());
            }

            return sb.ToString();
        }

        public void ClearDev()
        {
            for (int i = 0; i < currencySlots.Length; i++)
            {
                currencySlots[i].Set(0);
            }

            RecalculateCapacity(true);
        }

        public class Slot
        {
            private Currency currency;
            public Currency Currency => currency;

            private int maxCapacity;
            public int MaxCapacity => maxCapacity;

            private int amount;
            public int Amount => amount;

            private HelperInventory helperInventory;

            public Slot(HelperInventory helperInventory, Currency currency, int maxCapacity)
            {
                this.helperInventory = helperInventory;
                this.currency = currency;
                this.maxCapacity = maxCapacity;
            }

            public void SetMaxCapacity(int newMaxCapacity)
            {
                maxCapacity = newMaxCapacity;

                amount = Mathf.Clamp(amount, 0, maxCapacity);
            }

            public void Set(int newAmount, bool clampToMax = true)
            {
                amount = newAmount;

                if (clampToMax)
                    amount = Mathf.Clamp(amount, 0, maxCapacity);

                helperInventory.RecalculateCapacity();
            }

            public void Add(int addAmount, bool clampToMax = true)
            {
                amount += addAmount;

                if (clampToMax)
                    amount = Mathf.Clamp(amount, 0, maxCapacity);

                helperInventory.RecalculateCapacity();
            }

            public void Substract(int substractAmount)
            {
                amount -= substractAmount;

                if (amount < 0)
                    amount = 0;

                helperInventory.RecalculateCapacity();
            }

            public void Clear()
            {
                amount = 0;

                helperInventory.RecalculateCapacity();
            }

            public bool HasEnough(int checkAmount)
            {
                return amount >= checkAmount;
            }

            public override string ToString()
            {
                return string.Format("{0} - CUR:{1} - ({2}/{3})", helperInventory.helperBehavior.gameObject.name, currency.CurrencyType, amount, maxCapacity); 
            }
        }

        [System.Serializable]
        private class CapacityOverride
        {
            [SerializeField] CurrencyType currencyType;
            public CurrencyType CurrencyType => currencyType;

            [SerializeField] int maxCapacity = 1;
            public int MaxCapacity => maxCapacity;
        }
    }
}