using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class CustomOverlayElementAttribute : Attribute
    {
        private string elementName;
        public string ElementName => elementName;

        private string methodName;
        public string MethodName => methodName;

        private int order = 0;
        public int Order => order;

        public CustomOverlayElementAttribute(string elementName, string methodName, int order = 0)
        {
            this.elementName = elementName;
            this.methodName = methodName;
            this.order = order;
        }
    }
}