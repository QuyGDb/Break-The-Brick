using UnityEngine;

namespace Watermelon
{
    public class SubworldEntranceData : ISubworldEntrance
    {
        private SubworldBehavior subworldBehavior;
        public SubworldBehavior SubworldBehavior => subworldBehavior;

        private Transform subworldSpawnPoint;
        public Transform SubworldSpawnPoint => subworldSpawnPoint;

        private Transform exitSpawnPoint;
        public Transform ExitSpawnPoint => exitSpawnPoint;

        public SubworldEntranceData(SubworldBehavior subworldBehavior, Transform subworldSpawnPoint, Transform exitSpawnPoint)
        {
            this.subworldBehavior = subworldBehavior;
            this.subworldSpawnPoint = subworldSpawnPoint;
            this.exitSpawnPoint = exitSpawnPoint;
        }
    }
}