using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.DataObjects;

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

        bool DetailedExceptions { get; set; }
        string CustomQuery { get; set; }

        int GetTotalNoOfCustomQueryResultRows();
        string GetSingleCustomQueryResultRow(int startingFrom);

        ExploitDetails Exploit { get; set; }
        PayloadDetails Payload { get; set; }
    }
}
