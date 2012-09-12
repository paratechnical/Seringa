using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seringa.Engine.Utils
{
    public class FileHelpers
    {
        public static string GetCurrentDirectory()
        {
            var pathToRunningFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var pathToRunningFileDir = System.IO.Path.GetDirectoryName(pathToRunningFile);
            return pathToRunningFileDir;
        }
    }
}
