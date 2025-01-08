using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [HideScriptField]
    public abstract class AbstractComplexBehavior<T, K> : MonoBehaviour, IUnlockableComplex where T : IUnlockable where K : IPurchaser
    {
        [SerializeField, UniqueID, Order(-1)] string id;
        public string ID => id;

        public Vector3 Position => transform.position;

        [BoxFoldout("Main", "Main")]
        [SerializeField] protected T unlockable;
        [BoxFoldout("Main", "Main")]
        [SerializeField] protected K purchaser;
        [BoxFoldout("Main", "Main")]
        [SerializeField] protected ConstructionPointBehavior constructionPoint;

        [ShowIf("EditorHavePurhcasePoint")]
        [BoxFoldout("Construction & Purchase", "Construction & Purchase")]
        [SerializeField] protected bool isOpenFromStart;

        [ShowIf("EditorCanBePurchased")]
        [BoxFoldout("Construction & Purchase", "Construction & Purchase")]
        [SerializeField] protected List<Resource> cost;
        public ResourcesList Cost { get; private set; }

        [ShowIf("EditorCanBeConstructed")]
        [BoxFoldout("Construction & Purchase", "Construction & Purchase")]
        [SerializeField] protected int constructionHitsRequired;
        public int ConstructionHitsRequired => constructionHitsRequired;

        public int HitsMade => constructionPoint != null ? constructionPoint.HitsMade : 0;

        public ResourcesList CostLeft => purchaser.CostLeft;

        public bool CanBePurchased { get; private set; }
        public bool CanBeConstructed { get; private set; }
        public bool IsOpen { get; private set; }

        protected bool isInitialised;
        private SimpleCallback initialisedCallback;

        public virtual void Awake()
        {
            if (cost != null)
                Cost = new ResourcesList(cost);

            if (purchaser == null)
                isOpenFromStart = true;


            if (unlockable == null)
            {
                Debug.LogError("Complex behavior with name " + name + " is missing ref to its unlockable");
                return;
            }

            unlockable.SetID(id);
        }

        public void InvokeOrSubscribe(SimpleCallback callback)
        {
            if(isInitialised)
            {
                callback?.Invoke();
            }
            else
            {
                initialisedCallback += callback;
            }
        }

        protected void InvokeInitialiseCallback()
        {
            isInitialised = true;

            initialisedCallback?.Invoke();
            initialisedCallback = null;
        }

        public bool LookUpIsOpen()
        {
            if (isInitialised) return IsOpen;

            if ((purchaser == null && constructionPoint == null) || isOpenFromStart) return true;

            bool purchaserLocked = purchaser != null && !purchaser.LookUpPurchased(this);
            bool constructionLocked = constructionPoint != null && constructionPoint.LookUpConstructed(this);
            if (purchaserLocked || constructionLocked) return false;

            return true;
        }

        public virtual void Init()
        {
            if (isOpenFromStart)
            {
                if (purchaser != null)
                    purchaser.Destroy();

                if (constructionPoint != null)
                    constructionPoint.Destroy();

                InitOpen();

                InvokeInitialiseCallback();

                return;
            }

            if (purchaser != null)
            {
                PurchaserInit();
            }
            else if (constructionPoint != null)
            {
                ConstructionInit();
            }
            else
            {
                InitOpen();
            }

            InvokeInitialiseCallback();
        }

        private void PurchaserInit()
        {
            if (!purchaser.Init(this))
            {
                if (constructionPoint != null)
                {
                    ConstructionInit();
                }
                else
                {
                    InitOpen();
                }
            }
            else
            {
                InitNotPurchased();
            }

            return;
        }

        private void ConstructionInit()
        {
            if (!constructionPoint.Init(this))
            {
                InitOpen();
            }
            else
            {
                InitNotConstructed();
            }
        }

        private void InitOpen()
        {
            CanBePurchased = false;
            CanBeConstructed = false;

            unlockable.SpawnUnlocked();

            IsOpen = true;
        }

        private void InitNotConstructed()
        {
            CanBePurchased = false;
            CanBeConstructed = true;

            constructionPoint.Enable();

            unlockable.SpanwNotUnlocked();
        }

        private void InitNotPurchased()
        {
            CanBePurchased = true;
            CanBeConstructed = false;

            if (constructionPoint != null)
                constructionPoint.Disable();

            unlockable.SpanwNotUnlocked();
        }

        #region Purchase

        public virtual void Purchase()
        {
            purchaser.Destroy();
            CanBePurchased = false;

            if (constructionPoint != null)
            {
                CanBeConstructed = true;
                constructionPoint.Enable();
                constructionPoint.Init(this);
            }
            else
            {
                CanBePurchased = false;
                CanBeConstructed = false;
                IsOpen = true;
                unlockable.FullyUnlock();
            }
        }

        public void EnablePurchase()
        {
            CanBePurchased = purchaser != null;
            if (CanBePurchased)
                purchaser.Enable();
        }

        public void DisablePurchase()
        {
            CanBePurchased = false;
            if (purchaser != null)
                purchaser.Disable();
        }

        #endregion

        #region Construction

        public virtual void Construct()
        {
            if (!CanBeConstructed) return;

            constructionPoint.Destroy();

            CanBePurchased = false;
            CanBeConstructed = false;
            IsOpen = true;

            unlockable.FullyUnlock();
        }

        public void EnableConstructing()
        {
            CanBeConstructed = unlockable != null;
            if (CanBeConstructed)
                constructionPoint.Enable();
        }

        public void DisableCounstructing()
        {
            CanBeConstructed = false;
            if (constructionPoint != null)
                constructionPoint.Disable();
        }

        public Sprite GetConstrutionIcon()
        {
            if (constructionPoint != null)
            {
                return constructionPoint.GetConstructionIcon();
            }

            return null;
        }

        #endregion

        #region Events

        public bool SubscribeOnPurchased(SimpleCallback callback)
        {
            if (purchaser != null)
            {
                purchaser.OnPurhcased += callback;

                return true;
            }

            return false;
        }

        public bool UnsubscribeOnPurchased(SimpleCallback callback)
        {
            if (purchaser != null)
            {
                purchaser.OnPurhcased -= callback;

                return true;
            }

            return false;
        }

        public bool SubscribeOnResourcePlaced(SimpleCallback callback)
        {
            if (purchaser != null)
            {
                purchaser.OnResourcePlaced += callback;

                return true;
            }

            return false;
        }

        public bool UnsubscribeOnResourcePlaced(SimpleCallback callback)
        {
            if (purchaser != null)
            {
                purchaser.OnResourcePlaced -= callback;

                return true;
            }

            return false;
        }

        public bool SubscribeOnConstructed(SimpleCallback callback)
        {
            if (constructionPoint != null)
            {
                constructionPoint.OnConstructed += callback;

                return true;
            }

            return false;
        }

        public bool UnsubscribeOnConstructed(SimpleCallback callback)
        {
            if (constructionPoint != null)
            {
                constructionPoint.OnConstructed -= callback;

                return true;
            }

            return false;
        }

        public bool SubscribeOnGotHit(SimpleCallback callback)
        {
            if (constructionPoint != null)
            {
                constructionPoint.OnGotHit += callback;

                return true;
            }

            return false;
        }

        public bool UnsubscribeOnGotHit(SimpleCallback callback)
        {
            if (constructionPoint != null)
            {
                constructionPoint.OnGotHit -= callback;

                return true;
            }

            return false;
        }

        public bool SubscribeOnFullyUnlocked(SimpleCallback callback)
        {
            if (constructionPoint != null)
            {
                constructionPoint.OnConstructed += callback;
                return true;
            }
            else if (purchaser != null)
            {
                purchaser.OnPurhcased += callback;
                return true;
            }

            return false;
        }

        public bool UnsubscribeOnFullyUnlocked(SimpleCallback callback)
        {
            if (constructionPoint != null)
            {
                constructionPoint.OnConstructed -= callback;
                return true;
            }
            else if (purchaser != null)
            {
                purchaser.OnPurhcased -= callback;
                return true;
            }

            return false;
        }

        #endregion

        #region Editor

        protected bool EditorCanBePurchased()
        {
            return purchaser != null && !isOpenFromStart;
        }

        protected bool EditorHavePurhcasePoint()
        {
            return purchaser != null;
        }

        protected bool EditorCanBeConstructed()
        {
            return !(isOpenFromStart || constructionPoint == null);
        }

        public void UpdatePurchaseCostInEditor()
        {
            if (purchaser != null)
                purchaser.UpdateCostInEditor(cost);

            if (constructionPoint != null)
                constructionPoint.UpdateCostInEditor(ConstructionHitsRequired);
        }

        #endregion
    }
}