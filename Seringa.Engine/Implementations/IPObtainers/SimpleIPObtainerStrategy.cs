using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Interfaces;
using Seringa.Engine.Implementations.QueryRunners;
using Seringa.Engine.DataObjects;
using Seringa.Engine.Utils;

namespace Seringa.Engine.Implementations.IPObtainers
{
    public class SimpleIPObtainerStrategy : IIPObtainerStrategy
    {
        #region Private

        private IpObtainerDetails _details;

        #endregion Private

        #region Properties
        public IProxyDetails ProxyDetails { get; set; }
        public IQueryRunner QueryRunner { get; set; }
        public bool UseProxy { get; set; }
        #endregion Properties

        #region Constructor

        public SimpleIPObtainerStrategy()
        {
            QueryRunner = new SimpleQueryRunner();
             _details = XmlHelpers.GetObjectFromXml<IpObtainerDetails>(FileHelpers.GetCurrentDirectory() + "\\xml\\ipcheckers.xml","ipchecker",0);

        }

        #endregion Constructor

        public string GetIp(ref string error)
        {
            string result = string.Empty;
            string pageHtml = string.Empty;

            if (_details != null)
            {//@TODO: baga asta intr-un thread
                try
                {
                    pageHtml = QueryRunner.GetPageHtml(_details.Url, UseProxy ? ProxyDetails : null);
                }
                catch (Exception ex)
                {
                    error = "Can not obtain IP, error from ip obtainer website or erroneous bounds defined in configuration file";
                }
                result = GetAnswerFromHtml(pageHtml, ref error);
            }
            else
                error = "Can not obtain IP, missing or erroneous configuration(ipcheckers.xml)";

            return result;
        }

        #region Methods
        private string GetAnswerFromHtml(string html,ref string error)
        {
            string result = string.Empty;
            int startIndex = 0;
            int endIndex = 0;

            if (_details != null)
            {

                if (!string.IsNullOrEmpty(html))
                {
                    try
                    {
                        startIndex = html.IndexOf(_details.LowerBound) + _details.LowerBound.Length;
                        endIndex = html.IndexOf(_details.UpperBound, startIndex);

                        result = html.Substring(startIndex, endIndex - startIndex);
                    }
                    catch (Exception ex)//@TODO:log errors
                    {
                        error = "Can not obtain IP, error from ip obtainer website or erroneous bounds defined in configuration file";
                    }
                }
                else
                    error = "Can not obtain IP, error from ip obtainer website(choose another url in configuration file)";
            }
            else
                error = "Can not obtain IP, missing or erroneous configuration(ipcheckers.xml)";

            return result;
        }
                
        #endregion Methods
    }
}
