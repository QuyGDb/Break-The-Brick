using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class Recipe
    {
        [SerializeField] List<RecipeComponent> components;
        [SerializeField] CurrencyType resultResourceType;

        public CurrencyType ResultResourceType => resultResourceType;

        public int ComponentsAmount => components.Count;

        public RecipeComponent GetComponent(int i)
        {
            return components[i];
        }

        public static implicit operator ResourcesList(Recipe recipe)
        {
            var list = new ResourcesList();

            for (int i = 0; i < recipe.components.Count; i++)
            {
                list.Add(recipe.components[i]);
            }

            return list;
        }
    }

    [System.Serializable]
    public struct RecipeComponent
    {
        [SerializeField] CurrencyType resourceType;
        public CurrencyType ResourceType => resourceType;

        [SerializeField] int amount;
        public int Amount => amount;

        public static implicit operator Resource(RecipeComponent component) => new Resource(component.resourceType, component.amount);
    }
}
