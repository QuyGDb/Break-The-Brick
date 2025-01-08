using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorEventListener : MonoBehaviour
    {
        private static readonly BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private object linkedClass;
        private Type linkedClassType;

        private Dictionary<int, MethodInfo> cachedMethods = new Dictionary<int, MethodInfo>();

        public void Initialise(object linkedClass)
        {
            this.linkedClass = linkedClass;

            linkedClassType = linkedClass.GetType();
        }

        public void CacheMethods(params string[] methods)
        {
            if(!methods.IsNullOrEmpty())
            {
                foreach(string methodName in methods)
                {
                    if (!string.IsNullOrEmpty(methodName))
                    {
                        int eventHash = methodName.GetHashCode();
                        if (!cachedMethods.ContainsKey(eventHash))
                        {
                            MethodInfo method = linkedClassType.GetMethod(methodName, BINDING_FLAGS);
                            if (method != null)
                            {
                                cachedMethods.Add(eventHash, method);
                            }
                            else
                            {
                                Debug.Log(string.Format("Class ({0}) doesn't have the implemented method {1} or has an incorrect access level", linkedClassType, methodName));
                            }
                        }
                    }
                }
            }
        }

        private void OnEvent(string methodName)
        {
            if(!string.IsNullOrEmpty(methodName))
            {
                int eventHash = methodName.GetHashCode();
                if(cachedMethods.ContainsKey(eventHash))
                {
                    cachedMethods[eventHash].Invoke(linkedClass, null);
                }
                else
                {
                    MethodInfo method = linkedClassType.GetMethod(methodName, BINDING_FLAGS);
                    if(method != null)
                    {
                        cachedMethods.Add(eventHash, method);

                        method.Invoke(linkedClass, null);
                    }
                    else
                    {
                        Debug.Log(string.Format("Class ({0}) doesn't have the implemented method {1} or has an incorrect access level", linkedClassType, methodName));
                    }
                }
            }
        }
    }
}