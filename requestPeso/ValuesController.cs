using System;
using System.Web.Http;
using SerialPortListener.Serial;
using System.IO;
using System.Threading;
using System.Text;

namespace requestPeso
{
    public class ValuesController : ApiController
    {
        SerialPortManager _spManager = null;

        const int _timeOut = 15000;

        const int _baudRate = 9600;
        const int _nBits = 8;
        string _portName = "COMX";

        const int _lengthPesata = 8;
        string _pesata;

        const string _pathToCom = "C:\\requestoPeso.txt";

        string _user;

        public ValuesController()
        {
            _portName = Scheduler.PortName;
            inizializzaSeriale();
            //inizializzaThread();
        }

        /// <summary>
        /// Gestisce le richieste in entrata.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns></returns>
        public string Get(string command, string user)
        {
            if (_spManager != null)
            {
                string tmp = "";
                _user = user;

                if (command == "$")
                {
                    _spManager.StartListening();

                    tmp = requestToSerial();

                    _pesata = "";
                    _spManager.StopListening();
                }

                return tmp.Trim();
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Imposta l'oggetto per leggere dalla seriale e fa partire un thread apposito per la lettura
        /// </summary>
        private void inizializzaSeriale()
        {
            try
            {
                _spManager = new SerialPortManager();
                SerialSettings mySerialSettings = _spManager.CurrentSerialSettings;

                mySerialSettings.BaudRate = _baudRate;
                mySerialSettings.PortName = _portName;
                mySerialSettings.Parity = System.IO.Ports.Parity.None;
                mySerialSettings.StopBits = System.IO.Ports.StopBits.One;
                mySerialSettings.DataBits = _nBits;

                _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);

                //_spManager.StartListening();
            }
            catch (Exception ex)
            {
                Logs.errorLogs(ex);
            }
        }

        /// <summary>
        /// Richiede i dati alla seriale quando arriva un messaggio contente il carattere $ al webserver in ascolto
        /// </summary>
        private string requestToSerial()
        {
            try
            {
                _pesata = "";

                Logs.errorLogs("Ricevuta richiesta da: " + _user);
                
                _spManager.SendData("$");

                //Ferma l'esecuzione del thread e attende la lettura completa del dato dalla seriale.
                //Se la condizione non viene soddisfatta allo scadere del timeOut l'esecuzione andrà avanti.
                bool notTimeout = SpinWait.SpinUntil(() => _pesata.Length == _lengthPesata, _timeOut);
                
                if (notTimeout)
                {
                    Logs.errorLogs("Peso letto: " + _pesata);
                }
                else
                {
                    Logs.errorLogs("TIMEOUT");
                    _pesata = "";
                }

                return _pesata;
            }
            catch (Exception ex)
            {
                Logs.errorLogs(ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Triggerato quando ci sono nuovi dati da leggere. La variabile pesata viene concatenata fino a quando ci sono dati sulla seriale da leggere.
        /// </summary>
        /// <param name="e">Oggetto che contiene i dati</param>
        private void _spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {
            string data = Encoding.ASCII.GetString(e.Data);
            _pesata += data;

            //int dataInCoda = _spManager.SerialPort.BytesToRead;
        }

        public new void Dispose()
        {
            base.Dispose();

            if(_spManager != null)
                _spManager.Dispose();
        }

        #region Deprecated

        private void inizializzaThread()
        {
            //Thread threadOnStart = new Thread(requestToSerial);
            //threadOnStart.Name = "threadConnection";
            //threadOnStart.IsBackground = false;
            //threadOnStart.Start();
        }

        /// <summary>
        /// Legge la porta seriale da usare (COM X)
        /// </summary>
        private void getComToUse()
        {
            StreamReader sr = null;

            try
            {
                sr = new StreamReader(_pathToCom);
                _portName = sr.ReadLine();
                sr.Close();
            }
            catch (Exception ex)
            {
                Logs.errorLogs(ex);
            }
        }

        #endregion

    }
}
