using UnityEngine;

namespace Watermelon
{
    public abstract class WorldChangeSpecialBehavior : MonoBehaviour
    {
        public abstract void OnGroundTileOpened(bool immediately);
        public abstract void OnWorldChanged(SimpleCallback worldChangeCallback);
    }
}