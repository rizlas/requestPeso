using System;
using System.ServiceProcess;
using System.IO;
using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Web.Http.Cors;
using System.IO.Ports;
using SerialPortListener.Serial;
using System.Text;

namespace requestPeso
{
    public partial class Scheduler : ServiceBase
    {
        ValuesController _vc = null;
        const string _configPath = @"C:\Bilance\config.ini";

        static string _portName;

        Thread _threadOnStart;

        public static string PortName
        {
            get
            {
                return _portName;
            }
        }

        public Scheduler()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (args.Length == 2)
                Start(args[1]);
            else if (File.Exists(_configPath))
            {
                getComToUse();
                Start(_portName);
            }
            else
            {
                Logs.WriteLine("Parametri errati o config.ini mancante, usare --port COMX per i parametri!");
                this.Stop();
            }
        }

        /// <summary>
        /// Inizializza la variabile _portName dal file C:\requestoPeso.txt e imposta le variabili per leggere dalla seriale
        /// Da mettere public per debug
        /// </summary>
        private void Start(string port)
        {
            _portName = port;
            bool _goAhead = false;

            string[] portExists = SerialPort.GetPortNames();
            foreach (string item in portExists)
            {
                if (item == _portName)
                    _goAhead = true;
            }

            if (_goAhead)
            {
                inizializzaThread();
                Logs.WriteLine("Servizio partito..........");
            }
            else
            {
                Logs.WriteLine("Porta non trovata, controlla file config.ini o i parametri (--port COMX)!");
                this.Stop();
            }
        }

        private void inizializzaThread()
        {
            _threadOnStart = new Thread(inizializzaServerWeb);
            _threadOnStart.Name = "threadServerWeb";
            _threadOnStart.IsBackground = false;
            _threadOnStart.Start();
        }

        private void inizializzaServerWeb()
        {
            try
            {
                _vc = new ValuesController();

                HttpSelfHostConfiguration config = new HttpSelfHostConfiguration("http://localhost:8080");
                
                config.Routes.MapHttpRoute( name: "DefaultApi",
                                            routeTemplate: "api/{controller}/{id}",
                                            defaults: new { id = RouteParameter.Optional });
                
                config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                HttpSelfHostServer server = new HttpSelfHostServer(config);
                server.OpenAsync().Wait();

                Logs.WriteLine("WEBSERVER CREATO!");

                inizializzaSeriale();
            }
            catch (Exception ex)
            {
                Logs.WriteLine(ex);
            }
        }
        
        /// <summary>
        /// Ferma il servizio
        /// </summary>
        protected override void OnStop()
        {
            if(_vc != null)
                _vc.Dispose();

            Logs.WriteLine("Servizio fermato..........");
        }

        /// <summary>
        /// Legge la porta seriale da usare (COM X)
        /// </summary>
        private void getComToUse()
        {
            StreamReader sr = null;

            try
            {
                sr = new StreamReader(_configPath);
                _portName = sr.ReadLine();
                sr.Close();
            }
            catch (Exception ex)
            {
                Logs.WriteLine(ex);
            }
        }

        static SerialPortManager _spManager = null;
        static SerialSettings _mySerialSettings = null;

        const int _timeOut = 15000;
        const int _baudRate = 9600;
        const int _nBits = 8;
        const int _tick = 120000;
        const int _lengthPesata = 8;
        
        static string _pesata;

        /// <summary>
        /// Imposta l'oggetto per leggere dalla seriale e fa partire un thread apposito per la lettura
        /// </summary>
        private void inizializzaSeriale()
        {
            try
            {
                if (_spManager == null)
                {
                    _spManager = new SerialPortManager();
                    _mySerialSettings = _spManager.CurrentSerialSettings;

                    _mySerialSettings.BaudRate = _baudRate;
                    _mySerialSettings.PortName = _portName;
                    _mySerialSettings.Parity = System.IO.Ports.Parity.None;
                    _mySerialSettings.StopBits = System.IO.Ports.StopBits.One;
                    _mySerialSettings.DataBits = _nBits;

                    GC.SuppressFinalize(_spManager);

                    _spManager.StartListening();

                    _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);
                }
            }
            catch (Exception ex)
            {
                Logs.WriteLine(ex);
            }
        }

        /// <summary>
        /// Richiede i dati alla seriale quando arriva un messaggio contente il carattere $ al webserver in ascolto
        /// </summary>
        public static string requestToSerial()
        {
            try
            {
                _pesata = "";

                _spManager.SendData("$");

                //Ferma l'esecuzione del thread e attende la lettura completa del dato dalla seriale.
                //Se la condizione non viene soddisfatta allo scadere del timeOut l'esecuzione andrà avanti.
                bool notTimeout = SpinWait.SpinUntil(() => _pesata.Length == _lengthPesata, _timeOut);

                Logs.WriteLine("Pesata length: " + _pesata.Length + "  " + _pesata);

                if (notTimeout)
                {
                    Logs.WriteLine("Peso letto: " + _pesata);
                    Logs.WriteLine();
                }
                else
                {
                    Logs.WriteLine("TIMEOUT");
                    _pesata = "-1";
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

        public void Debug(string port)
        {
            Start(port);
        }
    }
}
