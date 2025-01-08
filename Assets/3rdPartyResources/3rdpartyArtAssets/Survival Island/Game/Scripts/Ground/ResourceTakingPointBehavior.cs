using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// This class is taking resources from the player when the player is inside the trigger, 
    /// and gives the said resources to the assigned ResourceTaker
    /// </summary>
    public class ResourceTakingPointBehavior : ResourcePointBehavior
    {
        public IResourceTaker ResourceTaker { get; private set; }
        public Dictionary<IResourceGiver, int> ResourceGivers { get; } = new Dictionary<IResourceGiver, int>();

        [Space]
        [SerializeField] protected bool overrideResourceDestination;
        [SerializeField, ShowIf("overrideResourceDestination")] protected Transform resourceDestinationPoint;

        public Vector3 ResourceDestination => overrideResourceDestination ? resourceDestinationPoint.position : (transform.position + Vector3.up);

        public void SetResourceTaker(IResourceTaker resourceTaker)
        {
            ResourceTaker = resourceTaker;
        }

        private void Update()
        {
            if (ResourceTaker != null &&                                // is ResourceTaker assigned
                !ResourceTaker.RequiredResources.IsNullOrEmpty() &&     // does ResourceTaker need resources
                ResourceGivers.Count > 0)                               // are there any resource givers inside the trigger
            {
                var succesfullGivers = new List<IResourceGiver>(); // Workaround for enumerator to work properly

                foreach (var giver in ResourceGivers.Keys)
                {
                    if (Time.time - giver.LastTimeResourceGiven < cooldown)
                        continue;   // is cooldown delay passed
                    if (giver.IsResourceGivingBlocked)
                        continue;                        // does giver can give resources (e.x. Player can't give resources when running)

                    var availableRequiredResources = new List<ShuffledResData>();

                    if (ResourceTaker.RequiredResources == null) break;
                    foreach (var requiredResourceType in ResourceTaker.RequiredResources)
                    {
                        float takingSpeedUpStage = ResourceGivers[giver] / 4;
                        var nextAmount = (int)(2 * takingSpeedUpStage);
                        if (nextAmount < 1)
                            nextAmount = 1;

                        var availableResources = Mathf.Min(giver.GetResourceCount(requiredResourceType), ResourceTaker.RequiredMaxAmount(requiredResourceType));
                        if (availableResources > 0) availableRequiredResources.Add(new ShuffledResData { type = requiredResourceType, availableResources = availableResources, amount = nextAmount });
                    }

                    availableRequiredResources.Shuffle();

                    foreach (var requiredResource in availableRequiredResources)
                    {
                        var nextAmount = Mathf.Clamp(requiredResource.amount, 1, requiredResource.availableResources);

                        var one = Resource.Create(requiredResource.type, nextAmount);

                        if (giver.HasResource(one))
                        {
                            giver.GiveResource(one);

                            FlyingResourceBehavior flyingResource = CurrenciesController.GetCurrency(requiredResource.type).Data.FlyingResPool.GetPooledComponent();

                            flyingResource.InitAtPosition(giver.FlyingResourceSpawnPosition, nextAmount);

                            ResourceTaker.TakeResource(flyingResource, giver.IsPlayer);

                            succesfullGivers.Add(giver);

                            break;
                        }
                    }
                }

                for (int i = 0; i < succesfullGivers.Count; i++)
                {
                    ResourceGivers[succesfullGivers[i]] = ResourceGivers[succesfullGivers[i]] + 1;
                }
            }
        }

        protected override void AddResourceCarrier(GameObject carrierObject)
        {
            var resourceGiver = carrierObject.GetComponent<IResourceGiver>();

            if (resourceGiver == null)
            {
                throw new System.Exception($"Game Object {carrierObject.name} does not implement IResourceGiver interface");
            }
            else if (!ResourceGivers.ContainsKey(resourceGiver))
            {
                ResourceGivers.Add(resourceGiver, 0);
            }
        }

        protected override void RemoveResourceCarrier(GameObject carrierObject)
        {
            var resourceGiver = carrierObject.GetComponent<IResourceGiver>();

            if (resourceGiver == null)
            {
                throw new System.Exception($"Game Object {carrierObject.name} does not implement IResourceGiver interface");
            }
            else if (ResourceGivers.ContainsKey(resourceGiver))
            {
                ResourceGivers.Remove(resourceGiver);
            }
        }

        private struct ShuffledResData
        {
            public CurrencyType type;
            public int availableResources;
            public int amount;
        }

        #region Editor

        protected bool ShowCreateResourceDestinationPointButton()
        {
            return overrideResourceDestination && resourceDestinationPoint == null;
        }

        [Button(visibilityMethodName: "ShowCreateResourceDestinationPointButton")]
        protected void CreateCustomResourceDestinationPoint()
        {
            Transform newObjectTransform = new GameObject("Resource Destination").transform;
            newObjectTransform.position = transform.position + Vector3.up;
            newObjectTransform.SetParent(transform);

            resourceDestinationPoint = newObjectTransform;

            RuntimeEditorUtils.SetDirty(newObjectTransform);
            RuntimeEditorUtils.SetDirty(this);
        }

        #endregion
    }
}