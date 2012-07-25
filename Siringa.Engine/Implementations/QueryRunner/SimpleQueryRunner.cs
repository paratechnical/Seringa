using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siringa.Engine.Interfaces;
using System.Net;
using System.IO;

namespace Siringa.Engine.Implementations.QueryRunner
{
    public class SimpleQueryRunner : IQueryRunner
    {
        private string _userAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; en)";

        public string GetPageHtml(string url)
        {
            string result = string.Empty;
            result = GetRequestResult(url);
            return result;
        }

        private string GetRequestResult(string url)
        {
            string result  = string.Empty;

            HttpWebResponse resp = null;
            HttpWebRequest TestGet = (HttpWebRequest) WebRequest.Create(url);
			TestGet.Method = "GET";
            TestGet.UserAgent = _userAgent;

            try
            {
                resp = (HttpWebResponse)TestGet.GetResponse();
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
                    resp = (HttpWebResponse)wex.Response;
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
