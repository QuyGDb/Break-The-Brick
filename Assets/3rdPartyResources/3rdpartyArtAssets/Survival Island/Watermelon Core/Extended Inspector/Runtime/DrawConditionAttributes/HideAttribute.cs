using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HideAttribute : ConditionAttribute
    {
        public HideAttribute()
        {

        }
    }
}