using System;
using System.ServiceProcess;
using SerialPortListener.Serial;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace requestPeso
{
    public partial class Scheduler : ServiceBase
    {
        SerialPortManager _spManager;
        UdpClient _udpServer;
        IPEndPoint _remoteEP;
        const int _port = 34567;

        const int _timeOut = 60000;

        const int _baudRate = 9600;
        const int _nBits = 8;
        const string _pathToCom = "C:\\requestoPeso.txt";
        string _portName = "COM7";

        const int _lengthPesata = 8;
        string _pesata;

        public Scheduler()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }
        
        /// <summary>
        /// Inizializza la variabile _portName dal file C:\requestoPeso.txt e imposta le variabili per leggere dalla seriale
        /// Da mettere public per debug
        /// </summary>
        private void Start()
        {
            getComToUse();
            inizializzaSeriale();
            errorLogs("Servizio partito");
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
                errorLogs(ex);
            }
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

                _spManager.StartListening();

                var threadOnStart = new Thread(requestToSerial);
                threadOnStart.Name = "threadConnection";
                threadOnStart.IsBackground = false;
                threadOnStart.Start();
            }
            catch (Exception ex)
            {
                errorLogs(ex);
            }
        }

        /// <summary>
        /// Richiede i dati alla seriale quando arriva un messaggio contente il carattere $ al server UDP in ascolto
        /// </summary>
        private void requestToSerial()
        {
            try
            {
                _udpServer = new UdpClient(_port);
                _remoteEP = new IPEndPoint(IPAddress.Any, _port);
                _pesata = "";

                errorLogs("SERVER CREATO!");

                while (true)
                {
                    byte[] data = _udpServer.Receive(ref _remoteEP);

                    string received = Encoding.ASCII.GetString(data);
                    errorLogs("receive data from " + _remoteEP.ToString() + " " + received);

                    if (received == "$")
                    {
                        _spManager.SendData("$");

                        //Ferma l'esecuzione del thread e attende la lettura completa del dato dalla seriale.
                        //Se la condizione non viene soddisfatta allo scadere del timeOut l'esecuzione andrà avanti.
                        bool notTimeout = SpinWait.SpinUntil(() => _pesata.Length == _lengthPesata, _timeOut);

                        if (notTimeout)
                        {
                            byte[] msg = Encoding.ASCII.GetBytes(_pesata.Trim());
                            _udpServer.Send(msg, msg.Length, _remoteEP);
                            errorLogs("Peso letto: " + _pesata);
                        }
                        else
                        {
                            byte[] msg = Encoding.ASCII.GetBytes(string.Empty);
                            _udpServer.Send(msg, msg.Length, _remoteEP);
                            errorLogs("TIMEOUT");
                        }

                        _pesata = "";
                    }
                    //Debug
                    //else
                    //{
                    //    byte[] msg = Encoding.ASCII.GetBytes("Sono il server");
                    //    _udpServer.Send(msg, msg.Length, _remoteEP); // reply back
                    //}
                }
            }
            catch (Exception ex)
            {
                errorLogs(ex);
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
        /// Ferma il servizio
        /// </summary>
        protected override void OnStop()
        {
            _spManager.StopListening();
            _spManager.Dispose();

            errorLogs("Servizio fermato");
        }

        /// <summary>
        /// Scrive su file gli errori generati
        /// </summary>
        /// <param name="ex">Oggetto che ha scatenato l'eccezione</param>
        private void errorLogs(Exception ex)
        {
            StreamWriter sw = null;

            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "LogPeso.txt", true);
                string dt = String.Format("{0:dd-MM-yyyy HH:mm:ss}", DateTime.Now);
                sw.WriteLine(String.Format("{0} - {1} - {2}", dt, ex.Source.ToString(), ex.Message));
                sw.Close();
                this.Stop();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Scrive su file i log interessanti per l'utente
        /// </summary>
        /// <param name="message">Messaggio da salvare</param>
        private void errorLogs(string message)
        {
            StreamWriter sw = null;

            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "LogPeso.txt", true);
                string dt = String.Format("{0:dd-MM-yyyy HH:mm:ss}", DateTime.Now);
                sw.WriteLine(String.Format("{0} - {1}", dt, message));
                sw.Close();
            }
            catch
            {
            }
        }
    }
}
