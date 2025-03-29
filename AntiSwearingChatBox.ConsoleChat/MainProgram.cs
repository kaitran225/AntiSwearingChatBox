using AntiSwearingChatBox.AI;
using AntiSwearingChatBox.AI.Moderation;
using AntiSwearingChatBox.ConsoleChat.AITesting;
using AntiSwearingChatBox.ConsoleChat.SystemUtils;
using Microsoft.Extensions.Options;

namespace AntiSwearingChatBox.ConsoleChat
{
    class ConsoleChat
    {
        static async Task Main(string[] args)
        {
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

            bool exit = false;
            while (!exit)
            {
                // Show main menu
                Console.WriteLine("\nMain Menu:");
                Console.WriteLine("1. Start Chat");
                Console.WriteLine("2. System Validator");
                Console.WriteLine("3. AI Testing Console");
                Console.WriteLine("4. Exit");
                Console.Write("\nSelect an option: ");
                string? mainOption = Console.ReadLine();

                switch (mainOption)
                {
                    case "1":
                        await StartChatAsync();
                        break;
                    case "2":
                        var systemValidator = new SystemValidator();
                        await systemValidator.RunSystemValidator();
                        break;
                    case "3":
                        await StartAITestingConsoleAsync();
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        static async Task StartChatAsync()
        {
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
                // Start the server in a background task
                var chatServer = new ChatServer.ChatServer(defaultPort);
                await Task.Run(async () => await chatServer.StartAsync());
                
                // Wait a moment for the server to start
                await Task.Delay(2000);
                
                string localIp = NetworkUtils.GetLocalIPAddress();
                Console.WriteLine($"Server started. Your local IP: {localIp}");
                Console.WriteLine($"Other computers can connect using: http://{localIp}:{defaultPort}");
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

            // Create and configure the chat client
            var chatClient = new ChatClient.ChatClient(server);
            
            try
            {
                // Connect to server
                await chatClient.ConnectAsync();

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
                await chatClient.JoinChatAsync(username);

                // Message loop
                while (true)
                {
                    string? message = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(message))
                        continue;

                    if (message.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                        break;

                    await chatClient.SendMessageAsync(message);
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
                await chatClient.DisconnectAsync();
                
                Console.WriteLine("Press any key to return to main menu...");
                Console.ReadKey();
            }
        }

        static async Task StartAITestingConsoleAsync()
        {
            try
            {
                // Load configuration from appsettings.json
                string projectDir = Directory.GetCurrentDirectory();
                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(projectDir)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Initialize Gemini Service with settings from configuration
                var settings = new GeminiSettings
                {
                    ApiKey = configuration["GeminiSettings:ApiKey"] ?? "AIzaSyD9Odq-PkFqA2HHYsR86EEhPbM85eHF2Sw",
                    ModelName = configuration["GeminiSettings:ModelName"] ?? "gemini-1.5-pro"
                };
                
                Console.WriteLine($"Using model: {settings.ModelName}");
                Console.WriteLine($"API Key: {settings.ApiKey.Substring(0, 8)}...[redacted]");
                
                var geminiService = new GeminiService(Options.Create(settings));
                var modelSettings = ModelSettings.Instance;
                
                var aiTestConsole = new AITestConsole(geminiService, modelSettings);
                await aiTestConsole.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error initializing AI Testing Console: {ex.Message}");
                Console.ResetColor();
                
                Console.WriteLine("Press any key to return to main menu...");
                Console.ReadKey();
            }
        }
    }
} 