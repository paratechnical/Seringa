using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Xml;

namespace Seringa.GUI.Helpers
{
    public static class UIHelpers
    {

        public static void BuildNodes(XmlTreeViewItem treeNode, XElement element)
        {
            foreach (XNode child in element.Nodes())
            {
                XElement childElement = child as XElement;
                XmlTreeViewItem childTreeNode = null;
                
                childTreeNode = new XmlTreeViewItem
                {
                    //Get First attribute where it is equal to value
                    Header = childElement.Attributes().First(s => s.Name == "name").Value,
                    TagName = childElement.Name.LocalName,
                    DirectAncestor = treeNode,
                    //Automatically expand elements
                    IsExpanded = true
                };
                treeNode.Items.Add(childTreeNode);
                BuildNodes(childTreeNode, childElement);
            }
        }


    }
}
