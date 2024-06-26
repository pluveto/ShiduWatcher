using System.Net;
using System.Net.Sockets;

namespace ShiduWatcher
{
    class NetworkHelper
    {

        public static int FindAvailablePort(int startPort, int endPort)
        {
            for (int port = startPort; port <= endPort; port++)
            {
                if (IsPortAvailable(port))
                {
                    return port;
                }
            }
            return -1;
        }

        public static bool IsPortAvailable(int port)
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
