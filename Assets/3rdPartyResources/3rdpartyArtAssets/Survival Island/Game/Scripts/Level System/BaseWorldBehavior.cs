using System.Linq;
using UnityEngine;

namespace Watermelon
{
    public abstract class BaseWorldBehavior : MonoBehaviour, ISceneSavingCallback
    {
        [UniqueID]
        [SerializeField]
        protected string worldSaveName = "world01";

        [ReadOnly, BoxFoldout("AA", "Auto-Assigned", 99)]
        [SerializeField] Component[] registredWorldElements;
        [ReadOnly, BoxFoldout("AA", "Auto-Assigned", 99)]
        [SerializeField] Component[] registeredNavmeshElements;

        [Space]
        [SerializeField] EnvironmentPresetType environmentPresetType;
        public EnvironmentPresetType EnvironmentPresetType => environmentPresetType;

        [Space]
        [SerializeField]
        protected Transform spawnPoint;
        public Transform SpawnPoint => spawnPoint;

        [Space(5)]
        [SerializeField] Transform helpersRestZone;
        [SerializeField] Vector2 helpersRestZoneSize = new Vector2(2f, 2f);

#if MODULE_CURVE
        [Space]
        [SerializeField] protected CurvatureSceneOverride curveOverride;
#endif

        protected BaseWorldBehavior linkedWorld;

        public virtual bool IsSubworld => linkedWorld != null;

        protected TaskHandler taskHandler;
        public TaskHandler TaskHandler => taskHandler;

        private IWorldElement[] worldElements;

        private MusicSource musicSource;
        public MusicSource MusicSource => musicSource;

        public virtual void Initialise()
        {
            taskHandler = new TaskHandler();
            taskHandler.Initialise();

            worldElements = new IWorldElement[registredWorldElements.Length];
            for (int i = 0; i < worldElements.Length; i++)
            {
                worldElements[i] = (IWorldElement)registredWorldElements[i];
                worldElements[i].LinkedWorldBehavior = this;
            }

            musicSource = GetComponent<MusicSource>();

            if (musicSource != null)
            {
                musicSource.Initialise();
            }
        }

        public void OnWorldLoaded()
        {
            foreach (IWorldElement element in worldElements)
            {
                if (element != null)
                {
                    element.OnWorldLoaded();
                }
            }

#if MODULE_CURVE
            if (curveOverride != null)
                curveOverride.Apply();
#endif

            EnvironmentController.SetPreset(EnvironmentPresetType);
        }

        public void OnPlayerEntered()
        {
            if(musicSource != null)
            {
                musicSource.Activate();
                musicSource.SetAsDefault();
            }
            else
            {
                DefaultMusicController.ActivateMusic();
            }
        }

        public void OnWorldNavMeshRecalculated()
        {
            if (!registeredNavmeshElements.IsNullOrEmpty())
            {
                foreach (Component navMeshAgent in registeredNavmeshElements)
                {
                    ((INavMeshAgent)navMeshAgent).OnNavMeshInitialised();
                }
            }
        }

        public virtual void Unload()
        {
            foreach (var element in worldElements)
            {
                if (element != null)
                {
                    element.OnWorldUnloaded();
                }
            }

#if MODULE_CURVE
            if (curveOverride != null)
                curveOverride.Clear();
#endif

            if (musicSource != null)
                musicSource.Unload();

            EnvironmentController.OnWorldUnloaded();
        }

        public void SetLinkedWorld(BaseWorldBehavior linkedWorld)
        {
            this.linkedWorld = linkedWorld;
        }

        public Vector3 GetHelperRestPosition()
        {
            Transform restZoneTransform = helpersRestZone;
            if (restZoneTransform == null)
                restZoneTransform = transform;

            return new Vector3(restZoneTransform.position.x + Random.Range(-helpersRestZoneSize.x, helpersRestZoneSize.x), restZoneTransform.position.y, restZoneTransform.position.z + Random.Range(-helpersRestZoneSize.y, helpersRestZoneSize.y));
        }

        private void OnDrawGizmos()
        {
            if (helpersRestZone != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(helpersRestZone.position, new Vector3(helpersRestZoneSize.x, 1, helpersRestZoneSize.y));
            }
        }

        [Button("Add Music Override", "IsMusicComponentExist", ButtonVisibility.HideIf)]
        private void AddOverrideMusicComponent()
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;

            MusicSource musicSource = gameObject.AddComponent<MusicSource>();
            musicSource.MoveComponentUp();

            Debug.Log("You need to link a music audio clip to the AudioSource component", audioSource);
        }

        private bool IsMusicComponentExist()
        {
            return gameObject.GetComponent<MusicSource>() != null;
        }

        public virtual void OnSceneSaving()
        {
            Component[] cachedRegistredWorldElements = GetComponentsInChildren(typeof(IWorldElement)).OrderByDescending(x => ((IWorldElement)x).InitialisationOrder).ToArray();
            if (!cachedRegistredWorldElements.SafeSequenceEqual(registredWorldElements))
            {
                registredWorldElements = cachedRegistredWorldElements;

                RuntimeEditorUtils.SetDirty(this);
            }

            Component[] cachedNavmeshElements = GetComponentsInChildren(typeof(INavMeshAgent));
            if (!cachedNavmeshElements.SafeSequenceEqual(registeredNavmeshElements))
            {
                registeredNavmeshElements = cachedNavmeshElements;

                RuntimeEditorUtils.SetDirty(this);
            }
        }
    }
}