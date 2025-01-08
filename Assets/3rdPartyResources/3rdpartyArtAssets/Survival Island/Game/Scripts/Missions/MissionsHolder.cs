using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class MissionsHolder : MonoBehaviour
    {
        public Mission[] Missions { get; private set; }

        public void Initialise()
        {
            List<Mission> missions = new List<Mission>();

            for (int i = 0; i < transform.childCount; i++)
            {
                Mission mission = transform.GetChild(i).GetComponent<Mission>();

                if (mission != null)
                {
                    missions.Add(mission);
                }
            }

            Missions = missions.ToArray();
        }
    }
}