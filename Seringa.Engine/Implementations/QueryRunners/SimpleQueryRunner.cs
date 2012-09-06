using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Interfaces;
using System.Net;
using System.IO;
using Seringa.Engine.Utils;
using Seringa.Engine.Exceptions;
using Seringa.Engine.Enums;

namespace Seringa.Engine.Implementations.QueryRunners
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

            if (proxyDetails != null)
            {
                if (proxyDetails.ProxyType == ProxyType.Proxy)
                {
                    result = (HttpWebRequest)WebRequest.Create(url);
                    result.Proxy = new WebProxy(proxyDetails.FullProxyAddress);
                    ((HttpWebRequest)result).UserAgent = _userAgent;
                }
                else if (proxyDetails.ProxyType == ProxyType.Socks)
                {
                    result = SocksHttpWebRequest.Create(url);
                    result.Proxy = new WebProxy(proxyDetails.FullProxyAddress);
                    //TODO: implement user and password
                    
                }
                else if (proxyDetails.ProxyType == ProxyType.None)
                {
                    result = (HttpWebRequest)WebRequest.Create(url);
                    ((HttpWebRequest)result).UserAgent = _userAgent;
                }
            }
            else
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
            WebRequest getRequest = CreateProperRequestType(url, proxyDetails);
			getRequest.Method = "GET";

            try
            {
                resp = getRequest.GetResponse();
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ReceiveFailure)
                {
                    throw new HtmlObtainingException("Failed to receive html output while trying to obtain page result");                 
                }
                else if (wex.Status == WebExceptionStatus.Timeout)
                {
                    throw new HtmlObtainingException("Timeout occured while trying to obtain page result");                 
                }
                else if (wex.Status == WebExceptionStatus.ProtocolError)
                {
                    resp = wex.Response;
                    //when using Privoxy this is always triggered but I do get a page back from it
                    //the actual error is server returned 503
                    //throw new HtmlObtainingException("Protocol error while trying to obtain page result");                 
                }
                else
                {
                    throw new HtmlObtainingException("Unknown exception occured while trying to obtain page result");                 
                }
            }

            if (resp != null)
            {
                Encoding encoding = Encoding.Default;
                if(resp is HttpWebResponse)
                {
                    var enc = ((HttpWebResponse)resp).ContentEncoding;
                    if(!string.IsNullOrEmpty(enc))
                        encoding = Encoding.GetEncoding(enc);
                }
                else if (resp is SocksHttpWebResponse)
                {
                    encoding = ((SocksHttpWebResponse)resp).CorrectEncoding;
                }
                // Get the stream associated with the response.
                Stream receiveStream = resp.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, encoding);

                result = readStream.ReadToEnd();
                resp.Close();
                readStream.Close();
            }

            return result;
        }
    }
}
