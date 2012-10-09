using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Seringa.Engine.Utils;

namespace Seringa.Engine.DataObjects
{
    public class PatternDetails
    {

        public string Dbms { get; set; }
        public string Value { get; set; }

        public PatternDetails()
        {
        }

        public PatternDetails(XElement pattern)
        {
            Dbms = XmlHelpers.GetAttributeValue<string>(pattern, "dbms", string.Empty);
            Value = pattern.Value;
        }

    }
}
