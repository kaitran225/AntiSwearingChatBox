using System;
using System.Net;
using System.Net.Sockets;

namespace AntiSwearingChatBox.ConsoleChat.SystemUtils
{
    public static class NetworkUtils
    {
        // Get local IP address for display purposes
        public static string GetLocalIPAddress()
        {
            try
            {
                using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530); // Use Google's public DNS server
                IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint?.Address.ToString() ?? "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        // Check if a port is available
        public static bool IsPortAvailable(int port)
        {
            try
            {
                using TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        // Check if a server is already running on the port
        public static bool IsServerRunning(int port)
        {
            try
            {
                using var client = new TcpClient();
                client.Connect("localhost", port);
                client.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Find an available port starting from the specified port
        public static int FindAvailablePort(int startPort)
        {
            int port = startPort;
            while (!IsPortAvailable(port) && port < startPort + 100)
            {
                port++;
            }
            return port;
        }

        public static string GetPublicIpAddress()
        {
            try
            {
                WebClient webClient = new();
                using WebClient client = webClient;
                return client.DownloadString("http://ifconfig.me/ip").Trim();
            }
            catch
            {
                return "Unable to determine";
            }
        }
    }
} 