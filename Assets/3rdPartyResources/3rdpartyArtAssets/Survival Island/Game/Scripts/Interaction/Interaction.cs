using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class Interaction
    {
        private const string ANIMATOR_LAYER_INTERACTIONS = "Interactions";
        private static readonly int ANIMATOR_BOOL_INTERACTION_HASH = Animator.StringToHash("Interaction");

        [SerializeField] InteractionData interactionData;
        [SerializeField] Transform toolHolderTransform;

        [SerializeField, HideInInspector] List<ToolOffsetData> toolOfsetData;

        public InteractionData Data => interactionData;
        public Transform ToolHolder => toolHolderTransform;

        private InteractionAnimationData undefinedAnimationData;
        private InteractionAnimationData[] animationsData;

        private Animator characterAnimator;
        private AnimatorOverrideController animatorOverrideController;
        private int interactionsLayer;

        private InteractionAnimationData activeAnimation;
        public InteractionAnimationData ActiveAnimation => activeAnimation;

        private TweenCase layerWeightTweenCase;

        public bool IsAnimationActive => activeAnimation != null;

        private ToolBehavior activeToolBehavior;
        public ToolBehavior ActiveToolBehavior => activeToolBehavior;

        public event CustomEventCallback CustomEventInvoked;

        public void Initialise(Animator characterAnimator)
        {
            this.characterAnimator = characterAnimator;

            undefinedAnimationData = interactionData.DefaultAnimationData;
            animationsData = interactionData.AnimationsData;

            animatorOverrideController = (AnimatorOverrideController)characterAnimator.runtimeAnimatorController;

            // Store interactions layer
            interactionsLayer = characterAnimator.GetLayerIndex(ANIMATOR_LAYER_INTERACTIONS);

            if(interactionsLayer == -1)
            {
                Debug.LogError("Interactions layer is missing in the character animator!", characterAnimator.gameObject);
            }
        }

        public void Activate(InteractionAnimationType animationType)
        {
            if (activeAnimation != null)
            {
                if(activeAnimation.AnimationType == animationType)
                    return;

                if (activeToolBehavior != null)
                {
                    activeToolBehavior.OnToolDisabled();

                    Object.Destroy(activeToolBehavior.gameObject);

                    CustomEventInvoked = null;

                    activeToolBehavior = null;
                }
            }

            InteractionAnimationData animationData = GetAnimationData(animationType);

            layerWeightTweenCase.KillActive();

            characterAnimator.SetLayerWeight(interactionsLayer, 1.0f);
            characterAnimator.SetBool(ANIMATOR_BOOL_INTERACTION_HASH, true);
            animatorOverrideController["Interaction"] = animationData.AnimationClip;

            activeAnimation = animationData;

            if (animationData.ToolBehaviorPrefab != null)
            {
                activeToolBehavior = SpawnTool(animationData.ToolBehaviorPrefab);
                activeToolBehavior.OnToolEnabled();
            }
        }

        public void Disable()
        {
            if (activeAnimation != null)
            {
                if(activeToolBehavior != null)
                {
                    activeToolBehavior.OnToolDisabled();

                    Object.Destroy(activeToolBehavior.gameObject);

                    CustomEventInvoked = null;

                    activeToolBehavior = null;
                }

                activeAnimation = null;
            }

            layerWeightTweenCase.KillActive();
            layerWeightTweenCase = characterAnimator.DOLayerWeight(interactionsLayer, 0.0f, 0.5f);

            characterAnimator.SetBool(ANIMATOR_BOOL_INTERACTION_HASH, false);
        }

        private ToolBehavior SpawnTool(ToolBehavior prefab)
        {
            GameObject toolObject = Object.Instantiate(prefab.gameObject, toolHolderTransform);

            TransferToolData(prefab, toolObject.transform);

            return toolObject.GetComponent<ToolBehavior>();
        }

        public void OverrideAnimation(string animationName, AnimationClip animationClip)
        {
            animatorOverrideController[animationName] = animationClip;
        }

        public bool IsAnimationExists(InteractionAnimationType animationType)
        {
            for (int i = 0; i < animationsData.Length; i++)
            {
                if (animationsData[i].AnimationType == animationType)
                    return true;
            }

            return false;
        }

        public InteractionAnimationData GetAnimationData(InteractionAnimationType animationType)
        {
            for (int i = 0; i < animationsData.Length; i++)
            {
                if (animationsData[i].AnimationType == animationType)
                    return animationsData[i];
            }

            return undefinedAnimationData;
        }

        public void InvokeHitEvent()
        {
            if(activeToolBehavior != null)
            {
                activeToolBehavior.OnHitPerformed();
            }
        }

        public void InvokeCustomEvent(string eventName)
        {
            if (activeToolBehavior != null)
                activeToolBehavior.OnCustomEventInvoked(eventName);

            CustomEventInvoked?.Invoke(eventName);
        }

        public delegate void CustomEventCallback(string eventName);

        public void CloneData(Interaction other)
        {
            if (toolOfsetData == null) toolOfsetData = new List<ToolOffsetData>();
            toolOfsetData.Clear();

            for (int i = 0; i < other.toolOfsetData.Count; i++) 
            {
                toolOfsetData.Add(other.toolOfsetData[i].Clone());
            }
        }

        public AnimationClip GetInteractionAnimation(ToolBehavior databaseReference)
        {
            for(int i = 0; i < Data.AnimationsData.Length; i++)
            {
                var data = Data.AnimationsData[i];

                if (data.ToolBehaviorPrefab == databaseReference)
                {
                    return data.AnimationClip;
                }
            }
            return null;
        }

        public void TransferToolData(ToolBehavior databaseReference, Transform toolTransform)
        {
            if (toolOfsetData == null) toolOfsetData = new List<ToolOffsetData>();

            for (int i = 0; i < toolOfsetData.Count; i++)
            {
                var data = toolOfsetData[i];
                if(data.Tool == databaseReference)
                {
                    data.Apply(toolTransform);

                    return;
                }
            }

            toolOfsetData.Add(new ToolOffsetData(databaseReference, toolTransform));
        }

        public void SetToolData(ToolBehavior databaseReference, Transform toolTransform)
        {
            if (toolOfsetData == null) toolOfsetData = new List<ToolOffsetData>();

            for (int i = 0; i < toolOfsetData.Count; i++)
            {
                var data = toolOfsetData[i];
                if (data.Tool == databaseReference)
                {
                    data.SetData(toolTransform);

                    return;
                }
            }

            toolOfsetData.Add(new ToolOffsetData(databaseReference, toolTransform));
        }

        public bool HasToolDataChanged(ToolBehavior databaseReference, Transform toolTransform)
        {
            if (toolOfsetData == null) toolOfsetData = new List<ToolOffsetData>();

            for (int i = 0; i < toolOfsetData.Count; i++)
            {
                var data = toolOfsetData[i];
                if (data.Tool == databaseReference)
                {
                    return data.HasDataChanged(toolTransform);
                }
            }

            return true;
        }

        [System.Serializable]
        public class ToolOffsetData
        {
            [SerializeField] ToolBehavior tool;
            public ToolBehavior Tool { get => tool; set => tool = value; }

            [SerializeField] Vector3 localOffset;
            [SerializeField] Vector3 localEulerAngles;

            private ToolOffsetData(ToolBehavior databaseReference)
            {
                Tool = databaseReference;
            }

            public ToolOffsetData(ToolBehavior databaseReference, Transform tool) 
            { 
                Tool = databaseReference;
                SetData(tool);
            }

            public void Apply(Transform tool)
            {
                tool.localPosition = localOffset;
                tool.localEulerAngles = localEulerAngles;
            }

            public void SetData(Transform tool)
            {
                localOffset = tool.localPosition;
                localEulerAngles = tool.localEulerAngles;
            }

            public bool HasDataChanged(Transform tool)
            {
                return tool.localPosition != localOffset || tool.localEulerAngles != localEulerAngles;
            }

            public ToolOffsetData Clone()
            {
                var data = new ToolOffsetData(tool);
                data.localOffset = localOffset;
                data.localEulerAngles = localEulerAngles;

                return data;
            }
        }
    }
}