using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seringa.Engine.Interfaces
{
    public interface IScannerStrategy
    {
        IList<string> GetUrls();
    }
}
