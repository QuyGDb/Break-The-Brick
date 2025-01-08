using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class TentBuilding : BuildingBehavior
    {

        [SerializeField] SubworldEntrance entrance;


        protected override void RegisterUpgrades()
        {

        }

        public override void FullyUnlock()
        {
            base.FullyUnlock();

            entrance.FirstTimeEnabled = false;
        }

        public override void SpawnUnlocked()
        {
            base.SpawnUnlocked();

            entrance.FirstTimeEnabled = true;
        }
    }
}