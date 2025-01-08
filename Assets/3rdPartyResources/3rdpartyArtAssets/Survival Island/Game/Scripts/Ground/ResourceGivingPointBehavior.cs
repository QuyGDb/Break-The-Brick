using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ResourceGivingPointBehavior : ResourcePointBehavior
    {
        public IResourceGiver ResourceGiver { get; private set; }
        public Dictionary<IResourceTaker, int> ResourceTakers { get; } = new Dictionary<IResourceTaker, int>();

        [Space]
        [SerializeField] protected bool overrideResourceSpawnPoint;
        [SerializeField, ShowIf("overrideResourceSpawnPoint")] protected Transform resourceSpawnPoint;
        
        public Vector3 ResourceSpawnPosition => overrideResourceSpawnPoint ? resourceSpawnPoint.position : transform.position;

        public void SetResourceGiver(IResourceGiver resourceGiver)
        {
            ResourceGiver = resourceGiver;
        }

        private void Update()
        {            
            if (ResourceGiver != null &&                                        // is ResourceGiver assigned
                Time.time - ResourceGiver.LastTimeResourceGiven > cooldown &&   // is cooldown delay passed
                !ResourceGiver.IsResourceGivingBlocked &&                       // does giver can give resources (e.x. Player can't give resources when running)
                ResourceTakers.Count > 0)                                       // are there any resource takers inside the trigger
            {

                var succesfullTakers = new List<IResourceTaker>(); // Workaround for enumerator to work properly

                foreach (var taker in ResourceTakers.Keys)
                {
                    if (taker.IsResourceTakingBlocked || taker.RequiredResources == null) continue;
                    
                    foreach (var requiredResourceType in taker.RequiredResources)
                    {
                        float takingSpeedUpStage = ResourceTakers[taker] / 4;
                        var nextAmount = (int)(2 * takingSpeedUpStage);
                        if (nextAmount < 1) nextAmount = 1;

                        var availableResources = Mathf.Min(ResourceGiver.GetResourceCount(requiredResourceType), taker.RequiredMaxAmount(requiredResourceType));
                        if (availableResources <= 0) continue;
                        nextAmount = Mathf.Clamp(nextAmount, 1, availableResources);

                        var one = Resource.Create(requiredResourceType, nextAmount);

                        if (ResourceGiver.HasResource(one))
                        {
                            ResourceGiver.GiveResource(one);

                            FlyingResourceBehavior flyingResource = CurrenciesController.GetCurrency(requiredResourceType).Data.FlyingResPool.GetPooledComponent();

                            flyingResource.InitAtPosition(ResourceGiver.FlyingResourceSpawnPosition, nextAmount);

                            taker.TakeResource(flyingResource, ResourceGiver.IsPlayer);

                            succesfullTakers.Add(taker);

                            ResourceTakers[taker] = ResourceTakers[taker] + 1;

                            return;
                        }
                    }

                    if (ResourceGiver.HasResources()) taker.Rejected();
                }

                for (int i = 0; i < succesfullTakers.Count; i++)
                {
                    ResourceTakers[succesfullTakers[i]] = ResourceTakers[succesfullTakers[i]] + 1;
                }
            }
        }

        protected override void AddResourceCarrier(GameObject carrierObject)
        {
            var resourceTaker = carrierObject.GetComponent<IResourceTaker>();

            if (resourceTaker == null)
            {
                throw new System.Exception($"Game Object {carrierObject.name} does not implement IResourceTaker interface");
            }
            else if (!ResourceTakers.ContainsKey(resourceTaker))
            {
                ResourceTakers.Add(resourceTaker, 0);
            }
        }

        protected override void RemoveResourceCarrier(GameObject carrierObject)
        {
            var resourceTaker = carrierObject.GetComponent<IResourceTaker>();

            if (resourceTaker == null)
            {
                throw new System.Exception($"Game Object {carrierObject.name} does not implement IResourceTaker interface");
            }
            else if (ResourceTakers.ContainsKey(resourceTaker))
            {
                ResourceTakers.Remove(resourceTaker);
            }
        }

        #region Editor

        protected bool ShowCreateCustomGavedResourceSpawnPointButton()
        {
            return overrideResourceSpawnPoint && resourceSpawnPoint == null;
        }

        [Button(visibilityMethodName: "ShowCreateCustomGavedResourceSpawnPointButton")]
        protected void CreateCustomGavedResourceSpawnPoint()
        {
            Transform newObjectTransform = new GameObject("Resource Spawn Position").transform;
            newObjectTransform.position = transform.position + Vector3.up;
            newObjectTransform.SetParent(transform);

            resourceSpawnPoint = newObjectTransform;

            RuntimeEditorUtils.SetDirty(newObjectTransform);
            RuntimeEditorUtils.SetDirty(this);
        }

        #endregion
    }
}