using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace requestPeso
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        static void Main()
        {
            bool debug = false;
            int timeOut = 240000;

            switch (debug)
            {
                case false:

                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                new Scheduler()
                    };
                    ServiceBase.Run(ServicesToRun);

                    break;

                case true:

#if (!DEBUG)
                System.ServiceProcess.ServiceBase[] ServicesToRun;
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { new Service1() };
                System.ServiceProcess.ServiceBase.Run(ServicesToRun);
#else
                    Scheduler service = new Scheduler();
                    //Togliere commento a service.Start() per abilitare debug
                    //service.Debug("COM7");
                    // Put a breakpoint on the following line to always catch
                    // your service when it has finished its work
                    System.Threading.Thread.Sleep(timeOut);
#endif

                    break;
            }
        }
    }
}
