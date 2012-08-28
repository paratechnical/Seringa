using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seringa.Engine.Interfaces
{
    public interface IInjectionStrategy : IWebOperation
    {
        
        string DbVulnerableVersionFrom { get;}
        string DbVulnerableVersionTo { get;}

        string Url {get; set;}
        string DisplayName { get;}

        string SelectedDb { get; set; }
        string SelectedTable { get; set; }

        bool TestIfVulnerable();

        string GetDbVersion();
        string GetDbUserName();
        string GetCurrentDbName();

        int GetTotalNoOfDbs();
        int GetTotalNoOfTables();
        int GetTotalNoOfColumns();

        string GetSingleDatabaseName(int startingFrom);
        string GetSingleTableName(int startingFrom);
        string GetSingleTableColumnName(int startingFrom);

        string CustomQuery { get; set; }

        int GetTotalNoOfCustomQueryResultRows();
        string GetSingleCustomQueryResultRow(int startingFrom);
    }
}
