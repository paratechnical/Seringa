using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Siringa.Engine.Utils
{
    public static class UrlHelper
    {
        //static HeadClient Client { get; set; }

        static UrlHelper()
        {
            //Client = new HeadClient();
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
