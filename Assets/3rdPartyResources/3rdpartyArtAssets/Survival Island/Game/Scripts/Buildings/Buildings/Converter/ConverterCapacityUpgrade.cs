using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ConverterCapacityUpgrade : LocalUpgrade<CapacityData>
    {
        public override string GetUpgradeDescription(int stageId)
        {
            try
            {
                var prevValue = GetStage(stageId).Value.outputStorageCapacity;
                var value = GetStage(stageId + 1).Value.outputStorageCapacity;

                return string.Format(DescriptionFormat, prevValue, value);
            }
            catch
            {
                return "";
            }
        }
    }

    [System.Serializable]
    public class CapacityData
    {
        public int inputStorageCapacity = 10;
        public int outputStorageCapacity = 10;
    }
}
