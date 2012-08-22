using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Siringa.Engine.Utils;

namespace Siringa.Engine.Interfaces
{
    public interface IQueryRunner
    {
        string GetPageHtml(string url);
        string GetPageHtml(string url, IProxyDetails proxyDetails);
    }
}
