using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Seringa.Engine.Static;

namespace Seringa.Engine.Utils
{
    public static class UrlHelpers
    {
        //static HeadClient Client { get; set; }

        static UrlHelpers()
        {
            //Client = new HeadClient();
        }

        public static string HexEncodeValue(string value)
        {
            string hexEncoded = string.Empty;

            hexEncoded = String.Join("", value.Select(c => String.Format("{0:X}", Convert.ToInt32(c))));

            return "0x" + hexEncoded;
        }

        public static string GeneratePossibleVulnerableUrl(string url)
        {
            return url + GeneralPayloads.UrlVulnerabilityTestingAppendix;
        }

        public static bool ValidUrl(string url)
        {
            Uri myUri;
            return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out myUri);

            /*
            try
            {
                Client.DownloadString(url);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
            */
        }
    }

    /*
    class HeadClient : WebClient
    {

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest req = base.GetWebRequest(address);
            req.Method = "GET";
            //req.Method = "HEAD";
            return req;
        }
    }
    */
}
