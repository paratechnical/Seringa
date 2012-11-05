using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Seringa.Engine.Static;
using System.Web;
using System.Collections.Specialized;

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

        private static string ToQueryString(NameValueCollection nvc)
        {
            return "?" + string.Join("&", Array.ConvertAll(nvc.AllKeys, key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))));
        }

        public static IList<string> GeneratePossibleVulnerableUrls(string url)
        {
            IList<string> results = new List<string>();

            Uri uri = new Uri(url);
            var parameters = HttpUtility.ParseQueryString(uri.Query);
            for(int i = 0; i< parameters.Count;i++)
            {
                parameters[parameters.Keys[i]] = parameters[i] + GeneralPayloads.UrlVulnerabilityTestingAppendix;
                var builder = new UriBuilder(uri.Scheme + "://" + uri.LocalPath);
                builder.Query = parameters.ToString();
                results.Add(builder.ToString());
            }

            //return url + GeneralPayloads.UrlVulnerabilityTestingAppendix;
            return results;
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
