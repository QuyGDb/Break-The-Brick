using System.Reflection;
using System;
using System.Collections.Generic;

namespace Watermelon
{
    [PropertyCondition(typeof(HideAttribute))]
    public class HidePropertyDrawCondition : PropertyCondition
    {
        public override bool CanBeDrawn(WatermelonEditor editor, FieldInfo fieldInfo, object targetObject, List<Type> nestedTypes)
        {
            return false;
        }
    }
}