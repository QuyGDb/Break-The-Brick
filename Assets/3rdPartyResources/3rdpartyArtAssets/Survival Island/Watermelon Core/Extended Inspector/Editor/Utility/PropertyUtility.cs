using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Watermelon
{
    public static class PropertyUtility
    {
        public static readonly Type TYPE_BOOL = typeof(bool);

        public static GUIRenderer[] GroupRenderers(WatermelonEditor editor, IEnumerable<GUIRenderer> baseRenderers)
        {
            List<GroupGUIRenderer> groupGUIRenderers = new List<GroupGUIRenderer>();

            IGrouping<GroupAttribute, GUIRenderer>[] groupRenderers = baseRenderers.Where(x => x.GroupAttribute != null).GroupBy(x => x.GroupAttribute).ToArray();
            foreach (IGrouping<GroupAttribute, GUIRenderer> group in groupRenderers)
            {
                groupGUIRenderers.Add(new GroupGUIRenderer(editor, group.Key, group.ToList()));
            }

            Dictionary<string, GroupGUIRenderer> groupsDictionary = groupGUIRenderers.ToDictionary(g => g.GroupID, g => g);

            for (int i = 0; i < groupGUIRenderers.Count; i++)
            {
                GroupGUIRenderer group = groupGUIRenderers[i];
                if (!string.IsNullOrEmpty(group.ParentPath))
                {
                    if (groupsDictionary.TryGetValue(group.ParentPath, out var parentGroup))
                    {
                        parentGroup.AddRenderer(group);
                        groupGUIRenderers.Remove(group);

                        i--;
                    }
                }
            }

            GUIRenderer[] groupedRenderers = groupGUIRenderers.Concat(baseRenderers.Where(x => x.GroupAttribute == null)).OrderBy(x => x.Order).ToArray();

            groupGUIRenderers = null; 
            groupsDictionary = null;

            return groupedRenderers;
        }

        public static T GetAttribute<T>(FieldInfo fieldInfo) where T : Attribute
        {
            if (fieldInfo != null)
                return fieldInfo.GetCustomAttribute<T>();

            return null;
        }

        public static T GetAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
        {
            if (propertyInfo != null)
                return propertyInfo.GetCustomAttribute<T>();

            return null;
        }

        public static T GetAttribute<T>(SerializedProperty property) where T : Attribute
        {
            T[] attributes = GetAttributes<T>(property);
            if (attributes != null && attributes.Length > 0)
                return attributes[0];

            return null;
        }

        public static T[] GetAttributes<T>(SerializedProperty property) where T : Attribute
        {
            Type targetType = property.serializedObject.targetObject.GetType();

            foreach (Type type in GetClassNestedTypes(targetType))
            {
                FieldInfo fieldInfo = type.GetField(property.name, ReflectionUtils.FLAGS_INSTANCE);
                if (fieldInfo != null)
                {
                    return (T[])Attribute.GetCustomAttributes(fieldInfo, typeof(T));
                }
            }

            return null;
        }

        public static UnityEngine.Object GetTargetObject(SerializedProperty property)
        {
            return property.serializedObject.targetObject;
        }

        public static string GetSubstringBeforeLastSlash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            int lastSlashIndex = input.LastIndexOf('/');

            if (lastSlashIndex == -1 || lastSlashIndex == input.Length - 1)
            {
                return string.Empty;
            }

            return input.Substring(0, lastSlashIndex);
        }

        public static List<Type> GetClassNestedTypes(Type type)
        {
            List<Type> typesList = new List<Type> { type };

            Type lastAddedType = type;

            while (lastAddedType.BaseType != null)
            {
                lastAddedType = lastAddedType.BaseType;

                typesList.Add(lastAddedType);
            }

            return typesList;
        }
    }
}