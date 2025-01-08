using Unity.AI.Navigation;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(NavMeshSurface))]
    public class SubworldBehavior : BaseWorldBehavior
    {
        [ReadOnly, BoxFoldout("AA", "Auto-Assigned", 99)]
        [SerializeField] SubworldExit[] exits;
        public SubworldExit[] Exits => exits;

        private NavMeshSurface navMeshSurface;

        public event SimpleCallback SubworldActivated;
        public event SimpleCallback SubworldDisabled;

        private void Awake()
        {
            navMeshSurface = GetComponent<NavMeshSurface>();
        }

        public override void Initialise()
        {
            base.Initialise();

            for (int i = 0; i < exits.Length; i++)
            {
                exits[i].Initialise(this);
            }
        }

        /// <summary>
        /// Invokes before a player enters the subworld.
        /// Use this method to register NavMesh and to activate subworld-dependent behaviors.
        /// </summary>
        public void Activate()
        {
            NavMeshController.AddNavMeshSurface(navMeshSurface);

#if MODULE_CURVE
            if (curveOverride != null)
                curveOverride.Apply();
#endif

            EnvironmentController.SetPreset(EnvironmentPresetType);

            SubworldActivated?.Invoke();
        }

        /// <summary>
        /// Invokes before a player leaves the subworld.
        /// </summary>
        public void Disable()
        {
            NavMeshController.RemoveNavMeshSurface(navMeshSurface);

            SubworldDisabled?.Invoke();
        }

        public override void Unload()
        {
            base.Unload();

            taskHandler.Unload();
        }

        public override void OnSceneSaving()
        {
            base.OnSceneSaving();

            SubworldExit[] cachedExits = transform.GetComponentsInChildren<SubworldExit>();
            if (!cachedExits.SafeSequenceEqual(exits))
            {
                exits = cachedExits;

                RuntimeEditorUtils.SetDirty(this);
            }
        }
    }
}