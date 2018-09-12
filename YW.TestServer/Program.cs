using System.ServiceModel;

namespace YW.TestServer
{
    class Program
    {
        private static ServiceHost _clientHost;
        private static ServiceHost _fileHost;
        static void Main(string[] args)
        {
            _clientHost = new ServiceHost(typeof(WCF.Client));
            _fileHost = new ServiceHost(typeof(WCF.GFile));
            _clientHost.Open();
            _fileHost.Open();
            System.Console.ReadLine();
            _clientHost.Close();
            _fileHost.Close();
        }
    }
}
