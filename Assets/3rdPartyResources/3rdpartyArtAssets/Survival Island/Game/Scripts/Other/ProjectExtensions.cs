using System.Linq;
using UnityEngine;

namespace Watermelon
{
    public static class ProjectExtensions
    {
        public static bool SafeSequenceEqual<T>(this T[] array1, T[] array2)
        {
            if (array1 == null && array2 == null)
            {
                // Both arrays are null, considered equal
                return true; 
            }

            if (array1 == null || array2 == null)
            {
                // One array is null, considered not equal
                return false; 
            }

            // Both arrays are not null, compare using SequenceEqual
            return array1.SequenceEqual(array2);
        }
    }
}
