using UnityEngine;

namespace Watermelon
{
    public interface IResourcePicker
    {
        public bool AutoPickResources { get; }
        public Transform SnappingTransform { get; }

        public void OnResourcePickPerformed(ResourceDropBehavior dropBehavior);
    }
}