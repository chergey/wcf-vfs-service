using System;
using System.ServiceModel;
using Emroy.Vfs.Service.Impl;

namespace Emroy.Vfs.ConsoleHost
{
    public class Program
    {
        private static ServiceHost _svcHost;

        public static void Main(string[] args)
        {
            //_svcHost = new ServiceHost(typeof (VfsService));
            _//svcHost.Open();
            Console.WriteLine("Press key to stop service...");
            Console.Read();
        }

      
    }
}
