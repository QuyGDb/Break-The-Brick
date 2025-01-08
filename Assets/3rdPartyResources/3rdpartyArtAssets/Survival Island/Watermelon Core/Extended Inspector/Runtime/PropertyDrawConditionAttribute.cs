using System;

namespace Watermelon
{
    public class PropertyConditionAttribute : BaseAttribute
    {
        public PropertyConditionAttribute(Type targetAttributeType) : base(targetAttributeType)
        {
        }
    }
}
