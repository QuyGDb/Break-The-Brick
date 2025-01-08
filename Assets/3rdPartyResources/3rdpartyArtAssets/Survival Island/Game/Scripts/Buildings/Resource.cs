using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ResourceListSave : ISaveObject
    {
        public ResourcesList Resources { get; set; }

        [SerializeField] private Resource[] saveCost;

        public void Init()
        {
            if(saveCost != null)
            {
                Resources = new ResourcesList(saveCost);
            } else
            {
                Resources = new ResourcesList();
            }
        }

        public void Flush()
        {
            saveCost = Resources.ToArray();
        }
    }

    public class ResourceDictionary: Dictionary<CurrencyType, int>
    {
        public ResourceDictionary()
        {

        }

        public ResourceDictionary(List<Resource> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var resource = list[i];

                if (ContainsKey(resource.currency))
                {
                    this[resource.currency] += resource.amount;
                }
                else
                {
                    Add(resource.currency, resource.amount);
                }
            }
        }
    }

    [System.Serializable]
    public class ResourcesList : List<Resource>
    {
        public ResourcesList() : base() { }
        public ResourcesList(int capacity) : base(capacity) { }
        public ResourcesList(IEnumerable<Resource> prices) : base(prices) { }

        public static ResourcesList operator +(ResourcesList cost, Resource price)
        {
            for (int i = 0; i < cost.Count; i++)
            {
                var costPrice = cost[i];

                if (price.currency == costPrice.currency)
                {
                    costPrice.amount += price.amount;
                    cost[i] = costPrice;
                    return cost;
                }
            }

            cost.Add(price);

            return cost;
        }

        public static ResourcesList operator -(ResourcesList a, ResourcesList b)
        {
            var result = new ResourcesList();

            for (int i = 0; i < a.Count; i++)
            {
                var valA = a[i];

                bool found = false;
                for (int j = 0; j < b.Count; j++)
                {
                    var valB = b[j];

                    if (valA.currency == valB.currency)
                    {
                        found = true;

                        var amount = valA.amount - valB.amount;

                        if (amount > 0)
                        {
                            result.Add(new Resource { currency = valA.currency, amount = amount });
                        }

                        break;
                    }
                }

                if (!found) result.Add(valA);
            }

            return result;
        }

        public static ResourcesList operator -(ResourcesList a, Resource b)
        {
            var result = new ResourcesList();

            for (int i = 0; i < a.Count; i++)
            {
                var valA = a[i];

                if (valA.currency == b.currency)
                {
                    valA -= b.amount;

                    if (valA != 0) result.Add(valA);

                } else
                {
                    result.Add(valA);
                }
            }

            return result;
        }

        public static ResourcesList operator /(ResourcesList list, float value)
        {
            var result = new ResourcesList();
            for (int i = 0; i < list.Count; i++)
            {
                var dropRes = list[i];

                dropRes.amount = (int) (dropRes.amount / value);

                if (dropRes.amount >= 1)
                {
                    result.Add(dropRes);
                }
            }

            return result;
        }

        public static ResourcesList operator *(ResourcesList list, float value)
        {
            var result = new ResourcesList();
            for (int i = 0; i < list.Count; i++)
            {
                var dropRes = list[i];

                dropRes.amount = (int)(dropRes.amount * value);

                result.Add(dropRes);
            }

            return result;
        }

        public static ResourcesList operator ^(ResourcesList list1, ResourcesList list2)
        {
            var result = new ResourcesList();

            for(int i = 0; i < list1.Count; i++)
            {
                var resource = list1[i];

                if (!list2.Has(resource.currency))
                {
                    result.Add(resource);
                }
            }

            return result;
        }

        public void SetEveryResourceAmountTo(int value)
        {
            for(int i = 0; i < Count; i++)
            {
                var resource = this[i];

                resource.amount = value;

                this[i] = resource;
            }
        }

        public bool Has(CurrencyType resType)
        {
            for(int i = 0; i < Count; i++)
            {
                if (this[i] == resType) return true;
            }

            return false;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Resources List. Count:{Count}");

            for(int i = 0; i < Count; i++)
            {
                stringBuilder.AppendLine(this[i].ToString());
            }

            return stringBuilder.ToString();
        }

        public static float operator %(ResourcesList a, ResourcesList b)
        {
            int aCount = 0;
            for (int i = 0; i < a.Count; i++)
            {
                aCount += a[i].amount;
            }

            int bCount = 0;
            for (int i = 0; i < b.Count; i++)
            {
                bCount += b[i].amount;
            }

            return aCount / (float)bCount;
        }

        public static ResourcesList operator +(ResourcesList a, ResourcesList b)
        {
            var result = new ResourcesList(a);

            for(int i = 0; i < b.Count; i++)
            {
                var resourceToAdd = b[i];

                bool found = false;
                for(int j = 0; j < result.Count; j++)
                {
                    var resource = result[j];
                    if (resource.currency == resourceToAdd.currency)
                    {
                        found = true;

                        resource.amount += resourceToAdd.amount;

                        result[j] = resource;

                        break;
                    }
                }

                if (!found)
                {
                    result.Add(resourceToAdd);
                }
            }

            return result;
        }

        public static implicit operator ResourcesList(CurrencyType resourceType) => new ResourcesList { resourceType };
    }

    [System.Serializable]
    public struct Resource
    {
        public CurrencyType currency;
        public int amount;

        public Resource(CurrencyType currency, int amount)
        {
            this.currency = currency;
            this.amount = amount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Resource operator -(Resource price, int amount) => new Resource { currency = price.currency, amount = price.amount - amount };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Resource One(CurrencyType currency) => new Resource { currency = currency, amount = 1 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Resource Zero(CurrencyType currency) => new Resource { currency = currency, amount = 0 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Resource Create(CurrencyType currency, int amount) => new Resource { currency = currency, amount = amount };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Resource price, int amount) => price.amount == amount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Resource price, int amount) => price.amount != amount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Resource price, CurrencyType currency) => price.currency == currency;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Resource price, CurrencyType currency) => price.currency != currency;

        public static implicit operator ResourcesList(Resource resource) => new ResourcesList { resource };
        public static implicit operator Resource(CurrencyType resourceType) => Resource.One(resourceType);

        public override string ToString()
        {
            return $"Resource {currency}: {amount}";
        }

        public override bool Equals(object obj)
        {
            return obj is Resource resource &&
                   currency == resource.currency &&
                   amount == resource.amount;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(currency, amount);
        }
    }
}