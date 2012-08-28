using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Seringa.Engine.Utils
{
    public static class QueryHelper
    {
        public static string CreateQuery(string url,string exploit, string payload)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(url);
            sb.Append(HttpUtility.UrlPathEncode(string.Format(exploit, payload)));
            return sb.ToString();
        }
    }
}
