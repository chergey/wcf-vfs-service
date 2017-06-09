using System;

namespace Emroy.Vfs.Client
{
    public class Program
    {
        private static readonly ClientCom ClientCom = new ClientCom();

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            ClientCom.Process();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Console.WriteLine(ex.Message);
        }
    }
}
