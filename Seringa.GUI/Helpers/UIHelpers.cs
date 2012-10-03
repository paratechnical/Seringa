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

        public static XmlTreeViewItem ClearTreeView(TreeView tv)
        {
            tv.Items.Clear();
            XmlTreeViewItem treeNode = new XmlTreeViewItem
            {
                //Should be Root
                Header = "Databases",
                IsExpanded = true
            };
            tv.Items.Add(treeNode);

            return treeNode;
        }

        public static XmlTreeViewItem GetTreeViewRoot(TreeView tv)
        {
            XmlTreeViewItem node = (XmlTreeViewItem)tv.Items[0];
            if(node != null)
                while (node.Parent != null && node.Parent.GetType() != typeof(TreeView))
                {
                    node = (XmlTreeViewItem)node.Parent;
                }

            return node;
        }

        public static XmlTreeViewItem GetXmlTreeViewItem(TreeView tv,string tagName, string name)
        {
            XmlTreeViewItem root = GetTreeViewRoot(tv);
            return GetXmlTreeViewItemRec(root, tagName, name);
        }

        public static XmlTreeViewItem GetXmlTreeViewItemRec(TreeViewItem node, string tagName, string name)
        {
            foreach (var item in node.Items)
            {
                if (((XmlTreeViewItem)item).TagName == tagName && ((XmlTreeViewItem)item).Header.ToString() == name)
                    return ((XmlTreeViewItem)item);
                GetXmlTreeViewItemRec(((XmlTreeViewItem)item), tagName, name);
            }

            return null;
        }

        

        public static void XmlTreeViewAdd(TreeViewItem afterItem, string tagName, string value)
        {
            XmlTreeViewItem treeNode = new XmlTreeViewItem
            {
                Header = value,
                TagName = tagName,
                IsExpanded = true,
            };

            XmlTreeViewAdd(afterItem, treeNode);
        }

        public static void XmlTreeViewAdd(TreeViewItem afterItem, TreeViewItem item)
        {
            afterItem.Items.Add(item);
        }

        public static void BuildNodes(XmlTreeViewItem treeNode, XElement element)
        {
            foreach (XNode child in element.Nodes())
            {
                XElement childElement = child as XElement;
                XmlTreeViewItem childTreeNode = new XmlTreeViewItem
                {
                    //Get First attribute where it is equal to value
                    Header = childElement.Attributes().First(s => s.Name == "name").Value,
                    TagName = childElement.Name.LocalName,
                    //Automatically expand elements
                    IsExpanded = true,
                };
                treeNode.Items.Add(childTreeNode);
                BuildNodes(childTreeNode, childElement);
            }
        }


    }
}
