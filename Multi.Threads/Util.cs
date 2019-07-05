using System;
using System.Collections.Generic;
using System.Linq;

namespace Multi.Threads
{
    internal static class Util
    {
        internal static T Next<T>(this List<T> list) where T : class, new()
        {
            if (list.Count == 0)
            {
                return null;
            }

            var el = list.First();
            list.RemoveAt(0);
            return el;
        }
    }
}
