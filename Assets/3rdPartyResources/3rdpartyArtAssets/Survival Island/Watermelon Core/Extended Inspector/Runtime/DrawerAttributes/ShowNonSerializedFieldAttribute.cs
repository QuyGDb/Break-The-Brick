using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ShowNonSerializedAttribute : Attribute { }
}
