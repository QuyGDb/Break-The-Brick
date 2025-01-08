using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(BoxCollider))]
    public class SubworldExit : MonoBehaviour, IGroundOpenable
    {
        [SerializeField] bool overrideExitSpawnPoint;

        [ShowIf("overrideExitSpawnPoint")]
        [SerializeField] Transform exitSpawnPointTransform;

        private SubworldBehavior subworldBehavior;

        private BoxCollider boxCollider;
        private Vector3 defaultScale;

        private ISubworldEntrance overridenSubworldEntrance;

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();

            defaultScale = transform.localScale;
        }

        public void Initialise(SubworldBehavior subworldBehavior)
        {
            this.subworldBehavior = subworldBehavior;

            if (overrideExitSpawnPoint)
            {
                overridenSubworldEntrance = new SubworldEntranceData(subworldBehavior, transform, exitSpawnPointTransform);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                boxCollider.enabled = false;

                Control.DisableMovementControl();

                Overlay.Show(0.3f, () =>
                {
                    WorldController.WorldBehavior.SubworldHandler.LeaveSubworld(overridenSubworldEntrance, () =>
                    {
                        Overlay.Hide(0.3f, () =>
                        {
                            Control.EnableMovementControl();

                            boxCollider.enabled = true;
                        });
                    });
                });
            }
        }

        public void OnGroundOpen(bool immediately)
        {
            gameObject.SetActive(true);

            if (immediately)
            {
                transform.localScale = defaultScale;
            }
            else
            {
                transform.localScale = Vector3.zero;
                transform.DOScale(defaultScale, 0.3f).SetEasing(Ease.Type.SineOut);
            }
        }

        public void OnGroundHidden(bool immediately)
        {
            if (immediately)
            {
                gameObject.SetActive(false);
            }
            else
            {
                transform.DOScale(0, 0.3f).SetEasing(Ease.Type.SineOut).OnComplete(() => gameObject.SetActive(false));
            }
        }
    }
}