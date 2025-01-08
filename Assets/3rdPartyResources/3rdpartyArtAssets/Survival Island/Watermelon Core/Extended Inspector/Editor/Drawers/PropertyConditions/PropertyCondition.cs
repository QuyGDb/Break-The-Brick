using System.Collections.Generic;
using System.Reflection;
using System;

namespace Watermelon
{
    public abstract class PropertyCondition
    {
        public abstract bool CanBeDrawn(WatermelonEditor editor, FieldInfo fieldInfo, object targetObject, List<Type> nestedTypes);
    }
}
