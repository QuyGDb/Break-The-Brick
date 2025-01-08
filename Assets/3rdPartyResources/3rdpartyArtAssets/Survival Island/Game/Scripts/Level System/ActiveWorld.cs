using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class ActiveWorld
    {
        private static WorldBehavior activeWorldBehavior;

        public static void Initialise(WorldBehavior behavior)
        {
            activeWorldBehavior = behavior;
        }

        public static void Unload()
        {
            activeWorldBehavior = null;
        }
    }
}