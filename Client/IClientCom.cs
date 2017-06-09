namespace Emroy.Vfs.Client
{
    public interface IClientCom
    {
        /// <summary>
        /// Establishes connection to service 
        /// </summary>
        /// <param name="serviceUrl"></param>
        /// <returns></returns>
        string ConnectToServer(string serviceUrl);

        /// <summary>
        /// Prints help
        /// </summary>
        void PrintHelp();

        /// <summary>
        /// Client main processing
        /// </summary>
        void Process();
    }
}