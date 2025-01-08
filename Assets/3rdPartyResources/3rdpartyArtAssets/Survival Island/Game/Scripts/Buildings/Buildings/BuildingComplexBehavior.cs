using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class BuildingComplexBehavior : AbstractComplexBehavior<BuildingBehavior, PurchasePoint>, IGroundOpenable
    {
        private List<NavMeshObstacle> obstacles = new List<NavMeshObstacle>();

        public override void Awake()
        {
            base.Awake();

            GetComponentsInChildren(true, obstacles);
        }

        public override void Purchase()
        {
            base.Purchase();

            for(int i = 0; i < obstacles.Count; i++)
            {
                obstacles[i].carveOnlyStationary = true;
            }

            Tween.DelayedCall(0.5f, () =>
            {
                for (int i = 0; i < obstacles.Count; i++)
                {
                    obstacles[i].carveOnlyStationary = false;
                }
            });
        }

        public void OnGroundOpen(bool immediately = false)
        {
            gameObject.SetActive(true);

            Init();
        }

        public void OnGroundHidden(bool immediately = false)
        {
            gameObject.SetActive(false);
        }
    }
}