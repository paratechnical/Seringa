using System;
using System.IO;
using System.Text;

namespace Seringa.Engine.Implementations.Loggers
{
    public class ExceptionLogger
    {
        /// <summary>
        /// Method to log a exception into a textfile.
        /// </summary>
        /// <param name="fileLocation">File that will be writen</param>
        /// <param name="exception">Exception to be logged</param>
        public static void Write(String fileLocation, Exception exception)
        {
            try
            {
                // If file do not exis, create it
                if (File.Exists(fileLocation) == false)
                    File.Create(fileLocation).Close();

                // Open the file to write the message
                using (FileStream fileStream = 
                    File.Open(fileLocation, FileMode.Append, FileAccess.Write))
                {
                    // Generate the message
                    var message = Encoding.UTF8.GetBytes(
                                    "[" + DateTime.Now + " : " + exception.Source + "] " 
                                     + exception.GetType() + " "
                                     + exception.Message 
                                     + exception.StackTrace
                                     + Environment.NewLine);

                    // Write the message down.
                    fileStream.Write(message, 0, message.Length);
                    fileStream.Close();
                }
            }
            catch (Exception ex)
            {
                // Better you decide....
            }
        }
    }
}
