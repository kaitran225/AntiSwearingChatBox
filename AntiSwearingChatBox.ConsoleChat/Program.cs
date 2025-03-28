using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

// Configuration
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "Anti-Swearing Chat Box";

// Initial UI
Console.Clear();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║             Anti-Swearing Chat Box               ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.ResetColor();

// Get local IP address for display purposes
string GetLocalIPAddress()
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
bool IsPortAvailable(int port)
{
    try
    {
        using TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        listener.Stop();
        return true;
    }
    catch
    {
        return false;
    }
}

// Find an available port starting from the specified port
int FindAvailablePort(int startPort)
{
    int port = startPort;
    while (!IsPortAvailable(port) && port < startPort + 100)
    {
        port++;
    }
    return port;
}

// Default server configuration
int defaultPort = 5122;
int serverPort = FindAvailablePort(defaultPort);
string server = $"http://localhost:{serverPort}";

// Show connection options
Console.WriteLine("\nConnection mode:");
Console.WriteLine("1. Connect to a server");
Console.WriteLine("2. Start as server and connect");
Console.Write("\nSelect an option (default: 1): ");
string? option = Console.ReadLine();

bool runAsServer = option == "2";

// If running as server, start it in the background
if (runAsServer)
{
    if (serverPort != defaultPort)
    {
        Console.WriteLine($"\nPort {defaultPort} is in use. Using port {serverPort} instead.");
    }
    
    Console.WriteLine($"\nStarting server on port {serverPort}...");
    Task.Run(async () => await RunServer(serverPort));
    // Wait a moment for the server to start
    await Task.Delay(2000);
    
    Console.WriteLine($"Server started. Your local IP: {GetLocalIPAddress()}");
    Console.WriteLine($"Other computers can connect using: http://{GetLocalIPAddress()}:{serverPort}");
}
else
{
    // If connecting to an existing server, prompt for address
    Console.WriteLine("\nEnter server address (e.g., 192.168.1.100:5122)");
    Console.Write("or press Enter for localhost: ");
    string? serverInput = Console.ReadLine();
    
    if (!string.IsNullOrWhiteSpace(serverInput))
    {
        // Add http:// prefix if not present
        if (!serverInput.StartsWith("http://") && !serverInput.StartsWith("https://"))
        {
            serverInput = $"http://{serverInput}";
        }
        
        server = serverInput;
    }
}

// Create hub connection
var connection = new HubConnectionBuilder()
    .WithUrl($"{server}/chatHub")
    .WithAutomaticReconnect()
    .Build();

// Set up event handlers
connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write($"{user}: ");
    Console.ResetColor();
    Console.WriteLine(message);
});

connection.On<string, string>("PrivateMessage", (user, message) =>
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write($"{user} (private): ");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(message);
    Console.ResetColor();
});

connection.On<string>("UserJoined", (user) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($">> {user} has joined the chat");
    Console.ResetColor();
});

connection.On<string>("UserLeft", (user) =>
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($">> {user} has left the chat");
    Console.ResetColor();
});

connection.On<string>("JoinConfirmed", (user) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($">> You have joined as {user}");
    Console.WriteLine(">> Type your message and press Enter to send");
    Console.WriteLine(">> Type '/exit' to quit the chat");
    Console.ResetColor();
});

connection.On<List<string>>("UserList", (users) =>
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(">> Connected users:");
    foreach (var user in users)
    {
        Console.WriteLine($"   - {user}");
    }
    Console.ResetColor();
});

connection.On<string>("Error", (message) =>
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($">> ERROR: {message}");
    Console.ResetColor();
});

// Start connection
try
{
    Console.WriteLine($"\nConnecting to chat server at {server}...");
    await connection.StartAsync();
    Console.WriteLine("Connected to server!");

    // Get username
    Console.WriteLine("\nEnter your username:");
    string? username = null;
    while (string.IsNullOrWhiteSpace(username))
    {
        username = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(username))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Username cannot be empty. Please try again:");
            Console.ResetColor();
        }
    }

    // Join chat
    await connection.InvokeAsync("JoinChat", username);

    // Message loop
    while (true)
    {
        string? message = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(message))
            continue;

        if (message.Equals("/exit", StringComparison.OrdinalIgnoreCase))
            break;

        await connection.InvokeAsync("SendMessage", message);
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: {ex.Message}");
    if (!runAsServer)
    {
        Console.WriteLine("Make sure the server is running and the address is correct.");
        Console.WriteLine("Try starting as server (option 2) instead.");
    }
    Console.ResetColor();
}
finally
{
    // Clean up
    if (connection.State == HubConnectionState.Connected)
    {
        await connection.StopAsync();
    }
    
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

// Method to run the server in the background with specified port
async Task RunServer(int port)
{
    try
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        
        // Add SignalR services
        builder.Services.AddSignalR();
        
        // Register profanity filter service (simplified version)
        builder.Services.AddSingleton<IProfanityFilter, SimpleProfanityFilter>();
        
        // Configure CORS for local network
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader());
        });
        
        var app = builder.Build();
        
        // Configure the HTTP request pipeline
        app.UseCors("CorsPolicy");
        
        // Map the ChatHub
        app.MapHub<ChatHub>("/chatHub");
        
        await app.RunAsync();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Server error: {ex.Message}");
        Console.ResetColor();
    }
}

// Simple implementation of the interfaces and classes needed for the server
public interface IProfanityFilter
{
    Task<bool> ContainsProfanityAsync(string text);
    Task<string> FilterTextAsync(string text);
}

public class SimpleProfanityFilter : IProfanityFilter
{
    private static readonly string[] _badWords = new[] { "badword", "damn", "swear", "profanity" };
    
    public Task<bool> ContainsProfanityAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Task.FromResult(false);
            
        return Task.FromResult(_badWords.Any(word => 
            text.ToLower().Contains(word.ToLower())));
    }
    
    public Task<string> FilterTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Task.FromResult(string.Empty);
            
        string filtered = text;
        foreach (var word in _badWords)
        {
            filtered = Regex.Replace(
                filtered,
                $@"\b{word}\b",
                new string('*', word.Length),
                RegexOptions.IgnoreCase);
        }
        
        return Task.FromResult(filtered);
    }
}

public class ChatHub : Hub
{
    private static readonly Dictionary<string, string> _connectedUsers = new();
    private readonly IProfanityFilter _profanityFilter;
    
    public ChatHub(IProfanityFilter profanityFilter)
    {
        _profanityFilter = profanityFilter;
    }
    
    public async Task SendMessage(string message)
    {
        string user = _connectedUsers[Context.ConnectionId];
        
        // Filter message for profanity
        var filteredMessage = await _profanityFilter.FilterTextAsync(message);
        
        // Check if message was filtered
        bool containedProfanity = await _profanityFilter.ContainsProfanityAsync(message);
        
        if (containedProfanity)
        {
            // Send a private warning to the user
            await Clients.Caller.SendAsync("PrivateMessage", "System", "Your message contained inappropriate language and was filtered.");
        }
        
        // Broadcast filtered message to all clients
        await Clients.All.SendAsync("ReceiveMessage", user, filteredMessage);
    }
    
    public async Task JoinChat(string username)
    {
        // Check if username contains profanity
        if (await _profanityFilter.ContainsProfanityAsync(username))
        {
            await Clients.Caller.SendAsync("Error", "Username contains inappropriate language. Please choose another username.");
            return;
        }
        
        _connectedUsers[Context.ConnectionId] = username;
        await Clients.Others.SendAsync("UserJoined", username);
        await Clients.Caller.SendAsync("JoinConfirmed", username);
        
        // Send the list of connected users to the new client
        await Clients.Caller.SendAsync("UserList", _connectedUsers.Values.ToList());
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectedUsers.TryGetValue(Context.ConnectionId, out string? username))
        {
            _connectedUsers.Remove(Context.ConnectionId);
            await Clients.Others.SendAsync("UserLeft", username);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
