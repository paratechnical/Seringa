using System;
using System.IO;
using System.Text;

namespace Seringa.Engine.Implementations.Loggers
{
    class ExceptionLogger
    {
        /// <summary>
        /// Method to log a exception into a textfile.
        /// </summary>
        /// <typeparam name="T">Exception Type</typeparam>
        /// <param name="fileLocation">File that will be writen</param>
        /// <param name="exception">Exception to be logged</param>
        public static void Write<T>(String fileLocation, T exception)
            where T : Exception
        {
            try
            {
                if (!File.Exists(fileLocation))
                    File.Create(fileLocation);

                using (FileStream fileStream = File.Open(fileLocation, FileMode.Open, FileAccess.Write))
                {
                   var message = Encoding.UTF8.GetBytes(
                                    "[" + exception.Source + "] " + exception.Message  + "\n\n" 
                                     + exception.StackTrace + "\n\n\n");

                   fileStream.Write(message, 0, message.Length);
                }
            }
            catch (Exception ex)
            {
                // Better you decide....
            }
        }
    }
}
