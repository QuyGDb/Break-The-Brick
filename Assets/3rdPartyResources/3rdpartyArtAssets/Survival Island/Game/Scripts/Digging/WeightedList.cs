using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public abstract class WeightedList<T> where T : class
    {
        [SerializeField] WeightedItem<T>[] items;
        public WeightedItem<T>[] Items => items;

        public int TotalWeight => items.Sum(x => x.Weight);

        public T GetRandomItem()
        {
            int totalWeight = items.Sum(x => x.Weight);
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (WeightedItem<T> item in items)
            {
                currentWeight += item.Weight;

                if (currentWeight >= randomValue)
                {
                    return item.Item;
                }
            }

            return null;
        }

        [System.Serializable]
        public class WeightedItem<I>
        {
            [SerializeField] int weight = 1;
            public int Weight => weight;

            [SerializeField] I item;
            public I Item => item;
        }
    }
}