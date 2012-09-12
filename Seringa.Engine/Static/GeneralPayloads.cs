using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seringa.Engine.Static
{
    public class GeneralPayloads
    {
        public static string ErrorBasedVictimIdentifier = "SELECT 'victim'";
        public static string ErrorBasedVictimConfirmationResult = "victim";
        public static string QueryResultCount = "SELECT count(*) FROM ({0}) cq";
        public static string MysqlSingleResultLimiterQuery = "{0} LIMIT {1},1";
        public static string MssqlSingleResultLimiterQuery = "";
    }
}
