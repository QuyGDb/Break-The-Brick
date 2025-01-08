using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(BoxCollider))]
    public class SubworldEntrance : MonoBehaviour, IGroundOpenable, ISubworldEntrance
    {
        [SerializeField] SubworldBehavior subworldBehavior;
        public SubworldBehavior SubworldBehavior => subworldBehavior;

        [SerializeField] Transform exitSpawnPoint;
        public Transform ExitSpawnPoint => exitSpawnPoint;

        [Space]
        [SerializeField] bool overrideSubworldSpawnPoint = false;

        [ShowIf("overrideSubworldSpawnPoint")]
        [SerializeField] Transform subworldSpawnPointOverride;
        public Transform SubworldSpawnPoint => subworldSpawnPointOverride;

        private BoxCollider boxCollider;

        private Vector3 defaultScale;

        public event SimpleCallback SubworldEntered;

        public bool FirstTimeEnabled { get; set; } = true;

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();

            defaultScale = transform.localScale;

            if (!overrideSubworldSpawnPoint)
            {
                subworldSpawnPointOverride = subworldBehavior.SpawnPoint;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!FirstTimeEnabled)
            {
                FirstTimeEnabled = true;
                return;
            }

            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                boxCollider.enabled = false;

                Control.DisableMovementControl();

                SubworldEntered?.Invoke();

                Overlay.Show(0.3f, () =>
                {
                    WorldController.WorldBehavior.SubworldHandler.EnterSubworld(this, () =>
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