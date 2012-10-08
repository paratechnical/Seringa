using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Exceptions;
using Seringa.Engine.DataObjects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;

namespace Seringa.Engine.Utils
{
    public class HtmlHelpers
    {

        public static List<string> GoogleSearch(string search_expression)
        {
            var url_template = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&rsz=large&safe=active&q={0}&start={1}";
            Uri search_url;
            var results_list = new List<string>();
            int[] offsets = { 0, 8, 16, 24, 32, 40, 48 };
            foreach (var offset in offsets)
            {
                search_url = new Uri(string.Format(url_template, search_expression, offset));
 
                var page = new WebClient().DownloadString(search_url);
 
                JObject o = (JObject)JsonConvert.DeserializeObject(page);
 
                var results_query =
                    from result in o["responseData"]["results"].Children()
                    select result.Value<string>("unescapedUrl").ToString();
 
                foreach (var result in results_query)
                    results_list.Add(result);
            }

            return results_list;
        }

        public static string GetAnswerFromHtml(string html, string query, ExploitDetails ExploitDetails, bool detailedExceptions)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(html))
            {
                try
                {
                    result = html.Substring(html.IndexOf(ExploitDetails.ResultStart) +
                                                ExploitDetails.ResultStart.Length,
                                                html.IndexOf(ExploitDetails.ResultEnd) - html.IndexOf(ExploitDetails.ResultStart) -
                                                ExploitDetails.ResultStart.Length);
                }
                catch
                {
                    string userFriendlyException = "Could not parse sql injection result.";

                    if (!string.IsNullOrEmpty(ExploitDetails.ErrorStart) && !string.IsNullOrEmpty(ExploitDetails.ErrorEnd))
                        if (html.IndexOf(ExploitDetails.ErrorStart) > -1 && html.IndexOf(ExploitDetails.ErrorEnd) > -1)
                            userFriendlyException = string.Format("Sql exception occured: {0}",
                                                        html.Substring(html.IndexOf(ExploitDetails.ErrorStart) +
                                                        ExploitDetails.ErrorStart.Length,
                                                        html.IndexOf(ExploitDetails.ErrorEnd) - html.IndexOf(ExploitDetails.ErrorStart) -
                                                        ExploitDetails.ErrorStart.Length));

                    if (detailedExceptions)
                        userFriendlyException = string.Format("{0}({1})", userFriendlyException, query);

                    throw new SqlInjException(userFriendlyException);
                }
            }

            if (ExploitDetails.TrimLast)
                result = result.Remove(result.Length - 1, 1);

            return result;
        }


        public static IList<string> GetMultipleAnswersFromHtml(string html, string query, ExploitDetails ExploitDetails, bool detailedExceptions)
        {
            IList<string> results = new List<string>();
            string result = string.Empty;

            if (!string.IsNullOrEmpty(html))
            {
                int resultStartIndex = 0; 
                int resultLength = 0;
                int resultEndIndex = 0;
                while (resultStartIndex != -1)
                {
                    try
                    {
                        resultStartIndex = html.IndexOf(ExploitDetails.ResultStart, resultEndIndex);
                        if (resultStartIndex == -1)
                            break;

                        resultStartIndex += ExploitDetails.ResultStart.Length;
                        resultEndIndex = html.IndexOf(ExploitDetails.ResultEnd, resultStartIndex);
                        resultLength = resultEndIndex - resultStartIndex;
                    }
                    catch
                    {
                        break;
                    }

                    try
                    {
                        result = html.Substring(resultStartIndex, resultLength);

                        if (ExploitDetails.TrimLast)
                            result = result.Remove(result.Length - 1, 1);

                        results.Add(result);
                    }
                    catch
                    {
                        string userFriendlyException = "Could not parse sql injection result.";

                        if (!string.IsNullOrEmpty(ExploitDetails.ErrorStart) && !string.IsNullOrEmpty(ExploitDetails.ErrorEnd))
                            if (html.IndexOf(ExploitDetails.ErrorStart) > -1 && html.IndexOf(ExploitDetails.ErrorEnd) > -1)
                                userFriendlyException = string.Format("Sql exception occured: {0}",
                                                            html.Substring(html.IndexOf(ExploitDetails.ErrorStart) +
                                                            ExploitDetails.ErrorStart.Length,
                                                            html.IndexOf(ExploitDetails.ErrorEnd) - html.IndexOf(ExploitDetails.ErrorStart) -
                                                            ExploitDetails.ErrorStart.Length));

                        if (detailedExceptions)
                            userFriendlyException = string.Format("{0}({1})", userFriendlyException, query);

                        throw new SqlInjException(userFriendlyException);
                    }
                }
            }
            
            return results;
        }

    }
}
