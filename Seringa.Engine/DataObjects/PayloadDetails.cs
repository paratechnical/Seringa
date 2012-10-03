using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Enums;
using Seringa.Engine.Utils;
using System.Xml.Linq;

namespace Seringa.Engine.DataObjects
{
    public class PayloadDetails
    {
        public string Name { get; set; }
        public string UserFriendlyName { get; set; }
        public ExpectedResultType ExpectedResultType { get; set; }
        public string Payload { get; set; }
        public List<PayloadParam> Params { get; set; }
        public string Dbms { get; set; }
        public string ParentNodeToMapTo { get; set; }
        public string NodeToMapTo { get; set; }
        public string MapToParams { get; set; }
        public string AttributeToMapTo { get; set; }

        public PayloadDetails()
        {
        }

        public PayloadDetails(XElement payload)
        {
            ParentNodeToMapTo = XmlHelpers.GetAttributeValue<string>(payload, "map-to-parent", string.Empty);
            NodeToMapTo = XmlHelpers.GetAttributeValue<string>(payload, "map-to-node", string.Empty);
            MapToParams = XmlHelpers.GetAttributeValue<string>(payload, "map-to-params", string.Empty);
            AttributeToMapTo = XmlHelpers.GetAttributeValue<string>(payload, "map-to-attribute", string.Empty);

            Dbms = XmlHelpers.GetAttributeValue<string>(payload, "dbms", string.Empty);
            Name = XmlHelpers.GetAttributeValue(payload, "name");
            UserFriendlyName = XmlHelpers.GetAttributeValue(payload, "user-friendly-name");
            ExpectedResultType = (ExpectedResultType)Enum.Parse(typeof(ExpectedResultType), 
                                            XmlHelpers.GetAttributeValue(payload, "expected-result-type"),true);
            Payload = payload.Descendants("value").SingleOrDefault().Value;
            Params = new List<PayloadParam>();
            var paramElems = payload.Descendants("params").Descendants("param");
            foreach (var param in paramElems)
            {
                Params.Add(new PayloadParam() { Name=XmlHelpers.GetAttributeValue(param, "name"),
                                                Position = XmlHelpers.GetAttributeValue<int>(param, "position", 0)
                });
            }

        }
    }
}
