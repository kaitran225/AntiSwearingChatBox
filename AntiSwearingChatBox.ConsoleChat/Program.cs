using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Security.Principal;

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
    catch (SocketException)
    {
        return false;
    }
}

// Check if a server is already running on the port
bool IsServerRunning(int port)
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
int FindAvailablePort(int startPort)
{
    int port = startPort;
    while (!IsPortAvailable(port) && port < startPort + 100)
    {
        port++;
    }
    return port;
}

// Show main menu
Console.WriteLine("\nMain Menu:");
Console.WriteLine("1. Start Chat");
Console.WriteLine("2. System Validator");
Console.WriteLine("3. Exit");
Console.Write("\nSelect an option: ");
string? mainOption = Console.ReadLine();

if (mainOption == "2")
{
    await RunSystemValidator();
    return;
}
else if (mainOption == "3")
{
    return;
}

// Default server configuration
int defaultPort = 5122;
string server = $"http://localhost:{defaultPort}";

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
    Console.WriteLine($"\nStarting server on port {defaultPort}...");
    Task.Run(async () => await RunServer(defaultPort));
    // Wait a moment for the server to start
    await Task.Delay(2000);
    
    Console.WriteLine($"Server started. Your local IP: {GetLocalIPAddress()}");
    Console.WriteLine($"Other computers can connect using: http://{GetLocalIPAddress()}:{defaultPort}");
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

// Method to run the system validator
async Task RunSystemValidator()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("=== SimpleChat System Requirements Validator ===\n");
    Console.ResetColor();
    
    var results = new List<(string Requirement, bool Passed, string Details)>();

    // Check .NET Version
    results.Add(CheckDotNetVersion());

    // Check SQL Server
    results.Add(await CheckSqlServer());

    // Check Port Availability
    results.Add(CheckPortAvailability());

    // Check Windows Firewall
    results.Add(CheckWindowsFirewall());

    // Check Network Configuration
    results.Add(CheckNetworkConfiguration());

    // Check Permissions
    results.Add(CheckPermissions());

    // Print Results
    Console.WriteLine("\n=== Validation Results ===\n");
    foreach (var (requirement, passed, details) in results)
    {
        Console.WriteLine($"{requirement}:");
        Console.WriteLine($"Status: {(passed ? "✓ PASSED" : "✗ FAILED")}");
        Console.WriteLine($"Details: {details}\n");
    }

    // Summary
    var passedCount = results.Count(r => r.Passed);
    var totalCount = results.Count;
    Console.WriteLine($"=== Summary ===\n");
    Console.WriteLine($"Total Requirements: {totalCount}");
    Console.WriteLine($"Passed: {passedCount}");
    Console.WriteLine($"Failed: {totalCount - passedCount}");
    Console.WriteLine($"\nSystem is {(passedCount == totalCount ? "READY" : "NOT READY")} for SimpleChat");
    
    Console.WriteLine("\nPress any key to return to main menu...");
    Console.ReadKey();
}

// System Validator Helper Methods
(string, bool, string) CheckDotNetVersion()
{
    try
    {
        var version = Environment.Version;
        var isCompatible = version.Major >= 9;
        return (
            ".NET Version",
            isCompatible,
            $"Current Version: {version}\nRequired: .NET 9.0 or higher"
        );
    }
    catch (Exception ex)
    {
        return (".NET Version", false, $"Error checking version: {ex.Message}");
    }
}

async Task<(string, bool, string)> CheckSqlServer()
{
    try
    {
        var connectionString = "Server=localhost\\SQLEXPRESS;Database=SimpleChat;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3";
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        
        // Set a short timeout for the connection attempt
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        await connection.OpenAsync(cancellationTokenSource.Token);
        
        return ("SQL Server", true, "Successfully connected to SQL Server Express");
    }
    catch (TaskCanceledException)
    {
        return (
            "SQL Server",
            false,
            "Connection attempt to SQL Server timed out.\nPlease ensure SQL Server is installed and running."
        );
    }
    catch (Exception ex)
    {
        return (
            "SQL Server",
            false,
            $"Failed to connect to SQL Server Express. Please ensure:\n" +
            "1. SQL Server Express is installed\n" +
            "2. SQL Server service is running\n" +
            "3. Windows Authentication is enabled\n" +
            $"Error: {ex.Message}"
        );
    }
}

(string, bool, string) CheckPortAvailability()
{
    try
    {
        var availablePorts = new List<int>();
        var startPort = 5000;
        
        for (int i = 0; i < 10; i++)
        {
            int port = startPort + i;
            if (IsPortAvailable(port))
            {
                availablePorts.Add(port);
            }
        }
        
        if (availablePorts.Count > 0)
        {
            return (
                "Port Availability",
                true,
                $"Available ports: {string.Join(", ", availablePorts)}\n" +
                "At least one port is available for use"
            );
        }
        else
        {
            return (
                "Port Availability",
                false,
                "No available ports found in the range 5000-5009.\n" +
                "Please ensure no other applications are using these ports."
            );
        }
    }
    catch (Exception ex)
    {
        return (
            "Port Availability",
            false,
            $"Error checking port availability: {ex.Message}"
        );
    }
}

(string, bool, string) CheckWindowsFirewall()
{
    try
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "advfirewall firewall show rule name=\"SimpleChat\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (output.Contains("SimpleChat"))
        {
            return ("Windows Firewall", true, "Firewall rule for SimpleChat exists");
        }
        
        return (
            "Windows Firewall",
            false,
            "No firewall rule found for SimpleChat.\n" +
            "You may need to add an inbound rule for the application port."
        );
    }
    catch (Exception ex)
    {
        return ("Windows Firewall", false, $"Error checking firewall: {ex.Message}");
    }
}

(string, bool, string) CheckNetworkConfiguration()
{
    try
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var localIp = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        var publicIp = GetPublicIpAddress();

        return (
            "Network Configuration",
            true,
            $"Local IP: {localIp}\n" +
            $"Public IP: {publicIp}\n" +
            "Network configuration is valid"
        );
    }
    catch (Exception ex)
    {
        return ("Network Configuration", false, $"Error checking network: {ex.Message}");
    }
}

(string, bool, string) CheckPermissions()
{
    try
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

        return (
            "Permissions",
            isAdmin,
            $"Current User: {identity.Name}\n" +
            $"Administrator Rights: {(isAdmin ? "Yes" : "No")}\n" +
            "Note: Administrator rights are recommended for server setup"
        );
    }
    catch (Exception ex)
    {
        return ("Permissions", false, $"Error checking permissions: {ex.Message}");
    }
}

string GetPublicIpAddress()
{
    try
    {
        using var client = new WebClient();
        return client.DownloadString("http://ifconfig.me/ip").Trim();
    }
    catch
    {
        return "Unable to determine";
    }
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
