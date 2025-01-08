using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class HorizontalStorageCanvas : WorldSpaceCanvas
    {
        [SerializeField] GameObject resourceIndicatorPrefab;
        [SerializeField] Transform indicatorsContainer;

        [SerializeField] GameObject emptyStorageIndicator;
        [SerializeField] TMP_Text emptyStorageText;

        private static PoolGeneric<SimpleResourceIndicator> indicatorsPool;
        
        private List<SimpleResourceIndicator> indicators = new List<SimpleResourceIndicator>();

        protected override void Awake()
        {
            base.Awake();

            if (indicatorsPool == null)
            {
                indicatorsPool = new PoolGeneric<SimpleResourceIndicator>(new PoolSettings(resourceIndicatorPrefab, 10, true));
            }
        }

        public void SetData(ResourcesList data, int capacity)
        {
            Clear();

            for(int i = 0; i < data.Count; i++)
            {
                var resource = data[i];

                var indicator = indicatorsPool.GetPooledComponent();

                indicator.Assign(indicatorsContainer);
                indicator.SetData(resource);

                indicators.Add(indicator);
            }

            if (data.IsNullOrEmpty())
            {
                emptyStorageIndicator.SetActive(true);
                emptyStorageText.text = $"0/{capacity}";
            } else
            {
                emptyStorageIndicator.SetActive(false);
            }
        }

        public void Clear()
        {
            foreach (var indicator in indicators)
            {
                indicator.Clear();
            }

            indicators.Clear();
        }
    }
}
