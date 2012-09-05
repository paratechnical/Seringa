using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Seringa.Engine.Utils
{
    public static class XmlHelpers
    {
        public static T GetObjectFromXml<T>(string docName, string elementType, string elementUserFriendlyName)
        {
            var doc = XDocument.Load(docName);

            var elem = doc.Descendants(elementType)
                            .SingleOrDefault(e => e.Attribute("user-friendly-name").Value == elementUserFriendlyName);

            T createdObj = (T)Activator.CreateInstance(typeof(T),elem);

            return createdObj;
        }

        public static IList<string> GetAllAttributeValuesFromDoc(string docName,string elementName,string attributeName)
        {
            IList<string> results = new List<string>();

            var doc = XDocument.Load(docName);

            results = doc.Descendants(elementName).Attributes(attributeName).ToList().Cast<string>().ToList();

            return results;
        }

        public static string GetElementValue(XElement element, string name)
        {
            if (element == null)
                return String.Empty;
            return element.Value;
        }

        public static T GetElementValue<T>(XElement element, T defaultValue)
        {
            T result = defaultValue;
            if (element != null)
            {
                string value = element.Value;
                result = (T)Convert.ChangeType(value, typeof(T));
            }
            return result;
        }

        public static string GetAttributeValue(XElement element, string name)
        {
            if ((element == null) || (element.Attribute(name).Value == null))
                return String.Empty;
            return element.Attribute(name).Value;
        }

        public static T GetAttributeValue<T>(XElement element, string name, T defaultValue)
        {
            T result = defaultValue;
            if ((element != null) || (element.Attribute(name).Value != null))
            {
                string value = element.Attribute(name).Value;
                result = (T)Convert.ChangeType(value, typeof(T));  
            }
            return result;
        }
    }
}
