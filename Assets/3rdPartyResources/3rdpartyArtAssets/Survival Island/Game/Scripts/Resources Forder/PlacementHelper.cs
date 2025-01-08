#pragma warning disable CS0414 

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Watermelon
{
    [ExecuteInEditMode]
    public class PlacementHelper : MonoBehaviour
    {
        [SerializeField, HideInInspector] public Vector3 offset;
        [SerializeField, ReadOnly] public GroundTileBehavior tileBehavior;
        [SerializeField] bool showWarning = true;

        private double lastUpdated = 0;
        private static bool justOpenedEditor = true;

        private void Awake()
        {
#if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                return;
#endif

            if (Application.isPlaying)
            {
                IGroundOpenable openable = GetComponent<IGroundOpenable>();

                if (openable != null)
                {
                    if (tileBehavior != null)
                    {
                        tileBehavior.RegisterOpenable(openable);
                    }
                    else
                    {
                        Tween.NextFrame(() => openable.OnGroundOpen(true));
                    }
                }
                else
                {
                    Debug.LogError("Object " + name + " doesn't have script with IGroundOpenable interface");
                }

                Destroy(this);
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (EditorApplication.timeSinceStartup - lastUpdated < 0.5f && !justOpenedEditor)
                {
                    return;
                }

                lastUpdated = EditorApplication.timeSinceStartup;
                if (tileBehavior == null || tileBehavior.transform.position - transform.position != offset)
                {
                    FindChunk();

                    if (tileBehavior != null)
                    {
                        offset = tileBehavior.transform.position - transform.position;
                    }
                }

                if (Random.value < 0.001f)
                    RemoveAllGeneratedColliders();
            }
#endif
        }

        [HorizontalGroup("G1"), Button("Find ground")]
        public void FindChunk()
        {
#if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                return;
#endif
            if (Application.isPlaying)
                return;

            var tiles = FindObjectsOfType<GroundTileBehavior>();

            List<TileData> info = new List<TileData>();

            for (int i = 0; i < tiles.Length; i++)
            {
                var tile = tiles[i];

                for (int j = 0; j < tile.transform.childCount; j++)
                {
                    var child = tile.transform.GetChild(j);

                    if (child.gameObject.layer == PhysicsHelper.LAYER_GROUND)
                    {
                        var meshRenderer = child.GetComponent<MeshRenderer>();

                        var bounds = meshRenderer.bounds;
                        bounds.size = bounds.size.SetY(1000);

                        if (bounds.Contains(transform.position))
                        {
                            info.Add(new TileData { renderer = meshRenderer, tile = tile });
                        }
                    }
                }
            }

            GroundTileBehavior closestTile = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < info.Count; i++)
            {
                var data = info[i];

                var mesh = data.renderer.GetComponent<MeshFilter>().sharedMesh;

                var collider = data.renderer.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;

                if (collider.Raycast(new Ray(transform.position + Vector3.up * 20, Vector3.down), out var hit, 100))
                {
                    if (hit.distance < minDistance)
                    {
                        minDistance = hit.distance;
                        closestTile = data.tile;
                    }
                }
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(collider);
                };
#endif
            }

            var prevTile = tileBehavior;
            tileBehavior = closestTile;

            var water = GameObject.FindWithTag("Water");
            if (water != null)
            {
                var collider = water.GetComponent<BoxCollider>();

                if (collider != null)
                {
                    if (collider.Raycast(new Ray(transform.position + Vector3.up * 20, Vector3.down), out var hit, 100))
                    {
                        if (hit.distance < minDistance)
                        {
                            tileBehavior = null;
                        }
                    }
                }
            }

            if (tileBehavior != prevTile)
                RuntimeEditorUtils.SetDirty(this);
        }

        [HorizontalGroup("G1"), Button("ALL: Find ground")]
        public static void ResetChunksForAll()
        {
            var helpers = FindObjectsOfType<PlacementHelper>();

            foreach (var helper in helpers)
            {
                var tile = helper.tileBehavior;

                helper.FindChunk();

                if (tile != helper.tileBehavior)
                {
                    if (tile != null && helper.tileBehavior != null)
                    {
                        Debug.Log($"{tile.name} -> {helper.tileBehavior.name}");
                    }
                    else
                    {
                        Debug.Log("null");
                    }
                }

            }
        }

        [HorizontalGroup("G2"), Button("Snap to the ground")]
        public void SnapToTheGround()
        {
            FindChunk();

            if (tileBehavior != null)
            {
                transform.position = transform.position.SetY(tileBehavior.transform.position.y);
                RuntimeEditorUtils.SetDirty(transform);
            }
        }

        [HorizontalGroup("G2"), Button("ALL: Snap to the ground")]
        public static void SnapToTheGroundForAll()
        {
            PlacementHelper[] helpers = FindObjectsOfType<PlacementHelper>();

            foreach (PlacementHelper helper in helpers)
            {
                if (helper.tileBehavior == null)
                {
                    helper.FindChunk();
                }

                if (helper.tileBehavior != null && helper.transform.position.y != helper.tileBehavior.transform.position.y)
                {
                    helper.SnapToTheGround();

                    Debug.Log($"{helper.name} -> {helper.tileBehavior.transform.position.y}");
                }

            }
        }

        [Button("Random Rotation")]
        public void RandomRotation()
        {
            transform.eulerAngles = Vector3.zero.SetY(Random.Range(0f, 360f));

            RuntimeEditorUtils.SetDirty(transform);
        }

        public static void RemoveAllGeneratedColliders()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                var tiles = FindObjectsOfType<GroundTileBehavior>();

                for (int i = 0; i < tiles.Length; i++)
                {
                    var tile = tiles[i];

                    for (int j = 0; j < tile.transform.childCount; j++)
                    {
                        var child = tile.transform.GetChild(j);

                        if (child.gameObject.layer == PhysicsHelper.LAYER_GROUND)
                        {
                            var meshRenderer = child.GetComponent<MeshRenderer>();
                            var collider = meshRenderer.gameObject.GetComponent<MeshCollider>();

                            while (collider != null)
                            {
                                DestroyImmediate(collider);
                                collider = meshRenderer.gameObject.GetComponent<MeshCollider>();
                            }
                        };
                    }
                }
            };
#endif
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!showWarning)
                return;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                return;

            if (tileBehavior == null)
            {
                var style = new GUIStyle(EditorStyles.boldLabel);
                style.normal.textColor = Color.red;
                Handles.Label(transform.position, "PLACEMENT ERROR!", style);
            }
#endif
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void OpenMethood()
        {
            justOpenedEditor = true;
        }
#endif

        private struct TileData
        {
            public GroundTileBehavior tile;
            public MeshRenderer renderer;
        }
    }
}