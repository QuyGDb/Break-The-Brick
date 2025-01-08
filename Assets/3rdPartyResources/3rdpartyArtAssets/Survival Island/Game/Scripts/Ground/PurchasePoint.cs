using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// This class' purpose is to let the player buy objects in the game world with resources
    /// </summary>
    /// 
    [CustomOverlayElement("Purchaser UI", "OnToggleValueChanged")]
    public class PurchasePoint : ResourceTaker<ResourcesCanvas>, IPurchaser
    {
        [SerializeField] FlyingResourceAnimation customFlyingResourceAnimation;

        public IUnlockableComplex UnlockableComplex { get; private set; }

        /// <summary>
        /// Triggered when all the needed resources have been collected from the player and flying resources have reached this point
        /// </summary>
        public event SimpleCallback OnPurhcased;

        /// <summary>
        /// Triggered when a flying resource reaches this point
        /// </summary>
        public event SimpleCallback OnResourcePlaced;

        /// <summary>
        /// The amount of resources that are needed to complete the purchase
        /// </summary>
        private ResourcesList costLeft;
        public ResourcesList CostLeft => costLeft;

        private PurchasePointSave pointSave;
        private PurchasePointSave Save
        {
            get => pointSave;
            set
            {
                pointSave = value;
                save = value;
            }
        }

        /// <summary>
        /// The cost that are displayed on the UI. Decreases when the flying resources reach this point
        /// </summary>
        private ResourcesList displayedCost;

        public override bool IsResourceTakingBlocked => false;

        public bool Init(IUnlockableComplex unlockableComplex)
        {
            UnlockableComplex = unlockableComplex;

            if (Save == null)
            {
                Save = SaveController.GetSaveObject<PurchasePointSave>(unlockableComplex.ID + "_purchase_point");
                Save.Init();
            }

            costLeft = UnlockableComplex.Cost - Save.Resources;
            if (costLeft.Count == 0 || Save.IsBought)
            {
                // The point has already done it's job, no need to keep it alive
                Destroy();
                return false;
            }

            displayedCost = new ResourcesList(costLeft);

            resourceCanvas.gameObject.SetActive(true);
            resourceCanvas.SetData(displayedCost);

            PopulateRequiredResources();
            gameObject.SetActive(true);

            return true;
        }

        public bool LookUpPurchased(IUnlockableComplex unlockableComplex)
        {
            if (Save == null)
            {
                Save = SaveController.GetSaveObject<PurchasePointSave>(unlockableComplex.ID + "_purchase_point");
                Save.Init();
            }

            return Save.IsBought || (UnlockableComplex.Cost - Save.Resources).Count == 0;
        }

        public override void TakeResource(FlyingResourceBehavior flyingResource, bool fromPlayer)
        {
            var one = Resource.Create(flyingResource.ResourceType, flyingResource.Amount);

            save.Resources += one;
            costLeft = UnlockableComplex.Cost - save.Resources;
            PopulateRequiredResources();

            flyingResource.SetCustomAnimation(customFlyingResourceAnimation);
            var tweenCase = flyingResource.PlayAnimation(FlyingResourceDestination, () =>
            {
                // The flying resource has reached this point
                flyingResource.gameObject.SetActive(false);

                displayedCost -= one;

                if (displayedCost.Count == 0)
                {
                    Save.IsBought = true;

                    UnlockableComplex.Purchase();
                    OnPurhcased?.Invoke();
                }
                else
                {
                    resourceCanvas.SetData(displayedCost);
                }

                OnResourcePlaced?.Invoke();
            });

            if (AudioController.AudioClips.resourcesPickUpFromStorageSound != null && fromPlayer)
            {
                var gameData = GameController.Data;

                tweenCase.OnTimeReached(gameData.StorageSoundStartTime, () =>
                {
                    gameData.StorageSoundHandler.Play(AudioController.AudioClips.resourcesPickUpFromStorageSound, transform.position);
                });
            }
        }

        public override int RequiredMaxAmount(CurrencyType currency)
        {
            for (int i = 0; i < costLeft.Count; i++)
            {
                var price = costLeft[i];

                if (price.currency == currency)
                {
                    return price.amount;
                }
            }

            return 0;
        }

        protected override void PopulateRequiredResources()
        {
            RequiredResources.Clear();

            for (int i = 0; i < costLeft.Count; i++)
            {
                var price = costLeft[i];

                if (!RequiredResources.Contains(price.currency))
                {
                    RequiredResources.Add(price.currency);
                }
            }
        }

        public void Destroy()
        {
            if (resourceCanvas != null)
                Destroy(resourceCanvas.gameObject);

            Destroy(gameObject);
        }

        #region Development

        public void OnToggleValueChanged(bool enabled)
        {
            if (resourceCanvas == null)
                return;

            resourceCanvas.gameObject.SetActive(enabled);
        }

        public void UpdateCostInEditor(List<Resource> cost)
        {
            if (resourceCanvas == null) return;

            resourceCanvas.SetData(cost);
        }
        #endregion


    }
}