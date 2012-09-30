using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Seringa.GUI.Extensions
{
    public static  class Extensions
    {
        #region StringExtensions
        public static void AddOnUI<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }

        public static Dictionary<int, string> ToDictionary(this Enum @enum)
        {
            var type = @enum.GetType();
            return Enum.GetValues(type).Cast<object>().ToDictionary(e => (int)e, e => Enum.GetName(type, e));
        }

        #endregion StringExtensions
    }
}
