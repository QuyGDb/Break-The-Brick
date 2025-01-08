using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DefineAttribute : Attribute
    {
        private string define;
        public string Define => define; 
        
        private string assemblyType;
        public string AssemblyType => assemblyType;

        private string[] linkedFiles;
        public string[] LinkedFiles => linkedFiles;

        public DefineAttribute(string define, string assemblyType = "", string[] linkedFiles = null)
        {
            this.define = define;

            this.assemblyType = assemblyType;
            this.linkedFiles = linkedFiles;
        }
    }
}
