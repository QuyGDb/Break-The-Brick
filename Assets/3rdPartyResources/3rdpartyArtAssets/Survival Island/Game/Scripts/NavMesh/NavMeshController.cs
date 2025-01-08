using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace Watermelon
{
    public static class NavMeshController
    {
        private static List<NavMeshSurface> navMeshSurfaces = new List<NavMeshSurface>();
        public static List<NavMeshSurface> NavMeshSurface => navMeshSurfaces;

        private static bool isNavMeshCalculated;
        public static bool IsNavMeshCalculated => isNavMeshCalculated;

        public static event SimpleCallback NavMeshRecalculated;

        private static bool navMeshRecalculating;
        private static Coroutine updateCoroutine;

        public static void AddNavMeshSurface(NavMeshSurface navMeshSurface)
        {
            if (navMeshSurfaces.FindIndex(x => x == navMeshSurface) == -1)
                navMeshSurfaces.Add(navMeshSurface);

            isNavMeshCalculated = false;
        }

        public static void RemoveNavMeshSurface(NavMeshSurface navMeshSurface)
        {
            int surfaceIndex = navMeshSurfaces.FindIndex((x) => x == navMeshSurface);
            if(surfaceIndex != -1)
            {
                navMeshSurfaces.RemoveAt(surfaceIndex);
            }
        }

        public static void CalculateNavMesh(SimpleCallback simpleCallback = null)
        {
            if (navMeshRecalculating)
                return;

            navMeshRecalculating = true;

            updateCoroutine = Tween.InvokeCoroutine(CalculationCoroutine(() =>
            {
                isNavMeshCalculated = true;
                navMeshRecalculating = false;

                simpleCallback?.Invoke();

                NavMeshRecalculated?.Invoke();
                NavMeshRecalculated = null;
            }));
        }

        private static IEnumerator CalculationCoroutine(SimpleCallback onRecalculated)
        {
            AsyncOperation updateOperation;

            foreach(var navMeshSurface in navMeshSurfaces)
            {
                updateOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

                while(!updateOperation.isDone)
                {
                    yield return null;
                }
            }

            onRecalculated?.Invoke();
        }

        public static void InvokeOrSubscribe(SimpleCallback callback)
        {
            if (isNavMeshCalculated)
            {
                callback?.Invoke();
            }
            else
            {
                NavMeshRecalculated += callback;
            }
        }

        public static void Reset()
        {
            if (updateCoroutine != null)
            {
                Tween.StopCustomCoroutine(updateCoroutine);

                updateCoroutine = null;
            }

            navMeshRecalculating = false;
            isNavMeshCalculated = false;

            NavMeshRecalculated = null;

            navMeshSurfaces.Clear();
        }
    }
}
