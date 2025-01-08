using UnityEngine;

namespace Watermelon
{
    public static class PhysicsHelper
    {
        public static readonly int LAYER_DEFAULT = LayerMask.NameToLayer("Default");
        public static readonly int LAYER_PLAYER = LayerMask.NameToLayer("Player");
        public static readonly int LAYER_CHARACTER = LayerMask.NameToLayer("Character");
        public static readonly int LAYER_HELPER = LayerMask.NameToLayer("Helper");
        public static readonly int LAYER_GROUND = LayerMask.NameToLayer("Ground");
        public static readonly int LAYER_HITTABLE = LayerMask.NameToLayer("Hittable");
        public static readonly int LAYER_INTERACTABLE_OBJECT = LayerMask.NameToLayer("Interactable Object");
        public static readonly int LAYER_RESOURCE = LayerMask.NameToLayer("Resource");

        public const string TAG_PLAYER = "Player";

        public static Quaternion RandomQuaternion()
        {
            return Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
        }
    }
}