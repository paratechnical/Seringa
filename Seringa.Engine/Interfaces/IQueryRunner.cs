using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Utils;

namespace Seringa.Engine.Interfaces
{
    public interface IQueryRunner
    {
        string GetPageHtml(string url);
        string GetPageHtml(string url, IProxyDetails proxyDetails);
    }
}
