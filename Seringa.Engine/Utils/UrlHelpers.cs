using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Seringa.Engine.Static;
using System.Web;
using System.Collections.Specialized;
using Seringa.Engine.Utils.Extensions;

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
            UriBuilder builder = null;
            Uri uri = new Uri(url);
            var parameters = HttpUtility.ParseQueryString(uri.Query);
            if (parameters.Count > 0)//no obvious parameters
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[parameters.Keys[i]] = parameters[i] + GeneralPayloads.UrlVulnerabilityTestingAppendix;
                    builder = new UriBuilder(uri.Scheme + "://" + uri.Host + uri.AbsolutePath);
                    builder.Query = parameters.ToString();
                    results.Add(builder.ToString());
                }
            }
            else//might be overrider url so try parsing segments
            {
                StringBuilder sb = new StringBuilder();
                for(int i=0;i<uri.Segments.Count();i++)// var segment in uri.Segments)
                {
                    sb.Clear();
                    sb.Append(uri.Scheme + "://" + uri.Host + "/");
                    for(int j=0;j<i;j++)
                        if(!string.IsNullOrEmpty(uri.Segments[j]) && uri.Segments[j] != "/")
                            sb.Append(uri.Segments[j].RemoveLastSlash() + "/");
                    if (!string.IsNullOrEmpty(uri.Segments[i]) && uri.Segments[i] != "/")
                        sb.Append(uri.Segments[i].RemoveLastSlash() + GeneralPayloads.UrlVulnerabilityTestingAppendix + "/");
                    else
                        continue;
                    for (int h = i+1; h < uri.Segments.Count(); h++)
                        if (!string.IsNullOrEmpty(uri.Segments[h]) && uri.Segments[h] != "/")
                            sb.Append(uri.Segments[h].RemoveLastSlash() + "/");
                    results.Add(sb.ToString());
                }
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
