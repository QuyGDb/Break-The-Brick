using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ResourceVisualStage: MonoBehaviour
    {
        [SerializeField] int id;
        public int Id => id;

        public float HealthThreshold { get; private set; }

        private List<ResourceChunk> chunks = new List<ResourceChunk>();

        public bool IsShown { get; private set; }

        [SerializeField] ResourceHarvestStageData data;

        public ResourcesList Drop { get; private set; } = new ResourcesList();

        private Vector3 scale;

        TweenCase tweenCase;

        Coroutine gravityCoroutine;

        private void Awake()
        {
            scale = transform.localScale;
        }

        public void Init(float healthThreshold)
        {
            HealthThreshold = healthThreshold;

            var meshRender = gameObject.GetComponent<MeshRenderer>();
            if (meshRender != null)
            {
                chunks.Add(new ResourceChunk(meshRender, data));
            }

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var childRenderer = gameObject.transform.GetChild(i).GetComponent<MeshRenderer>();

                if (childRenderer != null)
                {
                    chunks.Add(new ResourceChunk(childRenderer, data));
                }
            }

            IsShown = true;
        }

        public void Explode()
        {
            if(data != null)
            {
                var parentPos = gameObject.transform.position;
                var explosionPos = parentPos - (parentPos - PlayerBehavior.Position).SetY(0).normalized * 0.2f;

                foreach (var chunk in chunks)
                {
                    chunk.Activate(explosionPos + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)), data.ExplosionRadius, data.ExplosionForce.Random());
                }

                gravityCoroutine = Tween.InvokeCoroutine(CustomGravityCoroutine());

                tweenCase = Tween.DelayedCall(data.ScaleDuration + data.ScaleDelay + 0.1f, () =>
                {
                    gameObject.SetActive(false);

                    Tween.StopCustomCoroutine(gravityCoroutine);

                    ResetStage();
                });
            } else
            {
                gameObject.SetActive(false);
            }

            IsShown = false;
        }

        private IEnumerator CustomGravityCoroutine()
        {
            var waitForFoxed = new WaitForFixedUpdate();

            while (true)
            {
                yield return waitForFoxed;

                foreach (var chunk in chunks)
                {
                    chunk.AddCustomGravity(data.CustomGravity);
                }
            }
        }

        public void ResetStage()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].Reset();
            }
        }

        public void Show()
        {
            IsShown = true;
            gameObject.SetActive(true);

            transform.localScale = Vector3.zero;
            tweenCase = transform.DOScale(scale, 0.3f).SetEasing(Ease.Type.SineOut);
        }

        public void Clear()
        {
            if(gravityCoroutine != null) Tween.StopCustomCoroutine(gravityCoroutine);
            tweenCase.KillActive();
        }

        public class ResourceChunk
        {
            private Renderer renderer;
            private Rigidbody rigidbody;
            private BoxCollider collider;

            private TransformData initialData;
            private Transform parent;

            ResourceHarvestStageData Data;
            private bool usePhysics;

            public ResourceChunk(Renderer renderer, ResourceHarvestStageData data)
            {
                Data = data;
                this.renderer = renderer;

                usePhysics = data != null;

                if (usePhysics)
                {
                    collider = renderer.gameObject.AddComponent<BoxCollider>();

                    collider.center = renderer.localBounds.center;
                    collider.size = renderer.localBounds.size;
                    collider.enabled = false;

                    rigidbody = renderer.gameObject.AddComponent<Rigidbody>();

                    rigidbody.mass = collider.size.x * collider.size.y * collider.size.z * Data.WeightMultiplier;
                    rigidbody.drag = Data.Drag;
                    rigidbody.angularDrag = Data.AngularDrag;

                    rigidbody.useGravity = false;
                    rigidbody.isKinematic = true;
                    rigidbody.Sleep();
                }

                parent = renderer.transform.parent;

                initialData = renderer.transform;
            }

            public void Reset()
            {
                if (usePhysics)
                {
                    collider.enabled = false;

                    rigidbody.useGravity = false;
                    rigidbody.isKinematic = true;
                    rigidbody.Sleep();
                }

                renderer.transform.SetParent(parent);

                initialData.ApplyTo(renderer.transform);
            }

            public void Activate(Vector3 explosionPoint, float radius, float force)
            {
                if(usePhysics)
                {
                    rigidbody.transform.SetParent(null);

                    collider.enabled = true;

                    rigidbody.useGravity = Data.UseBildInGravity;
                    rigidbody.isKinematic = false;
                    rigidbody.WakeUp();

                    rigidbody.angularVelocity = Data.AngularForce.Random();
                    rigidbody.AddExplosionForce(force * rigidbody.mass, explosionPoint, radius, Data.UpwardForceModifier, ForceMode.Impulse);

                    rigidbody.DOScale(0, Data.ScaleDuration, Data.ScaleDelay).SetEasing(Ease.Type.SineIn);
                } else
                {
                    renderer.gameObject.SetActive(false);
                }
            }

            public void AddCustomGravity(float acceleration)
            {
                if(usePhysics)
                {
                    rigidbody.AddForce(Vector3.up * acceleration * rigidbody.mass);
                }
            }

            public struct TransformData
            {
                public Vector3 localPos;
                public Quaternion localRot;
                public Vector3 localScale;

                public static implicit operator TransformData(Transform transform) => new TransformData { localPos = transform.localPosition, localRot = transform.localRotation, localScale = transform.localScale };

                public void ApplyTo(Transform transform)
                {
                    transform.localPosition = localPos;
                    transform.localRotation = localRot;
                    transform.localScale = localScale;
                }
            }
        }
    }
}