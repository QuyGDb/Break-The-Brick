using UnityEngine;

namespace Watermelon
{
    public interface INavMeshAgent
    {
        void OnNavMeshInitialised();

        void OnNavMeshWaypointChanged(Vector3 targetPoint);
        void OnNavMeshAgentStartedMovement(Vector3 targetPoint);
        void OnNavMeshAgentStopped();

        void OnNavMeshWarpStarted();
        void OnNavMeshWarpFinished();
    }
}