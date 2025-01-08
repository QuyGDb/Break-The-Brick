using System;

namespace Watermelon
{
    public static class UniqueIDUtils
    {
        public static string GetUniqueID()
        {
            Guid guid = Guid.NewGuid();

            return guid.ToString();
        }
    }
}
