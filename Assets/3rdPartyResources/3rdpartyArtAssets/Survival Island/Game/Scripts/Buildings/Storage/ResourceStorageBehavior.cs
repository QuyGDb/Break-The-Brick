using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class ResourceStorageBehavior<T> : ResourceTaker<T>, IResourceGiver where T : WorldSpaceCanvas
    {
        [SerializeField] ResourceGivingPointBehavior resourceGivingPoint;
        public ResourceGivingPointBehavior ResourceGivingPoint => resourceGivingPoint;

        [SerializeField] FlyingResourceAnimation customFlyingResourceAnimation;

        public Vector3 FlyingResourceSpawnPosition => resourceGivingPoint.ResourceSpawnPosition;

        public float LastTimeResourceGiven { get; protected set; }
        public bool IsResourceGivingBlocked => false;
        public override bool IsResourceTakingBlocked => false;

        public bool IsPlayer => false;

        public ResourcesList Storage { get => save.Resources; protected set => save.Resources = value; }

        public event SimpleCallback OnResourcesChanged;

        /// <summary>
        /// The resources that are displayed on the UI
        /// </summary>
        protected ResourcesList displayedStorage;

        public abstract bool IsFull();

        protected override void Awake()
        {
            base.Awake();

            if (resourceGivingPoint != null)
                resourceGivingPoint.SetResourceGiver(this);
        }

        protected void Init(string saveName)
        {
            save = SaveController.GetSaveObject<ResourceListSave>(saveName);
            save.Init();

            displayedStorage = new ResourcesList(Storage);
        }

        protected void InvokeOnResourceChanged()
        {
            OnResourcesChanged?.Invoke();
        }

        public virtual void GiveResource(Resource resource)
        {
            Storage -= resource;
            displayedStorage -= resource;

            PopulateRequiredResources();

            UpdateCanvas();

            LastTimeResourceGiven = Time.time;
            InvokeOnResourceChanged();
        }

        /// <summary>
        /// Instantly add some resources to the storage
        /// </summary>
        /// <param name="resources"></param>
        public void AddResources(ResourcesList resources)
        {
            Storage += resources;
            displayedStorage += resources;

            UpdateCanvas();

            PopulateRequiredResources();
            InvokeOnResourceChanged();
        }

        /// <summary>
        /// Instantly remove some resources from the storage
        /// </summary>
        /// <param name="resource"></param>
        public void RemoveResource(ResourcesList resource)
        {
            Storage -= resource;
            displayedStorage -= resource;

            UpdateCanvas();

            PopulateRequiredResources();
            InvokeOnResourceChanged();
        }

        /// <summary>
        /// Physically add a one resouce to the storage
        /// </summary>
        /// <param name="flyingResource"></param>
        public override void TakeResource(FlyingResourceBehavior flyingResource, bool fromPlayer)
        {
            var one = Resource.Create(flyingResource.ResourceType, flyingResource.Amount);

            Storage += one;
            PopulateRequiredResources();

            flyingResource.SetCustomAnimation(customFlyingResourceAnimation);
            var tweenCase = flyingResource.PlayAnimation(FlyingResourceDestination, () =>
            {
                flyingResource.gameObject.SetActive(false);

                displayedStorage += one;

                UpdateCanvas();
            });

            if (AudioController.AudioClips.resourcesPickUpFromStorageSound != null)
            {
                var gameData = GameController.Data;

                tweenCase.OnTimeReached(gameData.StorageSoundStartTime, () =>
                {
                    gameData.StorageSoundHandler.Play(AudioController.AudioClips.resourcesPickUpFromStorageSound, transform.position);
                });
            }

            InvokeOnResourceChanged();
        }

        protected abstract void UpdateCanvas();

        protected override void PopulateRequiredResources()
        {

        }

        public bool HasResource(Resource resource)
        {
            foreach (var storedResource in displayedStorage)
            {
                if (storedResource.currency == resource.currency)
                {
                    return storedResource.amount >= resource.amount;
                }
            }

            return false;
        }

        public bool HasResources(ResourcesList resources)
        {
            foreach (var resource in resources)
            {
                if (!HasResource(resource))
                    return false;
            }

            return true;
        }

        public bool HasResources()
        {
            return Storage.Count > 0;
        }

        public int GetResourceCount(CurrencyType currencyType)
        {
            for (int i = 0; i < Storage.Count; i++)
            {
                var resource = Storage[i];

                if (resource.currency == currencyType)
                {
                    return Storage[i].amount;
                }
            }

            return 0;
        }

        public bool IsEmpty()
        {
            return Storage.Count == 0;
        }

        public void Clear()
        {
            for (int i = 0; i < Storage.Count; i++)
            {
                var resource = Storage[i];

                CurrenciesController.Add(resource.currency, resource.amount);
            }

            Storage.Clear();
            save.Flush();
            displayedStorage.Clear();
            if(!RequiredResources.IsNullOrEmpty()) RequiredResources.Clear();
        }
    }
}
