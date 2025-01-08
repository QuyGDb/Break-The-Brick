#pragma warning disable CS0414 

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Watermelon
{
    [ExecuteInEditMode]
    public class ToolsPlacementHelper : MonoBehaviour
    {
#if UNITY_EDITOR
        [BoxGroup("Data")]
        [SerializeField, ReadOnly] Interaction interaction;

        [SerializeField, HideInInspector] bool isInited = false;
        
        private List<ToolBehavior> Tools { get; set; }
        private ToolBehavior Tool { get; set; }
        private int ToolIndex { get; set; }
        private AnimationClip AnimationClip { get; set; }

        [ShowIf("HasAnimation")]
        [SerializeField, Range(0, 1)] float animationTime;

        [SerializeField, HideInInspector] bool validatedOnPrefabOpened = false;

        [SerializeField, HideInInspector] bool isClonerActive = false;

        [Order(101)]
        [SerializeField, ShowIf("isClonerActive")] CharacterGraphics cloneDataFrom;

        [Order(100)]
        [Button("Activate Cloner", "isClonerActive", ButtonVisibility.HideIf)]
        private void ActivateCloner()
        {
            isClonerActive = true;
        }

        [Order(100)]
        [Button("Disable Cloner", "isClonerActive", ButtonVisibility.ShowIf)]
        private void DisableCloner()
        {
            isClonerActive = false;
        }

        [Order(102)]
        [Button("Clone", "isClonerActive", ButtonVisibility.ShowIf)]
        private void Clone()
        {
            if(cloneDataFrom != null)
            {
                interaction.CloneData(cloneDataFrom.InteractionAnimations);

                Debug.Log("The Tool Data has been cloned successfully");

                isClonerActive = false;
            }
        }

        [Order(103)]
        [Button("Clear Tools", "ShoudClearTools")]
        private void ClearTools()
        {
            if (interaction != null && interaction.ToolHolder != null && interaction.ToolHolder.childCount != 0)
            {
                for (int i = interaction.ToolHolder.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(interaction.ToolHolder.GetChild(i).gameObject);
                }
            }

            RuntimeEditorUtils.SetDirty(transform);
        }

        private bool ShoudClearTools()
        {
            return IsReadyToCalibrate() && interaction != null && interaction.ToolHolder != null && interaction.ToolHolder.childCount != 0;
        }

        public void Init()
        {
            var character = GetComponent<CharacterGraphics>();

            if (character == null) 
            {
                isInited = false;
                return;
            }

            interaction = character.InteractionAnimations;
            
            isInited = true;
        }

        private void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(isInited && interaction != null && Tool != null)
            {
                if(interaction.HasToolDataChanged(Tools[ToolIndex], Tool.transform))
                {
                    interaction.SetToolData(Tools[ToolIndex], Tool.transform);

                    RuntimeEditorUtils.SetDirty(transform);
                }
            } 
        }

        private void OnValidate()
        {
            if (!isInited || interaction != null) Init();

            if (HasAnimation())
            {
                SampleAnimation();
            }
        }

        [Order(0)]
        [Button("Start Calibration", "IsReadyToCalibrate", ButtonVisibility.ShowIf)]
        private void StartCalibration()
        {
            Tools = new List<ToolBehavior>();
            
            for(int i = 0 ; i < interaction.Data.AnimationsData.Length; i++) 
            { 
                var anim = interaction.Data.AnimationsData[i];
                if (anim.ToolBehaviorPrefab == null) continue;

                if(!Tools.Contains(anim.ToolBehaviorPrefab)) Tools.Add(anim.ToolBehaviorPrefab);
            }

            if(Tools.Count > 0)
            {
                Tool = Instantiate(Tools[0].gameObject).GetComponent<ToolBehavior>();
                Tool.transform.SetParent(interaction.ToolHolder);
                ToolIndex = 0;
                Tool.gameObject.hideFlags = HideFlags.DontSave;

                interaction.TransferToolData(Tools[ToolIndex], Tool.transform);

                AnimationClip = interaction.GetInteractionAnimation(Tools[ToolIndex]);

                AnimationMode.StartAnimationMode();
                PrefabStage.prefabStageClosing += OnPrefabClosing;

                animationTime = 0f;
                SampleAnimation();
            }
        }

        private void OnPrefabClosing(PrefabStage obj)
        {
            StopCalibration();
            validatedOnPrefabOpened = false;
        }


        [Button("Stop Calibration", "CanStopCalibration", ButtonVisibility.ShowIf)]
        private void StopCalibration()
        {
            var tool = Tool;

            DestroyImmediate(tool.gameObject);

            Tool = null;
            Tools = null;

            AnimationMode.StopAnimationMode();
            PrefabStage.prefabStageClosing -= OnPrefabClosing;

            RuntimeEditorUtils.SetDirty(transform);
        }

        [HorizontalGroup("Tool")]
        [Button("Prev Tool", "PrevToolAvailable", ButtonVisibility.ShowIf)]
        private void PrevTool()
        {
            var tool = Tool;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(tool.gameObject);
            };

            ToolIndex--;

            Tool = Instantiate(Tools[ToolIndex].gameObject).GetComponent<ToolBehavior>();
            Tool.transform.SetParent(interaction.ToolHolder);
            interaction.TransferToolData(Tools[ToolIndex], Tool.transform);
            Tool.gameObject.hideFlags = HideFlags.DontSave;

            AnimationClip = interaction.GetInteractionAnimation(Tools[ToolIndex]);

            animationTime = 0f;
            SampleAnimation();
        }

        [HorizontalGroup("Tool")]
        [Button("Next Tool", "NextToolAvailable", ButtonVisibility.ShowIf)]
        private void NextTool()
        {
            var tool = Tool;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(tool.gameObject);
            };
            ToolIndex++;

            Tool = Instantiate(Tools[ToolIndex].gameObject).GetComponent<ToolBehavior>();
            Tool.transform.SetParent(interaction.ToolHolder);
            interaction.TransferToolData(Tools[ToolIndex], Tool.transform);
            Tool.gameObject.hideFlags = HideFlags.DontSave;

            AnimationClip = interaction.GetInteractionAnimation(Tools[ToolIndex]);

            animationTime = 0f;
            SampleAnimation();
        }

        private void SampleAnimation()
        {
            if (AnimationClip != null)
            {
                var animatorObject = gameObject;

                if (gameObject.GetComponent<Animator>() == null)
                {
                    var animator = gameObject.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        animatorObject = animator.gameObject;
                    }
                }

                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(animatorObject, AnimationClip, Mathf.Lerp(0, AnimationClip.length, animationTime));
                AnimationMode.EndSampling();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsReadyToCalibrate() => isInited && Tool == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanStopCalibration() => Tool != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAnimation() => isInited && Tool != null && AnimationClip != null && AnimationMode.InAnimationMode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool NextToolAvailable() => Tool != null && ToolIndex < Tools.Count - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool PrevToolAvailable() => Tool != null && ToolIndex > 0;

#endif
    }
}