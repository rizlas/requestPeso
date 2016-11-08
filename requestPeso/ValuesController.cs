using System;
using System.Web.Http;
using SerialPortListener.Serial;
using System.Threading;
using System.Text;

namespace requestPeso
{
    public class ValuesController : ApiController
    {
        SerialPortManager _spManager = null;
        SerialSettings _mySerialSettings = null;

        const int _timeOut = 15000;
        const int _baudRate = 9600;
        const int _nBits = 8;
        const int _tick = 120000;
        const int _lengthPesata = 8;

        string _portName = "COMX";
        string _pesata;
        string _user;

        System.Timers.Timer _keepAlive;

        public ValuesController()
        {
            _portName = Scheduler.PortName;
            inizializzaSeriale();

            _keepAlive = new System.Timers.Timer(5000);
            _keepAlive.Elapsed += Tick;
            _keepAlive.Start();            
        }

        private void Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(_spManager != null)
            {
                _spManager.StartListening();
                string tmp = requestToSerial();
                _pesata = string.Empty;
                _spManager.StopListening();
            }
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
                _keepAlive.Stop();

                string tmp = "";
                _user = user;

                if (command == "$")
                {
                    _spManager.StartListening();

                    tmp = requestToSerial();

                    _pesata = "";
                    _spManager.StopListening();

                    if (tmp.Trim() == "0.0000")
                        tmp = "-1";
                }

                _keepAlive.Start();
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
                _mySerialSettings = _spManager.CurrentSerialSettings;

                _mySerialSettings.BaudRate = _baudRate;
                _mySerialSettings.PortName = _portName;
                _mySerialSettings.Parity = System.IO.Ports.Parity.None;
                _mySerialSettings.StopBits = System.IO.Ports.StopBits.One;
                _mySerialSettings.DataBits = _nBits;

                GC.SuppressFinalize(_spManager);
                _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);
            }
            catch (Exception ex)
            {
                Logs.WriteLine(ex);
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

                if(!_keepAlive.Enabled)
                    Logs.WriteLine("Ricevuta richiesta da: " + _user);
                
                _spManager.SendData("$");

                //Ferma l'esecuzione del thread e attende la lettura completa del dato dalla seriale.
                //Se la condizione non viene soddisfatta allo scadere del timeOut l'esecuzione andrà avanti.
                bool notTimeout = SpinWait.SpinUntil(() => _pesata.Length == _lengthPesata, _timeOut);

                if (!_keepAlive.Enabled)
                {
                    Logs.WriteLine("Pesata length: " + _pesata.Length + "  " + _pesata);

                    if (notTimeout)
                    {
                        Logs.WriteLine("Peso letto: " + _pesata);
                    }
                    else
                    {
                        Logs.WriteLine("TIMEOUT");
                        _pesata = "-1";
                    }
                }

                return _pesata;
            }
            catch (Exception ex)
            {
                Logs.WriteLine(ex);
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

        /// <summary>
        /// Rilascia le risorse utilizzate dall'oggetto seriale
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();

            if(_spManager != null)
                _spManager.StopListening();

            if(_spManager != null)
                _spManager.Dispose();
        }
    }
}
