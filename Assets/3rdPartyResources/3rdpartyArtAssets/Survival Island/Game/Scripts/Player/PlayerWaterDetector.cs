using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class PlayerWaterDetector : MonoBehaviour
    {
        private static readonly Surface[] SURFACES = new Surface[] { Surface.None, Surface.Ground, Surface.Water, Surface.Shore };

        [BoxGroup("Settings", "Settings")]
        [SerializeField] int pointsCount = 3;
        [BoxGroup("Settings")]
        [SerializeField] int pointsPerFrame = 1;
        [BoxGroup("Settings")]
        [SerializeField] float pointOffset = 0.5f;

        [BoxGroup("AudioClip")]
        [SerializeField] AudioClipHandler waterAudioHelper;
        [BoxGroup("AudioClip")]
        [SerializeField] float maxVolume = 0.5f;
        [BoxGroup("AudioClip")]
        [SerializeField] float soundTransitionDuration = 0.3f;

        private List<AdditionalPointData> points;
        private int pointsInumerator;

        public bool IsActive { get; set; }

        public bool IsOnWater { get; private set; }
        public bool IsSwimming { get; private set; }
        public float SubmersionMultiplier { get; private set; }

        public Vector3 PlayerNavMeshPosition { get; private set; }

        public SimpleBoolCallback IsPlayerOnWater;
        public SimpleBoolCallback IsPlayerSwimming;
        public SimpleFloatCallback OnSwimmingMultiplierChanged;

        private void Awake()
        {
            points = new List<AdditionalPointData>();

            for(int i = 0; i < pointsCount; i++)
            {
                var angle = 360f / pointsCount * i + 20;

                var offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * pointOffset;

                points.Add(new AdditionalPointData(offset));
            }
        }

        private void OnDrawGizmos()
        {
            if(points == null) return;

            for(int i = 0; i < pointsCount; i++)
            {
                Gizmos.DrawSphere(transform.position + points[i].offset, 0.1f);
            }
        }

        private void Update()
        {
            if (!IsActive) return;

            // Is Player Currently On Water

            if (NavMesh.SamplePosition(transform.position, out var hit, 0.4f, NavMesh.AllAreas))
            {
                PlayerNavMeshPosition = hit.position;

                var oldValue = IsOnWater;
                IsOnWater = hit.mask == 8;

                if (oldValue != IsOnWater) IsPlayerOnWater?.Invoke(IsOnWater);
            }

            for(int i = 0; i < pointsPerFrame; i++)
            {
                CheckNextPoint();
            }

            if (IsOnWater)
            {
                int waterCount = 0;
                int shoreCount = 0;

                for(int i = 0; i < pointsCount; i++) 
                {
                    if (points[i].surface == Surface.Water) waterCount++;
                    if (points[i].surface == Surface.Shore) shoreCount++;
                }

                if(waterCount == pointsCount)
                {
                    if (!IsSwimming)
                    {
                        IsPlayerSwimming?.Invoke(true);
                        IsSwimming = true;

                        if (AudioController.AudioClips.waterSound != null)
                        {
                            waterAudioHelper.DoVolume(maxVolume, soundTransitionDuration);
                            waterAudioHelper.Play(AudioController.AudioClips.waterSound);
                        }
                    }
                } else
                {
                    if (shoreCount > 0) 
                    {
                        if (IsSwimming)
                        {
                            IsPlayerSwimming?.Invoke(false);
                            IsSwimming = false;

                            if (AudioController.AudioClips.waterSound != null)
                            {
                                waterAudioHelper.DoVolume(0, soundTransitionDuration).OnComplete(() => {
                                    waterAudioHelper.StopPlaying();
                                });
                            }
                        }

                        var oldMultiplier = SubmersionMultiplier;
                        SubmersionMultiplier = (float)waterCount / (float)pointsCount;
                        if (oldMultiplier != SubmersionMultiplier)
                        {
                            OnSwimmingMultiplierChanged?.Invoke(SubmersionMultiplier);
                        }
                    } else
                    {
                        if (!IsSwimming)
                        {
                            IsPlayerSwimming?.Invoke(true);
                            IsSwimming = true;

                            if (AudioController.AudioClips.waterSound != null)
                            {
                                waterAudioHelper.DoVolume(maxVolume, soundTransitionDuration);
                                waterAudioHelper.Play(AudioController.AudioClips.waterSound);
                            }
                        }
                    }
                }
            } else
            {
                if (IsSwimming)
                {
                    IsPlayerSwimming?.Invoke(false);
                    IsSwimming = false;

                    if (AudioController.AudioClips.waterSound != null)
                    {
                        waterAudioHelper.DoVolume(0, soundTransitionDuration).OnComplete(() => {
                            waterAudioHelper.StopPlaying();
                        });
                    }
                }

                if (SubmersionMultiplier != 0)
                {
                    SubmersionMultiplier = 0;
                    OnSwimmingMultiplierChanged?.Invoke(SubmersionMultiplier);
                }
            }
        }

        private void CheckNextPoint()
        {
            var point = points[pointsInumerator];

            if (NavMesh.SamplePosition(transform.position + point.offset, out var hit, 0.4f, NavMesh.AllAreas))
            {
                for(int i = 0; i < SURFACES.Length; i++)
                {
                    var surface = SURFACES[i];

                    if (hit.mask == (int)surface)
                    {
                        point.surface = surface;

                        break;
                    }
                }
            }

            points[pointsInumerator] = point;

            pointsInumerator = (pointsInumerator + 1) % pointsCount;
        }

        private struct AdditionalPointData
        {
            public Vector3 offset;
            public Surface surface;

            public AdditionalPointData(Vector3 offset)
            {
                this.offset = offset;
                surface = Surface.Ground;
            }
        }

        private enum Surface
        {
            None = 0,
            Ground = 1,
            Water = 8,
            Shore = 16,
            
        }
    }
}
