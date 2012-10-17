using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seringa.Engine.Interfaces
{
    public interface IIPObtainerStrategy : IWebOperation
    {

        string GetIp(ref string error);
    }
}
