using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Enemies Database", menuName = "Content/Enemies Database")]
    public class EnemiesDatabase : ScriptableObject
    {
        [SerializeField] EnemyData[] enemies;
        public EnemyData[] Enemies => enemies;

        private Dictionary<EnemyType, PoolGeneric<BaseEnemyBehavior>> enemyPools;

        public void Init()
        {
            enemyPools = new Dictionary<EnemyType, PoolGeneric<BaseEnemyBehavior>>();

            for(int i = 0; i < enemies.Length; i++)
            {
                var type = enemies[i].EnemyType;
                var prefab = enemies[i].Prefab;

                var pool = new PoolGeneric<BaseEnemyBehavior>(new PoolSettings(prefab, 5, true));

                enemyPools.Add(type, pool);
            }
        }

        public BaseEnemyBehavior GetEnemyBehavior(EnemyType type)
        {
            if (enemyPools.ContainsKey(type))
            {
                return enemyPools[type].GetPooledComponent();
            }

            return null;
        }

        public EnemyData GetEnemyData(EnemyType type)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].EnemyType.Equals(type))
                    return enemies[i];
            }

            Debug.LogError("[Enemies Database] Enemy of type " + type + " + is not found!");
            return enemies[0];
        }
    }
}
