using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(GroundTileComplexBehavior), true)]
    public class GroundTileComplexBehaviourEditor : WatermelonEditor
    {
        private const string REASSIGN_BOOL_PATH = "reassign_ground_tiles_everywhere";
        private bool isFoldoutExpanded;

        private GroundTileComplexBehavior replacementTile;

        private UnityEngine.Object prefab;
        private GroundTileComplexBehavior currentTile;
        private GroundTileComplexBehavior newTile;
        private bool isReassignEnabled;

        protected override void OnEnable()
        {
            base.OnEnable();

            isReassignEnabled = EditorPrefs.GetBool(REASSIGN_BOOL_PATH, false);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (targets.Length > 1)
            {
                EditorGUILayout.LabelField("Multi edit is not supported");
            }

            isFoldoutExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(isFoldoutExpanded, "Development");

            if (isFoldoutExpanded)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                replacementTile = EditorGUILayout.ObjectField("Replace with", replacementTile, typeof(GroundTileComplexBehavior), true) as GroundTileComplexBehavior;

                if (EditorGUI.EndChangeCheck())
                {
                    if (replacementTile != null)
                    {
                        prefab = null;

                        if (PrefabUtility.IsPartOfPrefabAsset(replacementTile))
                        {
                            prefab = replacementTile.gameObject;
                        }
                        else
                        {
                            prefab = PrefabUtility.GetCorrespondingObjectFromSource(replacementTile.gameObject);

                            if (!PrefabUtility.IsPartOfPrefabAsset(prefab))
                            {
                                prefab = null;
                            }

                        }
                    }
                    else
                    {
                        prefab = null;
                    }
                }

                EditorGUI.BeginDisabledGroup(prefab == null);

                if (GUILayout.Button("Replace"))
                {
                    Replace();
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                isReassignEnabled = EditorGUILayout.ToggleLeft("Reassign Ground Tiles Everywhere", isReassignEnabled);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool(REASSIGN_BOOL_PATH, isReassignEnabled);
                }

            }

            EditorGUILayout.EndFoldoutHeaderGroup();

        }

        private void Replace()
        {
            currentTile = target as GroundTileComplexBehavior;
            newTile = PrefabUtility.InstantiatePrefab(prefab, currentTile.transform.parent).GetComponent<GroundTileComplexBehavior>();
            newTile.transform.position = currentTile.transform.position;
            newTile.transform.rotation = currentTile.transform.rotation;
            newTile.transform.localScale = currentTile.transform.localScale;
            newTile.transform.SetSiblingIndex(currentTile.transform.GetSiblingIndex() + 1);

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                GameObject.Destroy(currentTile.gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(currentTile.gameObject);
            }

            Selection.activeGameObject = newTile.gameObject;

            if (isReassignEnabled)
            {
                PlacementHelper.ResetChunksForAll();
            }
        }
    }
}
