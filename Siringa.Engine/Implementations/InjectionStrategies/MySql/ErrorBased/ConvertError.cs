using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siringa.Engine.Interfaces;
using Siringa.Engine.Implementations.QueryRunner;

namespace Siringa.Engine.Implementations.InjectionStrategies.MySql.ErrorBased
{
    public class ConvertError : IInjectionStrategy
    {
        #region Constructors
        public ConvertError()
        {
            QueryRunner = new SimpleQueryRunner();
        }
        #endregion Constructors

        #region Public

        public IQueryRunner QueryRunner { get; set; }

        public string DbVulnerableVersionFrom
        {
            get
            {
                return "-";
            }
        }
        public string DbVulnerableVersionTo
        {
            get
            {
                return "-";
            }
        }

        public string Url { get; set; }

        public string SelectedDb { get; set; }
        public string SelectedTable { get; set; }

        public string DisplayName { get { return "Mysql convert method"; } }

        public bool TestIfVulnerable()
        {
            return !string.IsNullOrEmpty(GetDbVersion());
        }

        public int GetTotalNoOfTables()
        {
            return 0;
        }

        public int GetTotalNoOfColumns()
        {
            return 0;
        }

        public string GetDbVersion()
        {
            string result = string.Empty;
            //@TODO: actually implement
            return result;
        }

        public string GetDbUserName()
        {
            string result = string.Empty;
            //@TODO: actually implement
            return result;
        }
        public string GetCurrentDbName()
        {
            string result = string.Empty;
            //@TODO: actually implement
            return result;
        }

        public string GetSingleDatabaseName(int startingFrom)
        {
            string result = string.Empty;
            //@TODO: actually implement
            return result;
        }
        public string GetSingleTableName(int startingFrom)
        {
            string result = string.Empty;
            //@TODO: actually implement
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

        public int GetTotalNoOfDbs()
        {
            return 0;
        }

        #endregion Public


    }
}
