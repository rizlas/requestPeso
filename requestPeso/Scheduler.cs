using System;
using System.ServiceProcess;
using System.IO;
using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Web.Http.Cors;
using System.IO.Ports;

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

                config.Routes.MapHttpRoute(name: "DefaultApi",
                                            routeTemplate: "api/{controller}/{id}",
                                            defaults: new { id = RouteParameter.Optional });

                config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                HttpSelfHostServer server = new HttpSelfHostServer(config);
                server.OpenAsync().Wait();

                Logs.WriteLine("WEBSERVER CREATO!");
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
            if (_vc != null)
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

        public void Debug(string port)
        {
            Start(port);
        }
    }
}