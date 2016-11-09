using System.ServiceProcess;

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
                    Scheduler service = new Scheduler();
                    //Togliere commento a service.Debug() per abilitare debug
                    //service.Debug("COM4");

                    // Put a breakpoint on the following line to always catch
                    // your service when it has finished its work
                    System.Threading.Thread.Sleep(timeOut);
                    break;
            }
        }
    }
}
