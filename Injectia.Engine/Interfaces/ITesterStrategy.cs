using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Injectia.Engine.Interfaces
{
    interface ITesterStrategy
    {
        IList<string> GetInjectableUrls(IList<string> urls);
    }
}
