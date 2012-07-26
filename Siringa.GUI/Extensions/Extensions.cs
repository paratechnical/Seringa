using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Siringa.GUI.Extensions
{
    public static  class Extensions
    {
        #region Extensions
        public static void AddOnUI<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }
        #endregion Extensions
    }
}
