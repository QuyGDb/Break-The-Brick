using System.Linq;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class SubworldHandler
    {
        [SerializeField] SubworldBehavior[] subworlds;

        private WorldBehavior worldBehavior;

        private ISubworldEntrance lastSubworldEntrance;

        private SubworldBehavior activeSubworld;
        public SubworldBehavior ActiveSubworld => activeSubworld;

        public bool IsSubworldActive => activeSubworld != null;

        public event SimpleCallback OnSubworldEnetered;
        public event SimpleCallback OnSubworldLeft;

        public void Initialise(WorldBehavior worldBehavior)
        {
            this.worldBehavior = worldBehavior;

            if (!subworlds.IsNullOrEmpty())
            {
                for (int i = 0; i < subworlds.Length; i++)
                {
                    subworlds[i].Initialise();
                    subworlds[i].SetLinkedWorld(worldBehavior);
                }
            }
        }

        public void Unload()
        {
            if (!subworlds.IsNullOrEmpty())
            {
                for (int i = 0; i < subworlds.Length; i++)
                {
                    subworlds[i].Unload();
                }
            }
        }

        public void DisableSubworld()
        {
            if (activeSubworld == null) return;

            activeSubworld.Disable();
            activeSubworld = null;

            worldBehavior.OnPlayerEntered();
            worldBehavior.OnSubworldLeft();

            OnSubworldLeft?.Invoke();
        }

        public void EnterSubworld(ISubworldEntrance subworldEntrance, SimpleCallback onSubworldLoaded)
        {
            SubworldBehavior subworldBehaviour = subworldEntrance.SubworldBehavior;
            if (subworldBehaviour != null)
            {
                lastSubworldEntrance = subworldEntrance;

                subworldBehaviour.Activate();

                activeSubworld = subworldBehaviour;

                NavMeshController.CalculateNavMesh(() =>
                {
                    PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();
                    playerBehavior.Warp(subworldEntrance.SubworldSpawnPoint);

                    onSubworldLoaded?.Invoke();

                    subworldBehaviour.OnPlayerEntered();

                    worldBehavior.OnSubworldEntered();
                    OnSubworldEnetered?.Invoke();
                });
            }
        }

        public void LeaveSubworld(ISubworldEntrance subworldEntrance, SimpleCallback onSubworldUnloaded)
        {
            if (subworldEntrance == null)
                subworldEntrance = lastSubworldEntrance;

            SubworldBehavior subworldBehaviour = subworldEntrance.SubworldBehavior;
            if (subworldBehaviour != null)
            {
                subworldBehaviour.Disable();

                activeSubworld = null;

                NavMeshController.CalculateNavMesh(() =>
                {
                    PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();
                    playerBehavior.Warp(subworldEntrance.ExitSpawnPoint);

                    onSubworldUnloaded?.Invoke();

                    worldBehavior.OnPlayerEntered();
                    worldBehavior.OnSubworldLeft();

                    OnSubworldLeft?.Invoke();
                });
            }
        }

        public bool CacheSubworlds()
        {
            SubworldBehavior[] cachedSubworlds = Object.FindObjectsOfType<SubworldBehavior>(false);
            if (!subworlds.SafeSequenceEqual(cachedSubworlds))
            {
                subworlds = cachedSubworlds;

                return true;
            }

            return false;
        }
    }
}