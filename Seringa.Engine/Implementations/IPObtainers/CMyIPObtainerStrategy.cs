using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Interfaces;
using Seringa.Engine.Implementations.QueryRunners;

namespace Seringa.Engine.Implementations.IPObtainers
{
    public class CMyIPObtainerStrategy : IIPObtainerStrategy
    {
        #region Private

        #region Bounds

        private const string _ipResultLowerBound = "<title>My IP Address Is  ";
        private const string _ipResultUpperBound = "   - Quick and Easy way to SEE my IP address - CmyIP.com</title>";
        #endregion Bounds

        #endregion Private

        #region Properties
        public IProxyDetails ProxyDetails { get; set; }
        public IQueryRunner QueryRunner { get; set; }
        public bool UseProxy { get; set; }
        #endregion Properties

        #region Constructor

        public CMyIPObtainerStrategy()
        {
            QueryRunner = new SimpleQueryRunner();
        }

        #endregion Constructor

        public string GetIp()
        {
            string result = string.Empty;

            var pageHtml = QueryRunner.GetPageHtml("http://cmyip.com/", UseProxy ? ProxyDetails : null);
            result = GetAnswerFromHtml(pageHtml);

            return result;
        }

        #region Methods
        private string GetAnswerFromHtml(string html)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(html))
            {
                result = html.Substring(html.IndexOf(_ipResultLowerBound) +
                                            _ipResultLowerBound.Length,
                                            html.IndexOf(_ipResultUpperBound) - html.IndexOf(_ipResultLowerBound) -
                                            _ipResultLowerBound.Length);
            }
            return result;
        }
                
        #endregion Methods
    }
}
