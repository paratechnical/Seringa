using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siringa.Engine.Interfaces;
using System.Net;
using System.IO;
using Siringa.Engine.Utils;

namespace Siringa.Engine.Implementations.QueryRunner
{
    public class SimpleQueryRunner : IQueryRunner
    {
        private string _userAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; en)";

        public string GetPageHtml(string url,IProxyDetails proxyDetails)
        {
            string result = string.Empty;
            result = GetRequestResult(url, proxyDetails);
            return result;
        }

        public string GetPageHtml(string url)
        {
            string result = string.Empty;
            result = GetRequestResult(url,null);
            return result;
        }

        private WebRequest CreateProperRequestType(string url,IProxyDetails proxyDetails)
        {
            WebRequest result = null;

            if (proxyDetails.ProxyType == ProxyType.Proxy)
            {
                result = (HttpWebRequest)WebRequest.Create(url);
                result.Proxy = new WebProxy(proxyDetails.FullProxyAddress);
                ((HttpWebRequest)result).UserAgent = _userAgent;
            }
            else if (proxyDetails.ProxyType == ProxyType.Socks)
            {
                result = SocksHttpWebRequest.Create(url);
                result.Proxy = new WebProxy(proxyDetails.FullProxyAddress);//TODO: implement user and password
                //((SocksHttpWebRequest)result).UserAgent = _userAgent;
            }
            else if (proxyDetails.ProxyType == ProxyType.None)
            {
                result = (HttpWebRequest)WebRequest.Create(url);
                ((HttpWebRequest)result).UserAgent = _userAgent;
            }

            return result;
        }

        private string GetRequestResult(string url, IProxyDetails proxyDetails)
        {
            string result  = string.Empty;

            WebResponse resp = null;
            WebRequest TestGet = CreateProperRequestType(url, proxyDetails);
			TestGet.Method = "GET";

            try
            {
                resp = TestGet.GetResponse();
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ReceiveFailure)
                {
                    //@TODO:actually do something to correct this error
                    //@TODO:actually send error to gui
                }
                else if (wex.Status == WebExceptionStatus.Timeout)
                {
                    // Try again I guess.. 
                    //@TODO:actually do something to correct this error
                    //@TODO:actually send error to gui
                }
                else if (wex.Status == WebExceptionStatus.ProtocolError)
                {
                    resp = wex.Response;
                }
                else
                {
                    //@TODO:actually send error to gui
                    //ParentOutput(wex.ToString());                    
                }
            }

            if (resp != null)
            {
                // Get the stream associated with the response.
                Stream receiveStream = resp.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                result = readStream.ReadToEnd();
                resp.Close();
                readStream.Close();
            }

            return result;
        }
    }
}
