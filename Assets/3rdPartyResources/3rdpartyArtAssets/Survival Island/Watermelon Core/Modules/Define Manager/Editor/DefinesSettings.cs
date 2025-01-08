using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine;

namespace Watermelon
{
    public static class DefinesSettings
    {
        public static readonly string[] STATIC_DEFINES = new string[]
        {
            "UNITY_POST_PROCESSING_STACK_V2",
            "PHOTON_UNITY_NETWORKING",
            "PUN_2_0_OR_NEWER",
            "PUN_2_OR_NEWER",
        };

        public static readonly RegisteredDefine[] STATIC_REGISTERED_DEFINES = new RegisteredDefine[]
        {
            // System
            new RegisteredDefine("MODULE_INPUT_SYSTEM", "UnityEngine.InputSystem.InputManager", new string[] { "Packages/com.unity.inputsystem/InputSystem/InputManager.cs" }),

            // Core
            new RegisteredDefine("MODULE_MONETIZATION", "Watermelon.Monetization", new string[] { "Assets/Project Data/Watermelon Core/Extra Components/Monetization/Scripts/Monetization.cs" }),
            new RegisteredDefine("MODULE_IAP", "UnityEngine.Purchasing.UnityPurchasing", new string[] { "Packages/com.unity.purchasing/Runtime/Purchasing/UnityPurchasing.cs" }),
            new RegisteredDefine("MODULE_POWERUPS", "Watermelon.PUController", new string[] { "Assets/Project Data/Watermelon Core/Extra Components/Power Ups System/Scripts/PUController.cs" }),
            new RegisteredDefine("MODULE_HAPTIC", "Watermelon.Haptic", new string[] { "Assets/Project Data/Watermelon Core/Extra Components/Haptic/Scripts/Haptic.cs" }),
            new RegisteredDefine("MODULE_CURVE", "Watermelon.CurvatureManager", new string[] { "Assets/Project Data/Watermelon Core/Extra Components/Curve/CurvatureManager.cs" }),
        };

        public static List<RegisteredDefine> GetDynamicDefines()
        {
            //Get assembly
            List<Type> gameTypes = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly != null)
                {
                    try
                    {
                        Type[] tempTypes = assembly.GetTypes();

                        tempTypes = tempTypes.Where(m => m.IsDefined(typeof(DefineAttribute), true)).ToArray();

                        if (!tempTypes.IsNullOrEmpty())
                            gameTypes.AddRange(tempTypes);
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            List<RegisteredDefine> registeredDefines = new List<RegisteredDefine>();
            registeredDefines.AddRange(STATIC_REGISTERED_DEFINES);

            foreach (Type type in gameTypes)
            {
                //Get attribute
                DefineAttribute[] defineAttributes = (DefineAttribute[])Attribute.GetCustomAttributes(type, typeof(DefineAttribute));

                for (int i = 0; i < defineAttributes.Length; i++)
                {
                    if (!string.IsNullOrEmpty(defineAttributes[i].AssemblyType))
                    {
                        int methodId = registeredDefines.FindIndex(x => x.Define == defineAttributes[i].Define);
                        if (methodId == -1)
                        {
                            registeredDefines.Add(new RegisteredDefine(defineAttributes[i]));
                        }
                    }
                }
            }

            return registeredDefines;
        }
    }
}