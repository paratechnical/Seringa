using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.DataObjects;

namespace Seringa.Engine.Interfaces
{
    public interface IInjectionStrategy : IWebOperation
    {
        string Url {get; set;}
        string DisplayName { get;}

        string SelectedDb { get; set; }
        string SelectedTable { get; set; }

        bool TestIfVulnerable();

        bool DetailedExceptions { get; set; }

        string MappingFile { get; set; }

        IList<int> ColumnIndexes { get; set; }
        int NrColumnsInOriginalQuery { get; set; }
        int NumberOfResultsPerRequest { get; set; }

        int GetTotalNoOfCustomQueryResultRows();
        string GetSingleCustomQueryResultRow(int startingFrom);

        ExploitDetails ExploitDetails { get; set; }
        PayloadDetails PayloadDetails { get; set; }
    }
}
