using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class EnemySpawnPoint : MonoBehaviour, IGroundOpenable, IWorldElement
    {
        [SerializeField] EnemyType enemyType;
        [SerializeField] Transform spawnPoint;
        [SerializeField] AudioClip spawnSound;
        [SerializeField] float playerDetectionRadius = 10;

        [BoxFoldout("Respawn", label: "Respawn", order: 10)]
        [SerializeField] bool shouldRespawnAfterDeath = true;
        [BoxFoldout("Respawn")]

        [ShowIf("shouldRespawnAfterDeath")]
        [SerializeField] float respawnDuration = 10;

        public bool IsActive { get => enabled; private set => enabled = value; }

        public BaseEnemyBehavior Enemy { get; private set; }

        public int InitialisationOrder => 100;

        public BaseWorldBehavior LinkedWorldBehavior { get; set; }

        private float spawnAllowedTime;

        private int spawnCounter = 0;

        private void Awake()
        {
            IsActive = false;

            spawnCounter = 0;
        }

        private void Update()
        {
            if (!shouldRespawnAfterDeath && spawnCounter > 0)
                return;

            if (Time.time >= spawnAllowedTime && Time.frameCount % 10 == 0 && Enemy == null)
            {
                if (Vector3.Distance(transform.position, PlayerBehavior.Position) < playerDetectionRadius)
                {
                    Enemy = GameController.Data.EnemiesDatabase.GetEnemyBehavior(enemyType);

                    Enemy.Spawn(spawnPoint);

                    Enemy.OnDeath += OnEnemyDied;

                    spawnCounter++;

                    spawnAllowedTime = float.MaxValue;

                    if (spawnSound != null)
                        AudioController.PlaySound(spawnSound, 0.7f);
                }
            }
        }

        private void OnEnemyDied()
        {
            spawnAllowedTime = Time.time + respawnDuration;
            Enemy = null;
        }

        public void OnGroundHidden(bool immediately)
        {
            gameObject.SetActive(false);
        }

        public void OnGroundOpen(bool immediately)
        {
            if (!immediately)
            {
                transform.localScale = Vector3.zero;
                transform.DOScale(1, 0.3f).SetEasing(Ease.Type.BounceOut);
            }
            gameObject.SetActive(true);
            IsActive = true;
        }

        public void OnWorldLoaded()
        {

        }

        public void OnWorldUnloaded()
        {
            if (Enemy != null)
            {
                Enemy.Unload();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            int count = 50;

            Vector3[] circle = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                var angle = 360f / count * i;
                var rotation = Quaternion.Euler(0, angle, 0);
                var offset = rotation * Vector3.forward * playerDetectionRadius;

                circle[i] = transform.position + Vector3.up + offset;
            }

            Gizmos.DrawLineStrip(circle, true);
        }
    }
}