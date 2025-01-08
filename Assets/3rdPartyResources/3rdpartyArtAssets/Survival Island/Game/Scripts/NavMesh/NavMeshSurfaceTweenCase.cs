using UnityEngine;
using Unity.AI.Navigation;

namespace Watermelon
{
    public class NavMeshSurfaceTweenCase : TweenCase
    {
        private NavMeshSurface navMeshSurface;

        private AsyncOperation asyncOperation;

        public NavMeshSurfaceTweenCase(NavMeshSurface navMeshSurface)
        {
            this.navMeshSurface = navMeshSurface;

            delay = 0;
            duration = float.MaxValue;

            isUnscaled = true;
            
            asyncOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        }

        public override void DefaultComplete()
        {

        }

        public override void Invoke(float deltaTime)
        {
            if (asyncOperation.isDone)
                Complete();
        }

        public override bool Validate()
        {
            return true;
        }
    }
}
