using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Seringa.GUI.Helpers
{
    public class XmlTreeViewItem : TreeViewItem
    {
        public TreeViewItem DirectAncestor { get; set; }
        public string TagName { get; set; }
    }
}
