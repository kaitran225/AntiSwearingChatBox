using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.ConsoleChat.SystemUtils
{
    public static class NetworkUtils
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        /// <summary>
        /// Gets the local IP address of the machine
        /// </summary>
        public static string GetLocalIPAddress()
        {
            try
            {
                using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530); // Use Google's public DNS server
                IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint?.Address.ToString() ?? "127.0.0.1";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting local IP address: {ex.Message}");
                return "127.0.0.1";
            }
        }

        /// <summary>
        /// Checks if a port is available on the local machine
        /// </summary>
        public static bool IsPortAvailable(int port)
        {
            try
            {
                using var tcpClient = new TcpClient();
                var result = tcpClient.BeginConnect("127.0.0.1", port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));
                
                if (success)
                {
                    tcpClient.EndConnect(result);
                    return false; // Port is in use
                }
                
                return true; // Port is available
            }
            catch
            {
                return true; // Error means port is most likely available
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

        /// <summary>
        /// Gets the public IP address of the machine
        /// </summary>
        public static async Task<string> GetPublicIPAddressAsync()
        {
            try
            {
                string result = await _httpClient.GetStringAsync("https://api.ipify.org");
                return result.Trim();
            }
            catch
            {
                return "Unable to determine";
            }
        }
    }
} 