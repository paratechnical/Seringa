using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seringa.Engine.Utils
{
    public static class ListHelpers
    {
        public static IList<T> CommaSeparatedValuesToList<T>(string csv)
        {
            IList<T> results = new List<T>();

            IEnumerable<string> values = (IEnumerable<string>)csv.Split(',').ToList();
            if(values != null)
                results = values.Cast<T>().ToList();

            return results;
        }
        public static string ListToCommaSeparatedValues(IList<int> list)
        {
            string result = string.Empty;

            result = string.Join(",", list.ToArray());
            
            return result;
        }
    }
}
