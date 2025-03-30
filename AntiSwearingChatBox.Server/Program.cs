using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository;
using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Service;
using AntiSwearingChatBox.Service.Interface;
using AntiSwearingChatBox.AI.Interfaces;
using AntiSwearingChatBox.AI.Services;
using AntiSwearingChatBox.Server.Service;
using System.Threading;

namespace AntiSwearingChatBox.Server
{
    public class Program
    {
        private static Microsoft.Extensions.DependencyInjection.ServiceProvider _serviceProvider = null!;
        private static User? _currentUser = null;
        private static bool _isRunning = true;
        
        // App color palette
        private static class Colors
        {
            public static ConsoleColor Background = ConsoleColor.Black;
            public static ConsoleColor Primary = ConsoleColor.DarkCyan;
            public static ConsoleColor Secondary = ConsoleColor.Cyan;
            public static ConsoleColor Accent = ConsoleColor.Magenta;
            public static ConsoleColor Warning = ConsoleColor.Yellow;
            public static ConsoleColor Error = ConsoleColor.Red;
            public static ConsoleColor Success = ConsoleColor.Green;
            public static ConsoleColor NormalText = ConsoleColor.White;
            public static ConsoleColor Timestamp = ConsoleColor.DarkGray;
            public static ConsoleColor Username = ConsoleColor.Cyan;
            public static ConsoleColor SystemMessage = ConsoleColor.DarkYellow;
        }

        public static async Task Main(string[] args)
        {
            // Set console colors
            Console.BackgroundColor = Colors.Background;
            Console.ForegroundColor = Colors.NormalText;
            Console.Clear();
            
            ColorWriteLine("=== AntiSwearingChatBox CLI ===", Colors.Primary);
            
            // Setup services
            ConfigureServices();
            
            // Start command loop
            while (_isRunning)
            {
                DisplayPrompt();
                string command = Console.ReadLine() ?? string.Empty;
                await ProcessCommandAsync(command);
            }
            
            // Clean up services
            _serviceProvider.Dispose();
        }
        
        private static void ColorWrite(string text, ConsoleColor color)
        {
            ConsoleColor previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = previous;
        }
        
        private static void ColorWriteLine(string text, ConsoleColor color)
        {
            ColorWrite(text + Environment.NewLine, color);
        }
        
        private static void ConfigureServices()
        {
            // Create configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
                
            // Create service collection
            var services = new ServiceCollection();
            
            // Add configuration
            services.AddSingleton(configuration);
            
            // Add DB context
            services.AddDbContext<AntiSwearingChatBoxContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("AntiSwearingChatBox")));
            
            // Register repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Register services
            services.AddScoped<IChatThreadService, ChatThreadService>();
            services.AddScoped<IMessageHistoryService, MessageHistoryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserWarningService, UserWarningService>();
            services.AddScoped<IThreadParticipantService, ThreadParticipantService>();
            services.AddScoped<IFilteredWordService, FilteredWordService>();
            services.AddScoped<IAuthService, AuthService>();
            
            // Register profanity filter service
            services.AddSingleton<IProfanityFilter, ProfanityFilterService>();
            
            // Build service provider
            _serviceProvider = services.BuildServiceProvider();
        }
        
        private static void DisplayPrompt()
        {
            string prompt = _currentUser == null ? "Guest" : _currentUser.Username;
            ColorWrite($"{prompt}> ", Colors.Primary);
        }
        
        private static async Task ProcessCommandAsync(string command)
        {
            string[] parts = command.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0)
                return;
                
            string cmd = parts[0].ToLower();
            
            switch (cmd)
            {
                case "help":
                    DisplayHelp();
                    break;
                    
                case "exit":
                case "quit":
                    _isRunning = false;
                    ColorWriteLine("Goodbye!", Colors.Primary);
                    break;
                    
                case "login":
                    await LoginAsync(parts);
                    break;
                    
                case "register":
                    await RegisterAsync(parts);
                    break;
                    
                case "logout":
                    Logout();
                    break;
                    
                case "list":
                    ListItems(parts);
                    break;
                    
                case "create":
                    await CreateItemAsync(parts);
                    break;
                    
                case "chat":
                    if (parts.Length < 2)
                    {
                        ColorWriteLine("Usage: chat <groupId|userId>", Colors.Warning);
                        return;
                    }
                    
                    if (int.TryParse(parts[1], out int chatId))
                    {
                        // Check if it's a user ID or a group ID
                        var userService = _serviceProvider.GetRequiredService<IUserService>();
                        var chatThreadService = _serviceProvider.GetRequiredService<IChatThreadService>();
                        
                        // If it's a valid group ID, enter that chat
                        var group = chatThreadService.GetById(chatId);
                        if (group != null)
                        {
                            await EnterChatSessionAsync(chatId);
                            return;
                        }
                        
                        // If it's a valid user ID, find or create a personal chat
                        var user = userService.GetById(chatId);
                        if (user != null)
                        {
                            int personalChatId = await FindOrCreatePersonalChatAsync(chatId);
                            if (personalChatId > 0)
                            {
                                await EnterChatSessionAsync(personalChatId);
                            }
                            return;
                        }
                        
                        ColorWriteLine("Invalid ID. Not a valid group or user ID.", Colors.Error);
                    }
                    else
                    {
                        ColorWriteLine("Invalid ID format. Must be a number.", Colors.Error);
                    }
                    break;
                    
                case "add":
                    await AddMemberAsync(parts);
                    break;
                    
                case "remove":
                    await RemoveMemberAsync(parts);
                    break;
                    
                default:
                    ColorWriteLine($"Unknown command: {cmd}. Type 'help' for a list of commands.", Colors.Warning);
                    break;
            }
        }
        
        private static void DisplayHelp()
        {
            ColorWriteLine("\nAvailable commands:", Colors.Secondary);
            Console.WriteLine("  help                                - Display this help message");
            Console.WriteLine("  exit, quit                          - Exit the application");
            Console.WriteLine("  login <username> <password>         - Login with credentials");
            Console.WriteLine("  register <username> <email> <pwd>   - Register a new account");
            Console.WriteLine("  logout                              - Logout from current account");
            Console.WriteLine("  list groups                         - List all groups you're a member of");
            Console.WriteLine("  list users                          - List all users");
            Console.WriteLine("  list messages <groupId>             - List messages in a group");
            Console.WriteLine("  create group <name>                 - Create a group chat (3+ members)");
            Console.WriteLine("  chat <groupId>                      - Enter real-time chat session with a group");
            Console.WriteLine("  chat <userId>                       - Start or continue a personal chat with a user");
            Console.WriteLine("  add <groupId> <userId>              - Add a user to a group (non-personal chats only)");
            Console.WriteLine("  remove <groupId> <userId>           - Remove a user from a group");
            Console.WriteLine();
        }
        
        private static async Task LoginAsync(string[] parts)
        {
            if (_currentUser != null)
            {
                Console.WriteLine("You are already logged in. Please logout first.");
                return;
            }
            
            if (parts.Length < 3)
            {
                Console.WriteLine("Usage: login <username> <password>");
                return;
            }
            
            string username = parts[1];
            string password = parts[2];
            
            try
            {
                var authService = _serviceProvider.GetRequiredService<IAuthService>();
                var userService = _serviceProvider.GetRequiredService<IUserService>();
                var (success, message, token, _) = await authService.LoginAsync(username, password);
                
                if (success)
                {
                    // Get the user from user service
                    var user = userService.GetByUsername(username);
                    if (user != null)
                    {
                        _currentUser = user;
                        ColorWriteLine($"Welcome, {user.Username}!", Colors.Success);
                    }
                    else
                    {
                        ColorWriteLine("Login successful but user details could not be loaded.", Colors.Warning);
                    }
                }
                else
                {
                    ColorWriteLine($"Login failed: {message}", Colors.Error);
                }
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error during login: {ex.Message}", Colors.Error);
            }
        }
        
        private static async Task RegisterAsync(string[] parts)
        {
            if (_currentUser != null)
            {
                ColorWriteLine("You are already logged in. Please logout first to register a new account.", Colors.Warning);
                return;
            }
            
            if (parts.Length < 4)
            {
                ColorWriteLine("Usage: register <username> <email> <password>", Colors.Warning);
                return;
            }
            
            string username = parts[1];
            string email = parts[2];
            string password = parts[3];
            
            try
            {
                var authService = _serviceProvider.GetRequiredService<IAuthService>();
                var user = new User
                {
                    Username = username,
                    Email = email,
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    Role = "User"
                };
                
                var (success, message, _, _) = await authService.RegisterAsync(user, password);
                
                if (success)
                {
                    ColorWriteLine("Registration successful! You can now login.", Colors.Success);
                }
                else
                {
                    ColorWriteLine($"Registration failed: {message}", Colors.Error);
                }
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error during registration: {ex.Message}", Colors.Error);
            }
        }
        
        private static void Logout()
        {
            if (_currentUser == null)
            {
                ColorWriteLine("You are not logged in.", Colors.Warning);
                return;
            }
            
            string username = _currentUser.Username;
            _currentUser = null;
            ColorWriteLine($"Logged out {username} successfully.", Colors.Success);
        }
        
        private static void ListItems(string[] parts)
        {
            if (_currentUser == null)
            {
                ColorWriteLine("You must be logged in to use this command.", Colors.Warning);
                return;
            }
            
            if (parts.Length < 2)
            {
                ColorWriteLine("Usage: list [groups|users|messages <groupId>]", Colors.Warning);
                return;
            }
            
            string itemType = parts[1].ToLower();
            
            switch (itemType)
            {
                case "groups":
                    ListGroups();
                    break;
                    
                case "users":
                    ListUsers();
                    break;
                    
                case "messages":
                    if (parts.Length < 3 || !int.TryParse(parts[2], out int groupId))
                    {
                        Console.WriteLine("Usage: list messages <groupId>");
                        return;
                    }
                    ListMessages(groupId);
                    break;
                    
                default:
                    Console.WriteLine($"Unknown item type: {itemType}. Valid types are: groups, users, messages");
                    break;
            }
        }
        
        private static void ListGroups()
        {
            try
            {
                var threadParticipantService = _serviceProvider.GetRequiredService<IThreadParticipantService>();
                var chatThreadService = _serviceProvider.GetRequiredService<IChatThreadService>();
                
                var participations = threadParticipantService.GetByUserId(_currentUser!.UserId);
                var groupThreadIds = participations.Select(p => p.ThreadId).ToList();
                
                var groupThreads = chatThreadService.GetAll()
                    .Where(t => groupThreadIds.Contains(t.ThreadId) && !t.IsPrivate)
                    .ToList();
                
                if (groupThreads.Count == 0)
                {
                    ColorWriteLine("You are not a member of any groups.", Colors.Warning);
                    return;
                }
                
                ColorWriteLine("\nYour Groups:", Colors.Secondary);
                ColorWriteLine("ID | Name | Created At | Last Activity", Colors.Secondary);
                ColorWriteLine("-------------------------------------------", Colors.Secondary);
                
                foreach (var thread in groupThreads)
                {
                    Console.WriteLine($"{thread.ThreadId} | {thread.Title} | {thread.CreatedAt:g} | {thread.LastMessageAt:g}");
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error listing groups: {ex.Message}", Colors.Error);
            }
        }
        
        private static void ListUsers()
        {
            try
            {
                var userService = _serviceProvider.GetRequiredService<IUserService>();
                var users = userService.GetAll();
                
                ColorWriteLine("\nUsers:", Colors.Secondary);
                ColorWriteLine("ID | Username | Email | Role | Active", Colors.Secondary);
                ColorWriteLine("-------------------------------------------", Colors.Secondary);
                
                foreach (var user in users)
                {
                    Console.WriteLine($"{user.UserId} | {user.Username} | {user.Email} | {user.Role} | {(user.IsActive ? "Yes" : "No")}");
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error listing users: {ex.Message}", Colors.Error);
            }
        }
        
        private static void ListMessages(int groupId)
        {
            try
            {
                var threadParticipantService = _serviceProvider.GetRequiredService<IThreadParticipantService>();
                var chatThreadService = _serviceProvider.GetRequiredService<IChatThreadService>();
                var messageHistoryService = _serviceProvider.GetRequiredService<IMessageHistoryService>();
                var userService = _serviceProvider.GetRequiredService<IUserService>();
                
                // Verify thread exists
                var thread = chatThreadService.GetById(groupId);
                if (thread == null)
                {
                    Console.WriteLine("Group not found.");
                    return;
                }
                
                // Verify user is a participant
                var participants = threadParticipantService.GetByThreadId(groupId);
                var userParticipant = participants.FirstOrDefault(p => p.UserId == _currentUser!.UserId);
                if (userParticipant == null)
                {
                    Console.WriteLine("You are not a member of this group.");
                    return;
                }
                
                // Get messages
                var messages = messageHistoryService.GetByThreadId(groupId);
                
                if (!messages.Any())
                {
                    ColorWriteLine("No messages in this group yet.", Colors.Warning);
                    return;
                }
                
                ColorWriteLine($"\nMessages in {thread.Title}:", Colors.Secondary);
                ColorWriteLine("Time          User          Message", Colors.Secondary);
                ColorWriteLine("---------------------------------------", Colors.Secondary);
                
                foreach (var msg in messages)
                {
                    PrintFormattedMessage(msg, userService);
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error listing messages: {ex.Message}", Colors.Error);
            }
        }
        
        private static void PrintFormattedMessage(MessageHistory msg, IUserService userService)
        {
            var sender = userService.GetById(msg.UserId)?.Username ?? "Unknown";
            string displayMessage = msg.WasModified ? msg.ModeratedMessage! : msg.OriginalMessage;
            
            // Format time
            ColorWrite($"{msg.CreatedAt:HH:mm:ss}  ", Colors.Timestamp);
            
            // Format username with padding to align messages
            string paddedUsername = sender.PadRight(14);
            ColorWrite(paddedUsername, Colors.Username);
            
            // Print the message
            Console.WriteLine(displayMessage);
        }
        
        private static async Task CreateItemAsync(string[] parts)
        {
            if (_currentUser == null)
            {
                ColorWriteLine("You must be logged in to use this command.", Colors.Warning);
                return;
            }
            
            if (parts.Length < 3)
            {
                ColorWriteLine("Usage: create group <name>", Colors.Warning);
                return;
            }
            
            string itemType = parts[1].ToLower();
            
            switch (itemType)
            {
                case "group":
                    await CreateGroupAsync(parts[2]);
                    break;
                    
                default:
                    ColorWriteLine($"Unknown item type: {itemType}. Valid types are: group", Colors.Warning);
                    break;
            }
        }
        
        private static async Task CreateGroupAsync(string name)
        {
            try
            {
                var chatThreadService = _serviceProvider.GetRequiredService<IChatThreadService>();
                var threadParticipantService = _serviceProvider.GetRequiredService<IThreadParticipantService>();
                
                // Create a new thread for this group
                var chatThread = new ChatThread
                {
                    Title = name,
                    IsPrivate = false,  // Group chats are non-private by definition
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true,
                    ModerationEnabled = true
                };
                
                var result = chatThreadService.Add(chatThread);
                if (!result.success)
                {
                    ColorWriteLine($"Failed to create group: {result.message}", Colors.Error);
                    return;
                }
                
                // Add creator as participant
                var creatorParticipant = new ThreadParticipant
                {
                    ThreadId = chatThread.ThreadId,
                    UserId = _currentUser!.UserId,
                    JoinedAt = DateTime.UtcNow
                };
                
                threadParticipantService.Add(creatorParticipant);
                
                ColorWriteLine($"Group '{name}' created successfully with ID {chatThread.ThreadId}", Colors.Success);
                ColorWriteLine($"Remember to add at least 2 more members for a proper group chat.", Colors.Warning);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error creating group: {ex.Message}", Colors.Error);
            }
        }
        
        private static async Task AddMemberAsync(string[] parts)
        {
            if (_currentUser == null)
            {
                ColorWriteLine("You must be logged in to use this command.", Colors.Warning);
                return;
            }
            
            if (parts.Length < 3 || !int.TryParse(parts[1], out int groupId) || !int.TryParse(parts[2], out int userId))
            {
                ColorWriteLine("Usage: add <groupId> <userId>", Colors.Warning);
                return;
            }
            
            try
            {
                var chatThreadService = _serviceProvider.GetRequiredService<IChatThreadService>();
                var threadParticipantService = _serviceProvider.GetRequiredService<IThreadParticipantService>();
                var userService = _serviceProvider.GetRequiredService<IUserService>();
                
                // Verify thread exists
                var thread = chatThreadService.GetById(groupId);
                if (thread == null)
                {
                    ColorWriteLine("Group not found.", Colors.Error);
                    return;
                }
                
                // Check if this is a personal chat (private chat between 2 users)
                if (thread.IsPrivate)
                {
                    var participants = threadParticipantService.GetByThreadId(groupId);
                    if (participants.Count() == 2)
                    {
                        ColorWriteLine("Cannot add users to a personal chat.", Colors.Error);
                        return;
                    }
                }
                
                // Verify current user is a participant
                var participants = threadParticipantService.GetByThreadId(groupId);
                if (!participants.Any(p => p.UserId == _currentUser.UserId))
                {
                    ColorWriteLine("You are not a member of this group.", Colors.Error);
                    return;
                }
                
                // Verify current user is the creator (first participant)
                var firstParticipant = participants.OrderBy(p => p.JoinedAt).FirstOrDefault();
                if (firstParticipant == null || firstParticipant.UserId != _currentUser.UserId)
                {
                    ColorWriteLine("Only the group creator can add members.", Colors.Error);
                    return;
                }
                
                // Verify user to add exists
                var userToAdd = userService.GetById(userId);
                if (userToAdd == null)
                {
                    ColorWriteLine("User not found.", Colors.Error);
                    return;
                }
                
                // Check if already a member
                if (participants.Any(p => p.UserId == userId))
                {
                    ColorWriteLine("User is already a member of this group.", Colors.Warning);
                    return;
                }
                
                // Add user as participant
                var participant = new ThreadParticipant
                {
                    ThreadId = groupId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                };
                
                var result = threadParticipantService.Add(participant);
                
                if (result.success)
                {
                    ColorWriteLine($"Successfully added user '{userToAdd.Username}' to group '{thread.Title}'", Colors.Success);
                }
                else
                {
                    ColorWriteLine($"Failed to add user to group: {result.message}", Colors.Error);
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error adding member to group: {ex.Message}", Colors.Error);
            }
        }
        
        private static async Task RemoveMemberAsync(string[] parts)
        {
            if (_currentUser == null)
            {
                ColorWriteLine("You must be logged in to use this command.", Colors.Warning);
                return;
            }
            
            if (parts.Length < 3 || !int.TryParse(parts[1], out int groupId) || !int.TryParse(parts[2], out int userId))
            {
                ColorWriteLine("Usage: remove <groupId> <userId>", Colors.Warning);
                return;
            }
            
            try
            {
                var chatThreadService = _serviceProvider.GetRequiredService<IChatThreadService>();
                var threadParticipantService = _serviceProvider.GetRequiredService<IThreadParticipantService>();
                var userService = _serviceProvider.GetRequiredService<IUserService>();
                
                // Verify thread exists
                var thread = chatThreadService.GetById(groupId);
                if (thread == null)
                {
                    Console.WriteLine("Group not found.");
                    return;
                }
                
                // Get all participants
                var participants = threadParticipantService.GetByThreadId(groupId);
                var currentUserParticipant = participants.FirstOrDefault(p => p.UserId == _currentUser.UserId);
                
                // Verify current user is a member
                if (currentUserParticipant == null)
                {
                    Console.WriteLine("You are not a member of this group.");
                    return;
                }
                
                // Check if removing self or if user is the first participant (creator)
                var firstParticipant = participants.OrderBy(p => p.JoinedAt).FirstOrDefault();
                var isCreator = firstParticipant != null && firstParticipant.UserId == _currentUser.UserId;
                
                // Only allow self-removal or creator removing others
                if (_currentUser.UserId != userId && !isCreator)
                {
                    Console.WriteLine("Only the group creator can remove other members.");
                    return;
                }
                
                // Find the participant to remove
                var participantToRemove = participants.FirstOrDefault(p => p.UserId == userId);
                if (participantToRemove == null)
                {
                    Console.WriteLine("User is not a member of this group.");
                    return;
                }
                
                // Use RemoveUserFromThread method
                var result = threadParticipantService.RemoveUserFromThread(userId, groupId);
                
                if (result)
                {
                    var removedUser = userService.GetById(userId);
                    Console.WriteLine($"Successfully removed user '{removedUser?.Username ?? userId.ToString()}' from group '{thread.Title}'");
                }
                else
                {
                    Console.WriteLine("Failed to remove member from group.");
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing member from group: {ex.Message}");
            }
        }
        
        private static async Task EnterChatSessionAsync(int groupId)
        {
            if (_currentUser == null)
            {
                ColorWriteLine("You must be logged in to use this command.", Colors.Error);
                return;
            }
            
            try
            {
                var chatThreadService = _serviceProvider.GetRequiredService<IChatThreadService>();
                var threadParticipantService = _serviceProvider.GetRequiredService<IThreadParticipantService>();
                var messageHistoryService = _serviceProvider.GetRequiredService<IMessageHistoryService>();
                var userService = _serviceProvider.GetRequiredService<IUserService>();
                
                // Verify thread exists
                var thread = chatThreadService.GetById(groupId);
                if (thread == null)
                {
                    ColorWriteLine("Chat group not found.", Colors.Error);
                    return;
                }
                
                // Verify user is a participant
                var participants = threadParticipantService.GetByThreadId(groupId);
                if (!participants.Any(p => p.UserId == _currentUser.UserId))
                {
                    ColorWriteLine("You are not a member of this chat.", Colors.Error);
                    return;
                }
                
                // Get chat type for display
                string chatType = thread.IsPrivate && participants.Count() == 2 ? "Personal chat" : "Group chat";
                
                // Clear the console for a clean chat interface
                Console.Clear();
                ColorWriteLine($"=== {chatType}: {thread.Title} ===", Colors.Primary);
                ColorWriteLine("Type your message and press Enter to send. Type /exit to leave the chat.", Colors.Secondary);
                ColorWriteLine("---------------------------------------", Colors.Secondary);
                
                // Display most recent messages first
                await DisplayRecentMessagesAsync(groupId);
                
                // Start chat session
                bool inChatSession = true;
                DateTime lastCheck = DateTime.UtcNow;
                
                // Use a cancellation token to handle the background polling task
                using var cts = new CancellationTokenSource();
                var pollingTask = StartMessagePollingAsync(groupId, lastCheck, cts.Token);
                
                while (inChatSession)
                {
                    // Display prompt
                    ColorWrite($"{_currentUser.Username}> ", Colors.Primary);
                    string userInput = Console.ReadLine() ?? string.Empty;
                    
                    // Check if user wants to exit
                    if (userInput.Trim().ToLower() == "/exit")
                    {
                        inChatSession = false;
                        cts.Cancel();
                        ColorWriteLine("Exiting chat session...", Colors.SystemMessage);
                        continue;
                    }
                    
                    // Send the message if not empty
                    if (!string.IsNullOrWhiteSpace(userInput))
                    {
                        // Create message
                        var messageHistory = new MessageHistory
                        {
                            ThreadId = groupId,
                            UserId = _currentUser.UserId,
                            OriginalMessage = userInput,
                            ModeratedMessage = userInput,
                            WasModified = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        // Send message
                        var result = messageHistoryService.Add(messageHistory);
                        if (!result.success)
                        {
                            ColorWriteLine($"Error sending message: {result.message}", Colors.Error);
                        }
                    }
                }
                
                // Wait for the polling task to complete
                try
                {
                    await pollingTask;
                }
                catch (OperationCanceledException)
                {
                    // This is expected when we cancel the task
                }
                
                ColorWriteLine("Returned to command mode. Type 'help' for available commands.", Colors.SystemMessage);
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error in chat session: {ex.Message}", Colors.Error);
            }
        }
        
        private static async Task DisplayRecentMessagesAsync(int groupId)
        {
            var messageHistoryService = _serviceProvider.GetRequiredService<IMessageHistoryService>();
            var userService = _serviceProvider.GetRequiredService<IUserService>();
            
            // Get recent messages (last 20)
            var messages = messageHistoryService.GetByThreadId(groupId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(20)
                .OrderBy(m => m.CreatedAt)
                .ToList();
            
            foreach (var msg in messages)
            {
                PrintFormattedMessage(msg, userService);
            }
        }
        
        private static async Task StartMessagePollingAsync(int groupId, DateTime lastCheck, CancellationToken cancellationToken)
        {
            var messageHistoryService = _serviceProvider.GetRequiredService<IMessageHistoryService>();
            var userService = _serviceProvider.GetRequiredService<IUserService>();
            
            // Get the configuration for polling interval
            int pollingIntervalMs = 2000; // Default to 2 seconds
            
            try
            {
                var config = _serviceProvider.GetRequiredService<IConfiguration>();
                if (int.TryParse(config["CLISettings:MessagePollingIntervalMs"], out int configInterval))
                {
                    pollingIntervalMs = configInterval;
                }
            }
            catch
            {
                // Use default if configuration is not available
            }
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Wait for the polling interval
                    await Task.Delay(pollingIntervalMs, cancellationToken);
                    
                    // Get new messages since last check
                    var messages = messageHistoryService.GetByThreadId(groupId)
                        .Where(m => m.CreatedAt > lastCheck && m.UserId != _currentUser!.UserId)
                        .OrderBy(m => m.CreatedAt)
                        .ToList();
                    
                    if (messages.Any())
                    {
                        // Get current cursor position to restore it after displaying messages
                        int currentLeft = Console.CursorLeft;
                        int currentTop = Console.CursorTop;
                        
                        // Clear the current line if the user was typing
                        if (currentLeft > 0)
                        {
                            Console.SetCursorPosition(0, currentTop);
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.SetCursorPosition(0, currentTop);
                        }
                        
                        // Display new messages
                        foreach (var msg in messages)
                        {
                            PrintFormattedMessage(msg, userService);
                        }
                        
                        // Redisplay the prompt and whatever the user was typing
                        ColorWrite($"{_currentUser.Username}> ", Colors.Primary);
                    }
                    
                    // Update the last check time
                    lastCheck = DateTime.UtcNow;
                }
                catch (OperationCanceledException)
                {
                    // This is expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    // Log the error but continue polling
                    ColorWriteLine($"Error checking for messages: {ex.Message}", Colors.Error);
                }
            }
        }
        
        // Method to find or create a personal chat between current user and another user
        private static async Task<int> FindOrCreatePersonalChatAsync(int otherUserId)
        {
            if (_currentUser == null)
            {
                ColorWriteLine("You must be logged in to use this command.", Colors.Error);
                return -1;
            }
            
            if (_currentUser.UserId == otherUserId)
            {
                ColorWriteLine("You cannot start a chat with yourself.", Colors.Error);
                return -1;
            }
            
            try
            {
                var chatThreadService = _serviceProvider.GetRequiredService<IChatThreadService>();
                var threadParticipantService = _serviceProvider.GetRequiredService<IThreadParticipantService>();
                var userService = _serviceProvider.GetRequiredService<IUserService>();
                
                // Check if other user exists
                var otherUser = userService.GetById(otherUserId);
                if (otherUser == null)
                {
                    ColorWriteLine("User not found.", Colors.Error);
                    return -1;
                }
                
                // Find active personal chats where both users are participants
                var currentUserThreads = threadParticipantService.GetByUserId(_currentUser.UserId);
                var otherUserThreads = threadParticipantService.GetByUserId(otherUserId);
                
                // Get the intersection of thread IDs
                var sharedThreadIds = currentUserThreads.Select(t => t.ThreadId)
                    .Intersect(otherUserThreads.Select(t => t.ThreadId))
                    .ToList();
                
                // Find personal chats among shared threads
                foreach (var threadId in sharedThreadIds)
                {
                    var thread = chatThreadService.GetById(threadId);
                    if (thread != null && thread.IsPrivate && thread.IsActive)
                    {
                        // Check if this is a 2-person chat
                        var participants = threadParticipantService.GetByThreadId(threadId);
                        if (participants.Count() == 2)
                        {
                            ColorWriteLine($"Continuing existing chat with {otherUser.Username}...", Colors.Success);
                            return threadId;
                        }
                    }
                }
                
                // No existing personal chat found, create a new one
                var chatTitle = $"Chat between {_currentUser.Username} and {otherUser.Username}";
                var newThread = new ChatThread
                {
                    Title = chatTitle,
                    IsPrivate = true,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true,
                    ModerationEnabled = true
                };
                
                var result = chatThreadService.Add(newThread);
                if (!result.success)
                {
                    ColorWriteLine($"Failed to create chat: {result.message}", Colors.Error);
                    return -1;
                }
                
                // Add both users as participants
                var currentUserParticipant = new ThreadParticipant
                {
                    ThreadId = newThread.ThreadId,
                    UserId = _currentUser.UserId,
                    JoinedAt = DateTime.UtcNow
                };
                
                var otherUserParticipant = new ThreadParticipant
                {
                    ThreadId = newThread.ThreadId,
                    UserId = otherUserId,
                    JoinedAt = DateTime.UtcNow
                };
                
                threadParticipantService.Add(currentUserParticipant);
                threadParticipantService.Add(otherUserParticipant);
                
                ColorWriteLine($"Started new chat with {otherUser.Username}.", Colors.Success);
                return newThread.ThreadId;
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error creating personal chat: {ex.Message}", Colors.Error);
                return -1;
            }
        }
    }
}
