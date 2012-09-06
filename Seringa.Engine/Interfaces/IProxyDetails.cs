using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Utils;
using Seringa.Engine.Enums;

namespace Seringa.Engine.Interfaces
{
    public interface IProxyDetails
    {
        ProxyType ProxyType { get; set; }
        /// <summary>
        /// adress and port
        /// </summary>
        string FullProxyAddress { get; set; }
        string ProxyAddress { get; set; }
        int ProxyPort { get; set; }
        string ProxyUserName { get; set; }
        string ProxyPassword { get; set; }
    }
}
