using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class FoliageController : MonoBehaviour
    {
        private static List<Transform> foliageAgents = new List<Transform>();

        private static bool changed = false;

        public static void RegisterFoliageAgent(Transform transform)
        {
            if (foliageAgents.Contains(transform) || foliageAgents.Count > 10) return;

            foliageAgents.Add(transform);

            changed = true;
        }

        public static void RemoveFoliageAgent(Transform transform)
        {
            if (foliageAgents.Contains(transform))
            {
                foliageAgents.Remove(transform);

                changed = true;
            }
        }

        private void Update()
        {
            if(changed)
            {
                changed = false;
                Shader.SetGlobalInteger("_FoliagePositionsCount", foliageAgents.Count);
            }
            
            for(int i = 0; i < foliageAgents.Count; i++)
            {
                Shader.SetGlobalVectorArray("_FoliagePositions", foliageAgents.ConvertAll((agent) => (Vector4) agent.position).ToArray());
            }

        }

        private void OnDestroy()
        {
            Shader.SetGlobalInteger("_FoliagePositionsCount", 0);
        }
    }
}
