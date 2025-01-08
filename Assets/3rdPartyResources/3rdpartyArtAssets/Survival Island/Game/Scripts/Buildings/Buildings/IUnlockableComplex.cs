using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public interface IUnlockableComplex
    {
        string ID { get; }
       
        bool CanBePurchased { get; }
        bool CanBeConstructed { get; }
        
        Vector3 Position { get; }
        ResourcesList Cost { get; }
        ResourcesList CostLeft { get; }
        
        int ConstructionHitsRequired { get; }
        int HitsMade { get; }

        void Purchase();
        void DisablePurchase();
        void EnablePurchase();

        bool SubscribeOnPurchased(SimpleCallback callback);
        bool UnsubscribeOnPurchased(SimpleCallback callback);
        bool SubscribeOnResourcePlaced(SimpleCallback callback);
        bool UnsubscribeOnResourcePlaced(SimpleCallback callback);

        void Construct();
        void DisableCounstructing();
        void EnableConstructing();

        bool SubscribeOnConstructed(SimpleCallback callback);
        bool UnsubscribeOnConstructed(SimpleCallback callback);
        bool SubscribeOnGotHit(SimpleCallback callback);
        bool UnsubscribeOnGotHit(SimpleCallback callback);

        Sprite GetConstrutionIcon();

        bool SubscribeOnFullyUnlocked(SimpleCallback callback);
        bool UnsubscribeOnFullyUnlocked(SimpleCallback callback);
    }
}