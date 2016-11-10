using System;
using System.IO;

namespace requestPeso
{
    static class Logs
    {
        const string _pathToLog = @"C:\Bilance\logsBilance.txt";
        const string _directory = @"C:\Bilance";

        /// <summary>
        /// Scrive su file gli errori generati
        /// </summary>
        /// <param name="ex">Oggetto che ha scatenato l'eccezione</param>
        public static void WriteLine(Exception ex)
        {
            StreamWriter sw = null;

            try
            {
                if (!Directory.Exists(_directory))
                    Directory.CreateDirectory(_directory);

                sw = File.AppendText(_pathToLog);
                string dt = String.Format("{0:dd-MM-yyyy HH:mm:ss}", DateTime.Now);
                sw.WriteLine(String.Format("{0} - Exception: {1} - {2} \r\n{3}", dt, ex.Source.ToString(), ex.Message, ex.StackTrace));
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
                if (!Directory.Exists(_directory))
                    Directory.CreateDirectory(_directory);

                sw = File.AppendText(_pathToLog);
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
                if (!Directory.Exists(_directory))
                    Directory.CreateDirectory(_directory);

                sw = File.AppendText(_pathToLog);
                sw.WriteLine();
                sw.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Controlla se è passato un mese dal primo log scritto
        /// </summary>
        /// <returns>True se è passato un mese altrimenti false</returns>
        private static bool getFirstMonthLogged()
        {
            StreamReader sr = null;

            try
            {
                if (File.Exists(_pathToLog))
                {
                    var size = new FileInfo(_pathToLog).Length;
                    bool ret = false;

                    if (size > 0)
                    {
                        sr = new StreamReader(_pathToLog);
                        bool stop = false;
                        DateTime dt = new DateTime();

                        while (!stop)
                        {
                            if (DateTime.TryParse(sr.ReadLine().Split(' ')[0], out dt))
                                stop = true;
                        }

                        if (stop)
                            if (DateTime.Now.Subtract(dt).Days > 30)
                                ret = true;
                    }

                    return ret;
                }
                else
                    return false;
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// Svuota il file di log se passato un mese
        /// </summary>
        private static void resetFile()
        {
            try
            {
                if (getFirstMonthLogged())
                    File.WriteAllText(_pathToLog, string.Empty);
            }
            catch
            {
            }
        }
    }
}
