using System;
using System.Web.Http;
using SerialPortListener.Serial;
using System.Threading;
using System.Text;

namespace requestPeso
{
    public class ValuesController : ApiController
    {
        //SerialPortManager _spManager = null;
        //SerialSettings _mySerialSettings = null;

        //const int _timeOut = 15000;
        //const int _baudRate = 9600;
        //const int _nBits = 8;
        //const int _tick = 120000;
        //const int _lengthPesata = 8;

        //string _portName = "COMX";
        //string _pesata;
        string _user;

        public ValuesController()
        {
        }

        ~ValuesController()
        {
            Logs.WriteLine("Distruttore");

            //if (_spManager != null)
            //    _spManager.Dispose();
        }

        /// <summary>
        /// Gestisce le richieste in entrata.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns></returns>
        public string Get(string command, string user)
        {
            //if (_spManager != null)
            //{
            string tmp = "";
            _user = user;
            Logs.WriteLine("Ricevuta richiesta da: " + _user);

            if (command == "$")
            {
                //_spManager.StartListening();

                tmp = Scheduler.requestToSerial();

                if (tmp.Trim() == "0.0000")
                    tmp = "-1";

                //_spManager.StopListening();
                //_spManager.Dispose();
            }

            return tmp.Trim();
            //}
            //else
            //    return string.Empty;
        }

    }
}
