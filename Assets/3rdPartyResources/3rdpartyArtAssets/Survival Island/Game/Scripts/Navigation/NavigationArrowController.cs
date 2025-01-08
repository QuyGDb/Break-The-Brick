using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Class that controlls dynamic direction pointer.
    /// Has LateUpdate method called from outside to update active pointers direction.
    /// Has Unload method to unload all active pointers at once.
    /// </summary>
    [System.Serializable]
    public class DirecitonPointersController
    {
        [SerializeField] GameObject arrowDirectionPointerPrefab;
        [SerializeField] GameObject arrowsLineDirectionPointerPrefab;

        private static List<BaseDirectionPointerCase> activePointers = new List<BaseDirectionPointerCase>();

        private static Pool arrowPointerPool;
        private static Pool arrowLinePointerPool;

        public void Initialise()
        {
            arrowPointerPool = new Pool(new PoolSettings()
            {
                autoSizeIncrement = true,
                objectsContainer = null,
                size = 0,
                type = Pool.PoolType.Single,
                singlePoolPrefab = arrowDirectionPointerPrefab,
                name = "Arrow Pointer"
            });

            arrowLinePointerPool = new Pool(new PoolSettings()
            {
                autoSizeIncrement = true,
                objectsContainer = null,
                size = 0,
                type = Pool.PoolType.Single,
                singlePoolPrefab = arrowsLineDirectionPointerPrefab,
                name = "Arrow Line Pointer"
            });

            activePointers = new List<BaseDirectionPointerCase>();
        }

        public static ArrowPointerCase RegisterArrowPointer(Transform parent, Vector3 target)
        {
            ArrowPointerCase arrowCase = new ArrowPointerCase(parent, arrowPointerPool.GetPooledObject(), target);

            activePointers.Add(arrowCase);

            return arrowCase;
        }

        public static ArrowLinePointerCase RegisterArrowLinePointer(Transform parent, Vector3 target)
        {
            ArrowLinePointerCase arrowCase = new ArrowLinePointerCase(parent, arrowLinePointerPool.GetPooledObject(), target);

            activePointers.Add(arrowCase);

            return arrowCase;
        }

        public void LateUpdate()
        {
            if (activePointers.Count > 0)
            {
                for (int i = 0; i < activePointers.Count; i++)
                {
                    activePointers[i].LateUpdate();

                    if (activePointers[i].IsTargetReached)
                    {
                        activePointers.RemoveAt(i);

                        i--;
                    }
                }
            }
        }

        public void Unload()
        {
            activePointers.Clear();

            arrowPointerPool.ReturnToPoolEverything(true);
            arrowLinePointerPool.ReturnToPoolEverything(true);
        }
    }

    public abstract class BaseDirectionPointerCase : IDistanceToggle
    {
        protected Vector3 targetPosition;

        protected Transform parentTransform;

        protected Transform arrowTransform;

        public bool IsTargetReached { get; protected set; }

        public bool DistanceToggleActivated { get; protected set; }
        public bool IsDistanceToggleInCloseMode { get; protected set; }

        private float showingDistance = 4;
        public float ActivationDistanceOfDT => showingDistance;

        public Vector3 OriginPositionOfDT => targetPosition;

        public BaseDirectionPointerCase(Transform parentTransform, GameObject arrowObject, Vector3 targetPosition)
        {
            this.parentTransform = parentTransform;
            this.targetPosition = targetPosition;

            // Get transform refference and reset parent
            arrowTransform = arrowObject.transform;
            arrowTransform.SetParent(null);

            // Enable arrow object
            arrowObject.SetActive(true);

            DistanceToggleActivated = true;
            IsDistanceToggleInCloseMode = false;

            arrowTransform.gameObject.SetActive(true);

            // Add object to distance toggle
            DistanceToggle.AddObject(this);
        }

        public abstract void LateUpdate();

        public abstract void Disable();

        public abstract void PlayerEnteredZone();
        public abstract void PlayerLeavedZone();
    }

    public class ArrowPointerCase : BaseDirectionPointerCase
    {
        private Transform graphicsTransform;

        private TweenCase scaleTweenCase;
        private Vector3 defaultGraphicsScale;

        public ArrowPointerCase(Transform parentTransform, GameObject arrowObject, Vector3 targetPosition) : base(parentTransform, arrowObject, targetPosition)
        {
            // Prepare arrow position and rotation
            arrowTransform.position = parentTransform.position;
            arrowTransform.rotation = Quaternion.LookRotation((targetPosition - parentTransform.position).SetY(0).normalized, Vector3.up);

            // graphics is a child of parent object
            graphicsTransform = arrowTransform.GetChild(0);
            defaultGraphicsScale = graphicsTransform.localScale;
            graphicsTransform.localScale = Vector3.zero;

            // Do scale animation
            scaleTweenCase = graphicsTransform.DOScale(defaultGraphicsScale, 0.4f).SetEasing(Ease.Type.SineIn);
        }

        public override void LateUpdate()
        {
            // Fix arrow transform to player position
            arrowTransform.position = parentTransform.position;

            // Rotate arrow to target
            arrowTransform.rotation = Quaternion.Lerp(arrowTransform.rotation, Quaternion.LookRotation((targetPosition - parentTransform.position).SetY(0).normalized, Vector3.up), 0.075f);
        }

        public override void Disable()
        {
            if (IsTargetReached)
                return;

            IsTargetReached = true;

            DistanceToggleActivated = false;
            IsDistanceToggleInCloseMode = false;

            // Remove object from distance toggle
            DistanceToggle.RemoveObject(this);

            graphicsTransform.DOScale(0.0f, 0.4f).SetEasing(Ease.Type.SineOut).OnComplete(delegate
            {
                graphicsTransform.localScale = defaultGraphicsScale;
                arrowTransform.gameObject.SetActive(false);
            });
        }

        public override void PlayerEnteredZone()
        {
            // Hide arrow, player is near the target
            IsDistanceToggleInCloseMode = true;

            if (scaleTweenCase != null && scaleTweenCase.IsActive)
                scaleTweenCase.Kill();

            scaleTweenCase = graphicsTransform.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn);
        }

        public override void PlayerLeavedZone()
        {
            // Show arrow, player is far from the target
            IsDistanceToggleInCloseMode = false;

            if (scaleTweenCase != null && scaleTweenCase.IsActive)
                scaleTweenCase.Kill();

            graphicsTransform.transform.localScale = Vector3.zero;
            scaleTweenCase = graphicsTransform.DOScale(defaultGraphicsScale, 0.3f).SetEasing(Ease.Type.BackOut);
        }
    }

    public class ArrowLinePointerCase : BaseDirectionPointerCase
    {
        private static readonly int MATERIAL_ALPHA_HASH = Shader.PropertyToID("_Alpha");

        private const float DEFAULT_ALPHA = 0.5f;
        private const float DEFAULT_LINE_Y = 0.2f;

        private LineRenderer arrowLineRenderer;

        private MaterialPropertyBlock arrowPropertyBlock;

        private TweenCase fadeTweenCase;

        private Vector3[] linePositions;

        public ArrowLinePointerCase(Transform parentTransform, GameObject arrowObject, Vector3 targetPosition) : base(parentTransform, arrowObject, targetPosition)
        {
            arrowTransform.rotation = Quaternion.LookRotation((targetPosition - parentTransform.position).SetY(0).normalized, Vector3.up);

            arrowLineRenderer = arrowObject.GetComponent<LineRenderer>();

            // Create arrow property block
            arrowPropertyBlock = new MaterialPropertyBlock();

            linePositions = new Vector3[2];

            RecalculateLinePositions();

            // Do scale animation
            fadeTweenCase = arrowLineRenderer.DOPropertyBlockFloat(MATERIAL_ALPHA_HASH, arrowPropertyBlock, DEFAULT_ALPHA, 0.4f);
        }

        private void RecalculateLinePositions()
        {
            linePositions[0].x = 0;
            linePositions[0].y = DEFAULT_LINE_Y;
            linePositions[0].z = 0;

            linePositions[1].x = 0;
            linePositions[1].y = DEFAULT_LINE_Y;
            linePositions[1].z = Vector3.Distance(targetPosition, parentTransform.position) - 2;

            arrowLineRenderer.SetPositions(linePositions);
        }

        public override void LateUpdate()
        {
            arrowTransform.position = parentTransform.position;

            // Rotate arrow to target
            arrowTransform.rotation = Quaternion.LookRotation((targetPosition - parentTransform.position).SetY(0).normalized);

            RecalculateLinePositions();
        }

        public override void Disable()
        {
            if (IsTargetReached)
                return;

            IsTargetReached = true;

            DistanceToggleActivated = false;
            IsDistanceToggleInCloseMode = false;

            // Remove object from distance toggle
            DistanceToggle.RemoveObject(this);

            arrowLineRenderer.DOPropertyBlockFloat(MATERIAL_ALPHA_HASH, arrowPropertyBlock, 0, 0.4f).OnComplete(delegate
            {
                arrowTransform.gameObject.SetActive(false);
            });
        }

        public override void PlayerEnteredZone()
        {
            // Hide arrow, player is near the target
            IsDistanceToggleInCloseMode = true;

            if (fadeTweenCase != null && fadeTweenCase.IsActive)
                fadeTweenCase.Kill();

            // Do fade animation
            fadeTweenCase = arrowLineRenderer.DOPropertyBlockFloat(MATERIAL_ALPHA_HASH, arrowPropertyBlock, 0, 0.4f);
        }

        public override void PlayerLeavedZone()
        {
            // Show arrow, player is far from the target
            IsDistanceToggleInCloseMode = false;

            if (fadeTweenCase != null && fadeTweenCase.IsActive)
                fadeTweenCase.Kill();

            // Do fade animation
            fadeTweenCase = arrowLineRenderer.DOPropertyBlockFloat(MATERIAL_ALPHA_HASH, arrowPropertyBlock, DEFAULT_ALPHA, 0.4f);
        }
    }
}