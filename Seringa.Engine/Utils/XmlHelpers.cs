using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Seringa.Engine.DataObjects;
using Seringa.Engine.Interfaces;
using System.IO;
using System.Xml;


namespace Seringa.Engine.Utils
{
    public static class XmlHelpers
    {
        public static bool CreateOrLoadMappingFile(string mappingFile,IInjectionStrategy injectionStrategy,
                                                    string dbmsName, ref string error,out XDocument doc)
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
                catch(Exception ex)
                {
                    //TODO: do something
                }
            }

            try
            {
                if (document == null || (document != null && document.Element("map") == null))
                {
                    //create xml document from scratch
                    document = new XDocument(
                        new XElement("map",

                            new XElement("vulnerable-url", injectionStrategy.Url),
                            new XElement("injection-strategy",
                                
                                    new XAttribute("name",injectionStrategy.GetType().Name),
                                    new XElement("columns",
                                        new List<XElement>() 
                                        {
                                            new XElement("originalquery",injectionStrategy.NrColumnsInOriginalQuery),
                                            new XElement("resultinghtml",injectionStrategy.NumberOfResultsPerRequest),
                                            new XElement("indexes",
                                                ListHelpers.ListToCommaSeparatedValues(injectionStrategy.ColumnIndexes)),
                                        })),
                                
                            new XElement("dbms",new XAttribute("name",dbmsName),
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

            doc = document;
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

        public static void ChangeMappingFileElementValue(string mappingFile, string elementXpath, string discoveredValue,
                                                            IInjectionStrategy injectionStrategy, string dbmsName)
        {
            string error = string.Empty;
            //XDocument document = XDocument.Load(mappingFile);
            XDocument document = null;
            if(!CreateOrLoadMappingFile(mappingFile, injectionStrategy, dbmsName, ref error,out document))
                return;//TODO: write message to UI
            bool save = true;

            var element = document.XPathSelectElement(elementXpath);

            if (element != null)
                element.Value = discoveredValue;
            else
            {
                element = document.XPathSelectElement(elementXpath.Substring(0, elementXpath.LastIndexOf("/")));
                if (element != null)
                {
                    int last = elementXpath.LastIndexOf("/");
                    element.Add(new XElement(elementXpath.Substring(last, elementXpath.Length - last),discoveredValue));
                }
                else
                    save = false;
            }
            if (save)
                document.Save(mappingFile);
        }

        public static void ChangeMappingFileAttributeValue(string mappingFile, string elementXpath,string attributeName, string discoveredValue,
                                                            IInjectionStrategy injectionStrategy, string dbmsName)
        {
            XDocument document = null;
            string error = string.Empty;
            if (!CreateOrLoadMappingFile(mappingFile, injectionStrategy, dbmsName, ref error, out document))
                return;//TODO: write message to UI
            bool save = true;

            var element = document.XPathSelectElement(elementXpath);

            if (element != null)
            {
                var attribute = element.Attribute(attributeName);
                if (attribute != null)
                    attribute.Value = discoveredValue;
                else
                    element.Add(new XAttribute(attributeName, discoveredValue));
            }
            else
            {
                element = document.XPathSelectElement(elementXpath.Substring(0,elementXpath.LastIndexOf("/")));
                if (element != null)
                {
                    int last = elementXpath.LastIndexOf("/");
                    element.Add(new XElement(elementXpath.Substring(last, elementXpath.Length - last), new XAttribute(attributeName, discoveredValue)));
                }
                else
                    save = false;
            }
            if(save)
                document.Save(mappingFile);
        }

        public static bool SaveToMappingFile(string mappingFile,PayloadDetails payloadDetails,string discoveredValue, IInjectionStrategy strategy,
                                                string dbmsName)
        {
            if (string.IsNullOrEmpty(payloadDetails.NodeToMapTo))
                return false;

            XDocument document = null;
            string error = string.Empty;
            if (!CreateOrLoadMappingFile(mappingFile, strategy, dbmsName, ref error, out document))
                return false;//TODO: write message to UI

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
                return true;
            }
            return false;
        }

        public static XElement GetXmlElementViaXpath(string docName, string xpath)
        {
            XElement result = null;
            XDocument doc = XDocument.Load(docName);
            result = GetXmlElementViaXpath(doc, xpath);
            return result;
        }

        public static XElement GetXmlElementViaXpath(XElement elem, string xpath)
        {
            XElement result = null;
            result = elem.XPathSelectElements(xpath).FirstOrDefault();
            return result;
        }

        public static XElement GetXmlElementViaXpath(XDocument doc, string xpath)
        {
            return GetXmlElementViaXpath(doc.Root, xpath);
        }

        public static T GetObjectFromXml<T>(string docName, string elementType, string elementUserFriendlyName)
        {
            var doc = XDocument.Load(docName);

            var elem = doc.Descendants(elementType)
                            .SingleOrDefault(e => e.Attribute("user-friendly-name").Value == elementUserFriendlyName);

            T createdObj = default(T);

            if(elem != null)
                createdObj = (T)Activator.CreateInstance(typeof(T), elem);

            return createdObj;
        }

        public static T GetObjectFromXml<T>(string docName, string elementType, int index)
        {
            var doc = XDocument.Load(docName);

            var elem = doc.Descendants(elementType).ElementAt(index);

            T createdObj = (T)Activator.CreateInstance(typeof(T), elem);

            return createdObj;
        }

        public static IList<T> GetObjectsFromXml<T>(string docName, string elementType,IList<dynamic> filters)
        {
            IList<T> results = new List<T>();
            var doc = XDocument.Load(docName);

            var query = doc.Descendants(elementType).AsQueryable();

            if(filters != null && filters.Count > 0)
                foreach (var filter in filters)
                {
                    if (filter != null && filter.AttributeName != null && filter.AttributeValue != null)
                    {
                        var dict = (IDictionary<string, object>)filter;

                        query = query.Where(e =>
                            e.Attribute(dict["AttributeName"].ToString()) != null &&
                            (e.Attribute(dict["AttributeName"].ToString()).Value == dict["AttributeValue"].ToString())).AsQueryable();
                    }
                }

            foreach (var elem in query.ToList())
            {
                results.Add((T)Activator.CreateInstance(typeof(T), elem));
            }

            return results;
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


        public static T GetElementValueViaXpath<T>(XElement elem, string xpath, T defaultValue)
        {
            T result = defaultValue;

            XElement element =  GetXmlElementViaXpath(elem,xpath);

            if (element != null)
            {
                string value = element.Value;
                result = (T)Convert.ChangeType(value, typeof(T));
            }
            return result;
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

        public static T GetAttributeValueFromDoc<T>(string docName, string elemXpath, string attributeName, T defaultValue)
        {
            T result = defaultValue;
            XElement injectionStrategyElem = null;
            XAttribute injectionTypeNameAttr = null;

            if ((injectionStrategyElem = GetXmlElementViaXpath(docName, elemXpath)) != null)
                if ((injectionTypeNameAttr = injectionStrategyElem.Attribute(attributeName)) != null)
                    result = (T)Convert.ChangeType(injectionTypeNameAttr.Value, typeof(T));

            return result;
        }

        public static T GetElementValueFromDoc<T>(string docName, string elemXpath, T defaultValue)
        {
            T result = defaultValue;
            XElement injectionStrategyElem = null;

            if ((injectionStrategyElem = GetXmlElementViaXpath(docName, elemXpath)) != null)
                result = (T)Convert.ChangeType(injectionStrategyElem.Value, typeof(T));

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

        public static string GetLastTagFromXpath(string xpath)
        {
            string result = string.Empty;

            int lastSlash = xpath.LastIndexOf("/")+1;
            int lastParanthesis = xpath.LastIndexOf("[");

            if (lastSlash > -1)
            {
                int length = ((lastParanthesis > -1)?lastParanthesis:xpath.Length) - lastSlash;

                result = xpath.Substring(lastSlash, length);
            }
            return result;
        }
    }
}
