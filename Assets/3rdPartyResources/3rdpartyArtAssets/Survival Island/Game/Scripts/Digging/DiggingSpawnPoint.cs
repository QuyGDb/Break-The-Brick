using UnityEngine;

namespace Watermelon
{
    public class DiggingSpawnPoint : MonoBehaviour, IWorldElement, IGroundOpenable
    {
        [SerializeField] int spawnPriorityWeight = 1;
        public int SpawnPriorityWeight => spawnPriorityWeight;

        [Space]
        [SerializeField] bool isManuallyPlacedInWorld = false;

        [UniqueID]
        [ShowIf("isManuallyPlacedInWorld")]
        [SerializeField] string uniqueSaveID;

        [ShowIf("isManuallyPlacedInWorld")]
        [SerializeField] DiggingSpotBehavior manualDiggingPoint;

        [ShowIf("isManuallyPlacedInWorld")]
        [SerializeField] bool useForGlobalSpawn = true;

        public int InitialisationOrder => 0;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        private DiggingSpotBehavior activePointBehavior;

        public bool IsActive => activePointBehavior == null;

        private bool isGroundOpened;

        private TweenCase disableDelayTweenCase;
        private DiggingPointSave pointSave;
        
        public void OnWorldLoaded()
        {
            if(isManuallyPlacedInWorld)
            {
                pointSave = SaveController.GetSaveObject<DiggingPointSave>(uniqueSaveID);

                if (pointSave.IsCollected)
                {
                    if (manualDiggingPoint != null)
                    {
                        Destroy(manualDiggingPoint.gameObject);

                        manualDiggingPoint = null;
                        activePointBehavior = null;
                    }
                }
                else
                {
                    activePointBehavior = manualDiggingPoint;
                    activePointBehavior.LinkSpawnPoint(this);

                    if (isGroundOpened)
                    {
                        activePointBehavior.PlaySpawnAnimation();
                    }
                    else
                    {
                        activePointBehavior.gameObject.SetActive(false);
                    }
                }

                if(useForGlobalSpawn)
                {
                    DiggingController.RegisterSpawnPoint(this);
                }
            }
            else
            {
                DiggingController.RegisterSpawnPoint(this);
            }
        }

        public void OnWorldUnloaded()
        {
            disableDelayTweenCase.KillActive();

            if (activePointBehavior != null)
            {
                activePointBehavior.Unload();

                Destroy(activePointBehavior.gameObject);

                activePointBehavior = null;
            }
        }

        public DiggingSpotBehavior Spawn(GameObject diggingPointPrefab)
        {
            GameObject diggingPointObject = Instantiate(diggingPointPrefab);
            diggingPointObject.transform.position = transform.position;
            diggingPointObject.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            diggingPointObject.transform.localScale = Vector3.zero;

            DiggingSpotBehavior diggingPointBehavior = diggingPointObject.GetComponent<DiggingSpotBehavior>();
            diggingPointBehavior.LinkSpawnPoint(this);

            if(isGroundOpened)
            {
                diggingPointBehavior.PlaySpawnAnimation();
            }
            else
            {
                diggingPointBehavior.gameObject.SetActive(false);
            }

            activePointBehavior = diggingPointBehavior;

            return diggingPointBehavior;
        }

        public void OnPointCollected()
        {
            if(activePointBehavior != null)
            {
                activePointBehavior.Unload();

                disableDelayTweenCase = Tween.DelayedCall(3.0f, () =>
                {
                    DiggingController.OnDiggingPointCollected(activePointBehavior);

                    Destroy(activePointBehavior.gameObject);

                    activePointBehavior = null;
                });

                if(isManuallyPlacedInWorld)
                {
                    pointSave.IsCollected = true;
                }
            }
        }

        public void OnGroundOpen(bool immediately = false)
        {
            isGroundOpened = true;

            if (activePointBehavior != null)
            {
                activePointBehavior.gameObject.SetActive(true);
                activePointBehavior.PlaySpawnAnimation();
            }
        }

        public void OnGroundHidden(bool immediately = false)
        {
            isGroundOpened = false;
        }

        #region Development

#if UNITY_EDITOR
        private Color gizmoDefaultColorDev;

        private void OnValidate()
        {
            gizmoDefaultColorDev = Gizmos.color;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawWireSphere(transform.position, 0.5f);

            Gizmos.color = gizmoDefaultColorDev;
        }
#endif

        #endregion
    }
}