using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CurrencyData
    {
        [SerializeField] bool displayAlways = false;
        public bool DisplayAlways => displayAlways;

        [SerializeField] GameObject flyingResPrefab;
        public GameObject FlyingResPrefab => flyingResPrefab;

        [SerializeField] GameObject dropResPrefab;
        public GameObject DropResPrefab => dropResPrefab;

        [SerializeField] AudioClip pickUpSound;
        public AudioClip PickUpSound => pickUpSound;

        [SerializeField] bool useInventory;
        public bool UseInventory => useInventory;

        [SerializeField] int moneyConversionRate;
        public int MoneyConversionRate => moneyConversionRate;

        private PoolGeneric<FlyingResourceBehavior> flyingResPool;
        public PoolGeneric<FlyingResourceBehavior> FlyingResPool => flyingResPool;

        private Pool dropResPool;
        public Pool DropResPool => dropResPool;

        public void Initialise(Currency currency)
        {
            if (flyingResPrefab) flyingResPool = new PoolGeneric<FlyingResourceBehavior>(new PoolSettings(currency.CurrencyType.ToString(), flyingResPrefab, 10, true));
            if (dropResPrefab) dropResPool = new Pool(new PoolSettings(dropResPrefab.name, dropResPrefab, 0, true));
        }
    }
}