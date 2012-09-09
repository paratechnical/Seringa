using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Seringa.Engine.DataObjects;
using Seringa.Engine.Interfaces;
using System.IO;

namespace Seringa.Engine.Utils
{
    public static class XmlHelpers
    {
        public static bool CreateOrLoadMappingFile(string mappingFile,ref string error)
        {
            bool outcome = true;
            XDocument document = null;

            if (!File.Exists(mappingFile))
            {
                var file = File.Create(mappingFile);
                file.Dispose();
            }
            else
            {
                try
                {
                    document = XDocument.Load(mappingFile);
                }
                catch
                {
                }
            }

            try
            {
                if (document == null || (document != null && document.Element("map") == null))
                {
                    //create xml document from scratch
                    document = new XDocument(
                        new XElement("map",

                            new XElement("vulnerable-url", ""),
                            new XElement("dbms",
                                new XElement("users", "")
                                ),
                             new XElement("databases", "")
                        )
                    );

                    //save constructed document
                    document.Save(mappingFile);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                outcome = false;
            }

            return outcome;
        }

        public static string CreateProperMapToNodeFinderXpath(PayloadDetails payloadDetails,IInjectionStrategy strategy)
        {
            string result = string.Empty;

            result = payloadDetails.ParentNodeToMapTo;

            string[] replaceVars = payloadDetails.MapToParams.Split(',');

            for(int i=0;i<replaceVars.Count();i++)
            {
                result = result.Replace("{" + i + "}", PayloadHelpers.GetData(replaceVars[i], strategy));
            }

            return result;
        }

        public static string CreateProperMapToNodeCreatorXpath(PayloadDetails payloadDetails, string discoveredValue)
        {
            string result = string.Empty;
            result = payloadDetails.NodeToMapTo;
            if(!string.IsNullOrEmpty(payloadDetails.AttributeToMapTo))
                result += "[@"+payloadDetails.AttributeToMapTo+"='" + discoveredValue + "']";
            return result;
        }

        public static void SaveToMappingFile(string mappingFile,PayloadDetails payloadDetails,string discoveredValue, IInjectionStrategy strategy)
        {
            XDocument document = XDocument.Load(mappingFile);

            var element = document.XPathSelectElement(CreateProperMapToNodeFinderXpath(payloadDetails, strategy));

            if (element.XPathSelectElement(CreateProperMapToNodeCreatorXpath(payloadDetails,discoveredValue)) == null)
            {
                if(!string.IsNullOrEmpty(payloadDetails.AttributeToMapTo))
                    element.Add(
                        new XElement(payloadDetails.NodeToMapTo, new XAttribute(payloadDetails.AttributeToMapTo, discoveredValue))
                    );
                else
                    element.Add(new XElement(payloadDetails.NodeToMapTo, discoveredValue));

                //For simplicity, I just use the Save() method to overwrite the current .xml file
                document.Save(mappingFile);
            }
        }

        public static XElement GetXmlElementViaXpath(string docName, string xpath)
        {
            XElement result = null;
            XDocument doc = XDocument.Load(docName);
            result = GetXmlElementViaXpath(doc, xpath);
            return result;
        }

        public static XElement GetXmlElementViaXpath(XDocument doc, string xpath)
        {
            XElement result = null;
            //TODO:implement
            return result;
        }

        public static T GetObjectFromXml<T>(string docName, string elementType, string elementUserFriendlyName)
        {
            var doc = XDocument.Load(docName);

            var elem = doc.Descendants(elementType)
                            .SingleOrDefault(e => e.Attribute("user-friendly-name").Value == elementUserFriendlyName);

            T createdObj = (T)Activator.CreateInstance(typeof(T),elem);

            return createdObj;
        }

        public static IList<string> GetValuesFromDocByXpath(string docName,string xPath,string attributeName)
        {
            IList<string> results = new List<string>();

            var doc = XDocument.Load(docName);

            var attributes = doc.Root.XPathSelectElements(xPath).Attributes(attributeName);

            if(attributes.Count() > 0)
                results = attributes.Select(a => a.Value).ToList();

            return results;
        
        }

        public static IList<string> GetAllAttributeValuesFromDoc(string docName,string elementName,string attributeName)
        {
            IList<string> results = new List<string>();

            var doc = XDocument.Load(docName);

            results = doc.Descendants(elementName).Attributes(attributeName).Select(a => a.Value).Distinct().ToList();

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
            if ((element != null) && element.Attribute(name)!=null && (element.Attribute(name).Value != null))
            {
                string value = element.Attribute(name).Value;
                result = (T)Convert.ChangeType(value, typeof(T));  
            }
            return result;
        }
    }
}
