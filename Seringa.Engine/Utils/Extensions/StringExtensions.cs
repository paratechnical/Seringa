using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seringa.Engine.Utils.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Removes the last slash in a string if such a slash exists
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveLastSlash(this string source)
        {
            return ((source.Last() == '/') ? source.Remove(source.Length - 1) : source);
        }
    }
}
