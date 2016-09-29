using System;
using System.ServiceProcess;
using SerialPortListener.Serial;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Web.Http.Cors;

namespace requestPeso
{
    public partial class Scheduler : ServiceBase
    {
        ValuesController _vc = null;

        static string _portName;

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
            else
            {
                Logs.errorLogs("Parametri errati, usare --port COMX");
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

            _vc = new ValuesController();
            inizializzaServerWeb();

            Logs.errorLogs("Servizio partito");
        }

        private void inizializzaServerWeb()
        {
            try
            {
                HttpSelfHostConfiguration config = new HttpSelfHostConfiguration("http://localhost:8080");
                
                config.Routes.MapHttpRoute( name: "DefaultApi",
                                            routeTemplate: "api/{controller}/{id}",
                                            defaults: new { id = RouteParameter.Optional });
                
                config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                HttpSelfHostServer server = new HttpSelfHostServer(config);
                server.OpenAsync().Wait();

                Logs.errorLogs("WEBSERVER CREATO!");
            }
            catch (Exception ex)
            {
                Logs.errorLogs(ex);
            }
        }
        
        /// <summary>
        /// Ferma il servizio
        /// </summary>
        protected override void OnStop()
        {
            _vc.Dispose();
            Logs.errorLogs("Servizio fermato");
            
        }
    }
}
