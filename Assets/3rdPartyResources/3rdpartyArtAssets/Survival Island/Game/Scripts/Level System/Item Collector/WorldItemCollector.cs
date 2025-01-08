using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class WorldItemCollector
    {
        private const float GARBAGE_COLLECT_COOLDOWN = 10.0f;

        private static List<WorldItemCollectorCase> itemGarbageCollector = new List<WorldItemCollectorCase>();
        private static Coroutine itemGarbageCoroutine;

        public static void Initialise()
        {
            itemGarbageCoroutine = Tween.InvokeCoroutine(GarbageCollectCoroutine());
        }

        private static IEnumerator GarbageCollectCoroutine()
        {
            float currentTime = Time.realtimeSinceStartup;
            float garbageCollectTime = currentTime + GARBAGE_COLLECT_COOLDOWN;

            while (true)
            {
                currentTime = Time.realtimeSinceStartup;
                if (currentTime > garbageCollectTime)
                {
                    if (itemGarbageCollector.Count > 0)
                    {
                        for (int i = 0; i < itemGarbageCollector.Count; i++)
                        {
                            if (!itemGarbageCollector[i].IsDisabled && itemGarbageCollector[i].DisableTime < currentTime)
                            {
                                itemGarbageCollector[i].Disable();
                            }

                            if (itemGarbageCollector[i].IsDisabled)
                            {
                                itemGarbageCollector.RemoveAt(i);

                                i--;
                            }
                        }
                    }

                    garbageCollectTime = currentTime + GARBAGE_COLLECT_COOLDOWN;
                }

                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
            }
        }

        public static WorldItemCollectorCase RegisterGameObject(IWorldItemCollector itemCollector, float disableTime)
        {
            WorldItemCollectorCase itemCollectorCase = new WorldItemCollectorCase(itemCollector, disableTime);
            itemGarbageCollector.Add(itemCollectorCase);

            return itemCollectorCase;
        }

        public static void DisableObjects()
        {
            if (!itemGarbageCollector.IsNullOrEmpty())
            {
                for (int i = 0; i < itemGarbageCollector.Count; i++)
                {
                    itemGarbageCollector[i].Disable();
                }

                itemGarbageCollector.Clear();
            }
        }

        public static void Unload()
        {
            if (itemGarbageCoroutine != null)
            {
                Tween.StopCustomCoroutine(itemGarbageCoroutine);

                itemGarbageCoroutine = null;
            }

            itemGarbageCollector.Clear();
        }
    }
}