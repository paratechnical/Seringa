using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Interfaces;
using Seringa.Engine.Utils;

namespace Seringa.Engine.Implementations.Proxy
{
    public class ProxyDetails : IProxyDetails
    {
        public ProxyType ProxyType { get; set; }
        /// <summary>
        /// adress and port
        /// </summary>
        public string FullProxyAddress { get; set; }
        public string ProxyAddress { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUserName { get; set; }
        public string ProxyPassword { get; set; }
    }
}
