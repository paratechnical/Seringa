using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Seringa.Engine.Utils;

namespace Seringa.Engine.DataObjects
{
    public class IpObtainerDetails
    {
        public string Url { get; set; }
        public string LowerBound { get; set; }
        public string UpperBound { get; set; }

        public IpObtainerDetails(XElement details)
        {
            Url = XmlHelpers.GetElementValueViaXpath<string>(details, "url", string.Empty);
            LowerBound = XmlHelpers.GetElementValueViaXpath<string>(details, "lowerbound", string.Empty);
            UpperBound = XmlHelpers.GetElementValueViaXpath<string>(details, "upperbound", string.Empty);
        }
    }
}
