using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Static;
using Seringa.Engine.Interfaces;

namespace Seringa.Engine.Utils
{
    public static class PayloadHelpers
    {

        public static string GetData(string placeholder,IInjectionStrategy injectionStrategy)
        {
            switch (placeholder)
            {
                case "SelectedDb":
                    return injectionStrategy.SelectedDb;
                case "SelectedTable":
                    return injectionStrategy.SelectedTable;
                default:
                    return string.Empty;
            }
        }

        public static string GetSingleResultLimiter(string dbms)
        {
            switch (dbms)
            {
                case "mysql":
                    return GeneralPayloads.MysqlSingleResultLimiterQuery;
                    break;
                case "mssql":
                    return GeneralPayloads.MssqlSingleResultLimiterQuery;
                    break;
                default:
                    return GeneralPayloads.MysqlSingleResultLimiterQuery;//highest dbms probability is mysql
                    break;
            }
        }
    }
}
