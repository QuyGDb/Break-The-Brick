using UnityEngine;

namespace Watermelon
{
    [SetupTab("IAP", texture = "icon_iap")]
    public class IAPSettings : ScriptableObject
    {
        [Group("Settings")]
        [SerializeField] GameObject messagesCanvasPrefab;
        public GameObject MessagesCanvasPrefab => messagesCanvasPrefab;

        [SerializeField] IAPItem[] storeItems;
        public IAPItem[] StoreItems => storeItems;
    }
}