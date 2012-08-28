using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seringa.Engine.Interfaces
{
    public interface IWebOperation
    {
        IProxyDetails ProxyDetails { get; set; }
        IQueryRunner QueryRunner { get; set; }
        bool UseProxy { get; set; }
    }
}
