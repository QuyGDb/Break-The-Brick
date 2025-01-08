using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// The class that can be given resources and has the ability to display them on the world space UI
    /// </summary>
    /// <typeparam name="T">The implementation of the ResourceCanvas abstract class</typeparam>
    public abstract class ResourceTaker<T> : MonoBehaviour, IResourceTaker where T : WorldSpaceCanvas
    {
        [SerializeField] protected T resourceCanvas;
        public T ResourceCanvas => resourceCanvas;
        [SerializeField] protected ResourceTakingPointBehavior resourceTakingPoint;
        public ResourceTakingPointBehavior ResourceTakingPoint => resourceTakingPoint;

        public abstract bool IsResourceTakingBlocked { get; }

        /// <summary>
        /// The list of resources that the ResourceTaker needs at this moment.
        /// </summary>
        public List<CurrencyType> RequiredResources { get; protected set; } = new List<CurrencyType>();

        public Vector3 FlyingResourceDestination => resourceTakingPoint.ResourceDestination;

        protected ResourceListSave save;

        public abstract void TakeResource(FlyingResourceBehavior flyingResource, bool fromPlayer);
        public abstract int RequiredMaxAmount(CurrencyType currency);
        protected abstract void PopulateRequiredResources();

        protected virtual void Awake()
        {
            RequiredResources = new List<CurrencyType>();
            if (resourceTakingPoint != null) resourceTakingPoint.SetResourceTaker(this);
        }

        public void Enable()
        {
            resourceCanvas.Visible = true;

            PopulateRequiredResources();
        }

        public void Disable()
        {
            resourceCanvas.Visible = false;

            RequiredResources.Clear();
        }

        public virtual void Rejected() { }
    }
}