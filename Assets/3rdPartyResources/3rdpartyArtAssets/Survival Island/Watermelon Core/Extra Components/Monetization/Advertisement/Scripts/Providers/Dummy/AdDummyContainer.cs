#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class AdDummyContainer
    {
        [SerializeField] BannerPosition bannerPosition = BannerPosition.Bottom;
        public BannerPosition BannerPosition => bannerPosition;

        [SerializeField] GameObject dummyCanvasPrefab;
        public GameObject DummyCanvasPrefab => dummyCanvasPrefab;
    }
}