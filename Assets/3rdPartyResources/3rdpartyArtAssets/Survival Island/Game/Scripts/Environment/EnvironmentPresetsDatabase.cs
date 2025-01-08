using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Environment Presets Database", menuName = "Content/Weather/Environment Presets Database")]
    public class EnvironmentPresetsDatabase : ScriptableObject
    {
        [SerializeField] List<EnvironmentPreset> presets;

        public EnvironmentPreset GetPreset(EnvironmentPresetType type)
        {
            for(int i = 0; i < presets.Count; i++)
            {
                if (presets[i].Type == type) return presets[i];
            }

            return null;
        }
    }
}
