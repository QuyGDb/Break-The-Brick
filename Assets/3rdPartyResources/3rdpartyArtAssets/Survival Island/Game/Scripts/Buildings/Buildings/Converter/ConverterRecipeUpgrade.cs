using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ConverterRecipeUpgrade : LocalUpgrade<Recipe>
    {
        public override string GetUpgradeDescription(int stageId)
        {
            try
            {
                var value = GetStage(stageId).Value.ResultResourceType;

                return string.Format(DescriptionFormat, value);
            }
            catch
            {
                return "";
            }
        }
    }
}