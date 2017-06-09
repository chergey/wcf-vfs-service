using System.ServiceProcess;

namespace Emroy.Vfs.WinService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun = {
                new WinSevice()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
