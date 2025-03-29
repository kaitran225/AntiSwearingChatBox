using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.ConsoleChat.ChatClient
{
    public class ChatClient
    {
        private HubConnection _connection;
        private string _server;
        private string _username;

        public ChatClient(string server)
        {
            _server = server;
            
            // Create hub connection
            _connection = new HubConnectionBuilder()
                .WithUrl($"{_server}/chatHub")
                .WithAutomaticReconnect()
                .Build();

            // Set up event handlers
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{user}: ");
                Console.ResetColor();
                Console.WriteLine(message);
            });

            _connection.On<string, string>("PrivateMessage", (user, message) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{user} (private): ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ResetColor();
            });

            _connection.On<string>("UserJoined", (user) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($">> {user} has joined the chat");
                Console.ResetColor();
            });

            _connection.On<string>("UserLeft", (user) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($">> {user} has left the chat");
                Console.ResetColor();
            });

            _connection.On<string>("JoinConfirmed", (user) =>
            {
                _username = user;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($">> You have joined as {user}");
                Console.WriteLine(">> Type your message and press Enter to send");
                Console.WriteLine(">> Type '/exit' to quit the chat");
                Console.ResetColor();
            });

            _connection.On<List<string>>("UserList", (users) =>
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(">> Connected users:");
                foreach (var user in users)
                {
                    Console.WriteLine($"   - {user}");
                }
                Console.ResetColor();
            });

            _connection.On<string>("Error", (message) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($">> ERROR: {message}");
                Console.ResetColor();
            });
        }

        public async Task ConnectAsync()
        {
            try
            {
                Console.WriteLine($"\nConnecting to chat server at {_server}...");
                await _connection.StartAsync();
                Console.WriteLine("Connected to server!");
                return;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error connecting to server: {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }

        public async Task JoinChatAsync(string username)
        {
            await _connection.InvokeAsync("JoinChat", username);
            _username = username;
        }

        public async Task SendMessageAsync(string message)
        {
            await _connection.InvokeAsync("SendMessage", message);
        }

        public async Task DisconnectAsync()
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.StopAsync();
            }
        }

        public string GetUsername()
        {
            return _username;
        }

        public HubConnectionState GetConnectionState()
        {
            return _connection.State;
        }
    }
} 