using System;
using System.IO;

namespace requestPeso
{
    static class Logs
    {
        const string _pathToLog = "C:\\logRequestPeso.txt";

        /// <summary>
        /// Scrive su file gli errori generati
        /// </summary>
        /// <param name="ex">Oggetto che ha scatenato l'eccezione</param>
        public static void WriteLine(Exception ex)
        {
            StreamWriter sw = null;

            try
            {
                //Old: AppDomain.CurrentDomain.BaseDirectory + "LogPeso.txt"
                sw = new StreamWriter(_pathToLog, true);
                string dt = String.Format("{0:dd-MM-yyyy HH:mm:ss}", DateTime.Now);
                sw.WriteLine(String.Format("{0} - Exception: {1} - {2}", dt, ex.Source.ToString(), ex.Message));
                sw.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Scrive su file i log interessanti per l'utente
        /// </summary>
        /// <param name="message">Messaggio da salvare</param>
        public static void WriteLine(string message)
        {
            StreamWriter sw = null;

            try
            {
                sw = new StreamWriter(_pathToLog, true);
                string dt = String.Format("{0:dd-MM-yyyy HH:mm:ss}", DateTime.Now);
                sw.WriteLine(String.Format("{0} - {1}", dt, message));
                sw.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// NewLine
        /// </summary>
        public static void WriteLine()
        {
            StreamWriter sw = null;

            try
            {
                //Old: AppDomain.CurrentDomain.BaseDirectory + "LogPeso.txt"
                sw = new StreamWriter(_pathToLog, true);
                sw.WriteLine();
                sw.Close();
            }
            catch
            {
            }
        }
    }
}
