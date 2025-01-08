using UnityEngine;
using System;

namespace Watermelon
{
    public abstract class InitModule : ScriptableObject
    {
        public abstract string ModuleName { get; }

        public abstract void CreateComponent(GameObject holderObject);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterModuleAttribute : Attribute
    {
        public string Path;
        public bool Core;

        public RegisterModuleAttribute(string path, bool core = false)
        {
            Path = path;
            Core = core;
        }
    }
}