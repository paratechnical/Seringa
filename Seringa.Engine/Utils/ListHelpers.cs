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

            if (!string.IsNullOrEmpty(csv))
            {
                List<string> values = new List<string>();
                if (csv.Contains(","))
                    values = csv.Split(',').ToList();
                else
                    values.Add(csv);
                if (values.Count > 0)
                    results = values.ConvertAll<T>(new Converter<string, T>(StringToType<T>));
            }

            return results;
        }
        public static string ListToCommaSeparatedValues(IList<int> list)
        {
            string result = string.Empty;

            result = string.Join(",", list.ToArray());
            
            return result;
        }

        public static T StringToType<T>(string input)
        {
            T output = default(T);
            output = (T)Convert.ChangeType(input, typeof(T));
            return output;
        }
    }
}
