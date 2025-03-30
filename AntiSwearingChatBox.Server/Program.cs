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

        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== AntiSwearingChatBox CLI ===");
            
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
            Console.Write($"{prompt}> ");
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
                    Console.WriteLine("Goodbye!");
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
                    
                case "join":
                    await JoinGroupAsync(parts);
                    break;
                    
                case "msg":
                case "send":
                    await SendMessageAsync(parts);
                    break;
                    
                case "add":
                    await AddMemberAsync(parts);
                    break;
                    
                case "remove":
                    await RemoveMemberAsync(parts);
                    break;
                    
                case "chat":
                    if (parts.Length < 2 || !int.TryParse(parts[1], out int chatGroupId))
                    {
                        Console.WriteLine("Usage: chat <groupId>");
                        return;
                    }
                    await EnterChatSessionAsync(chatGroupId);
                    break;
                    
                default:
                    Console.WriteLine($"Unknown command: {cmd}. Type 'help' for a list of commands.");
                    break;
            }
        }
        
        private static void DisplayHelp()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  help                                - Display this help message");
            Console.WriteLine("  exit, quit                          - Exit the application");
            Console.WriteLine("  login <username> <password>         - Login with credentials");
            Console.WriteLine("  register <username> <email> <pwd>   - Register a new account");
            Console.WriteLine("  logout                              - Logout from current account");
            Console.WriteLine("  list groups                         - List all groups you're a member of");
            Console.WriteLine("  list users                          - List all users");
            Console.WriteLine("  list messages <groupId>             - List messages in a group");
            Console.WriteLine("  create group <name>                 - Create a new group");
            Console.WriteLine("  join <groupId>                      - Join a group");
            Console.WriteLine("  msg <groupId> <message>             - Send a message to a group");
            Console.WriteLine("  chat <groupId>                      - Enter real-time chat session");
            Console.WriteLine("  add <groupId> <userId>              - Add a user to a group");
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
                        Console.WriteLine($"Welcome, {user.Username}!");
                    }
                    else
                    {
                        Console.WriteLine("Login successful but user details could not be loaded.");
                    }
                }
                else
                {
                    Console.WriteLine($"Login failed: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
            }
        }
        
        private static async Task RegisterAsync(string[] parts)
        {
            if (_currentUser != null)
            {
                Console.WriteLine("You are already logged in. Please logout first to register a new account.");
                return;
            }
            
            if (parts.Length < 4)
            {
                Console.WriteLine("Usage: register <username> <email> <password>");
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
                    Console.WriteLine("Registration successful! You can now login.");
                }
                else
                {
                    Console.WriteLine($"Registration failed: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
            }
        }
        
        private static void Logout()
        {
            if (_currentUser == null)
            {
                Console.WriteLine("You are not logged in.");
                return;
            }
            
            string username = _currentUser.Username;
            _currentUser = null;
            Console.WriteLine($"Logged out {username} successfully.");
        }
        
        private static void ListItems(string[] parts)
        {
            if (_currentUser == null)
            {
                Console.WriteLine("You must be logged in to use this command.");
                return;
            }
            
            if (parts.Length < 2)
            {
                Console.WriteLine("Usage: list [groups|users|messages <groupId>]");
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
                    Console.WriteLine("You are not a member of any groups.");
                    return;
                }
                
                Console.WriteLine("\nYour Groups:");
                Console.WriteLine("ID | Name | Created At | Last Activity");
                Console.WriteLine("-------------------------------------------");
                
                foreach (var thread in groupThreads)
                {
                    Console.WriteLine($"{thread.ThreadId} | {thread.Title} | {thread.CreatedAt:g} | {thread.LastMessageAt:g}");
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing groups: {ex.Message}");
            }
        }
        
        private static void ListUsers()
        {
            try
            {
                var userService = _serviceProvider.GetRequiredService<IUserService>();
                var users = userService.GetAll();
                
                Console.WriteLine("\nUsers:");
                Console.WriteLine("ID | Username | Email | Role | Active");
                Console.WriteLine("-------------------------------------------");
                
                foreach (var user in users)
                {
                    Console.WriteLine($"{user.UserId} | {user.Username} | {user.Email} | {user.Role} | {(user.IsActive ? "Yes" : "No")}");
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing users: {ex.Message}");
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
                    Console.WriteLine("No messages in this group yet.");
                    return;
                }
                
                Console.WriteLine($"\nMessages in {thread.Title}:");
                Console.WriteLine("Time | User | Message");
                Console.WriteLine("-------------------------------------------");
                
                foreach (var msg in messages)
                {
                    var sender = userService.GetById(msg.UserId)?.Username ?? "Unknown";
                    string displayMessage = msg.WasModified ? msg.ModeratedMessage! : msg.OriginalMessage;
                    Console.WriteLine($"{msg.CreatedAt:g} | {sender}: {displayMessage}");
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing messages: {ex.Message}");
            }
        }
        
        private static async Task CreateItemAsync(string[] parts)
        {
            if (_currentUser == null)
            {
                Console.WriteLine("You must be logged in to use this command.");
                return;
            }
            
            if (parts.Length < 3)
            {
                Console.WriteLine("Usage: create group <name>");
                return;
            }
            
            string itemType = parts[1].ToLower();
            
            switch (itemType)
            {
                case "group":
                    await CreateGroupAsync(parts[2]);
                    break;
                    
                default:
                    Console.WriteLine($"Unknown item type: {itemType}. Valid types are: group");
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
                    IsPrivate = false,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true,
                    ModerationEnabled = true
                };
                
                var result = chatThreadService.Add(chatThread);
                if (!result.success)
                {
                    Console.WriteLine($"Failed to create group: {result.message}");
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
                
                Console.WriteLine($"Group '{name}' created successfully with ID {chatThread.ThreadId}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating group: {ex.Message}");
            }
        }
        
        private static async Task JoinGroupAsync(string[] parts)
        {
            if (_currentUser == null)
            {
                Console.WriteLine("You must be logged in to use this command.");
                return;
            }
            
            if (parts.Length < 2 || !int.TryParse(parts[1], out int groupId))
            {
                Console.WriteLine("Usage: join <groupId>");
                return;
            }
            
            try
            {
                var chatThreadService = _serviceProvider.GetRequiredService<IChatThreadService>();
                var threadParticipantService = _serviceProvider.GetRequiredService<IThreadParticipantService>();
                
                // Verify thread exists
                var thread = chatThreadService.GetById(groupId);
                if (thread == null)
                {
                    Console.WriteLine("Group not found.");
                    return;
                }
                
                // Check if already a member
                var participants = threadParticipantService.GetByThreadId(groupId);
                if (participants.Any(p => p.UserId == _currentUser.UserId))
                {
                    Console.WriteLine("You are already a member of this group.");
                    return;
                }
                
                // Add user as participant
                var participant = new ThreadParticipant
                {
                    ThreadId = groupId,
                    UserId = _currentUser.UserId,
                    JoinedAt = DateTime.UtcNow
                };
                
                var result = threadParticipantService.Add(participant);
                
                if (result.success)
                {
                    Console.WriteLine($"Successfully joined group '{thread.Title}'");
                }
                else
                {
                    Console.WriteLine($"Failed to join group: {result.message}");
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error joining group: {ex.Message}");
            }
        }
        
        private static async Task SendMessageAsync(string[] parts)
        {
            if (_currentUser == null)
            {
                Console.WriteLine("You must be logged in to use this command.");
                return;
            }
            
            if (parts.Length < 3 || !int.TryParse(parts[1], out int groupId))
            {
                Console.WriteLine("Usage: msg <groupId> <message>");
                return;
            }
            
            // Combine remaining parts as the message content
            string message = string.Join(" ", parts.Skip(2));
            
            try
            {
                var chatThreadService = _serviceProvider.GetRequiredService<IChatThreadService>();
                var threadParticipantService = _serviceProvider.GetRequiredService<IThreadParticipantService>();
                var messageHistoryService = _serviceProvider.GetRequiredService<IMessageHistoryService>();
                
                // Verify thread exists
                var thread = chatThreadService.GetById(groupId);
                if (thread == null)
                {
                    Console.WriteLine("Group not found.");
                    return;
                }
                
                // Verify user is a participant
                var participants = threadParticipantService.GetByThreadId(groupId);
                if (!participants.Any(p => p.UserId == _currentUser.UserId))
                {
                    Console.WriteLine("You are not a member of this group.");
                    return;
                }
                
                // Create and store message
                var messageHistory = new MessageHistory
                {
                    ThreadId = groupId,
                    UserId = _currentUser.UserId,
                    OriginalMessage = message,
                    ModeratedMessage = message, 
                    WasModified = false,
                    CreatedAt = DateTime.UtcNow
                };
                
                var result = messageHistoryService.Add(messageHistory);
                
                if (result.success)
                {
                    // Update the last message timestamp for the thread
                    thread.LastMessageAt = DateTime.UtcNow;
                    chatThreadService.Update(thread);
                    
                    Console.WriteLine("Message sent successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to send message: {result.message}");
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
        
        private static async Task AddMemberAsync(string[] parts)
        {
            if (_currentUser == null)
            {
                Console.WriteLine("You must be logged in to use this command.");
                return;
            }
            
            if (parts.Length < 3 || !int.TryParse(parts[1], out int groupId) || !int.TryParse(parts[2], out int userId))
            {
                Console.WriteLine("Usage: add <groupId> <userId>");
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
                
                // Verify current user is a participant
                var participants = threadParticipantService.GetByThreadId(groupId);
                if (!participants.Any(p => p.UserId == _currentUser.UserId))
                {
                    Console.WriteLine("You are not a member of this group.");
                    return;
                }
                
                // Verify current user is the creator (first participant)
                var firstParticipant = participants.OrderBy(p => p.JoinedAt).FirstOrDefault();
                if (firstParticipant == null || firstParticipant.UserId != _currentUser.UserId)
                {
                    Console.WriteLine("Only the group creator can add members.");
                    return;
                }
                
                // Verify user to add exists
                var userToAdd = userService.GetById(userId);
                if (userToAdd == null)
                {
                    Console.WriteLine("User not found.");
                    return;
                }
                
                // Check if already a member
                if (participants.Any(p => p.UserId == userId))
                {
                    Console.WriteLine("User is already a member of this group.");
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
                    Console.WriteLine($"Successfully added user '{userToAdd.Username}' to group '{thread.Title}'");
                }
                else
                {
                    Console.WriteLine($"Failed to add user to group: {result.message}");
                }
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding member to group: {ex.Message}");
            }
        }
        
        private static async Task RemoveMemberAsync(string[] parts)
        {
            if (_currentUser == null)
            {
                Console.WriteLine("You must be logged in to use this command.");
                return;
            }
            
            if (parts.Length < 3 || !int.TryParse(parts[1], out int groupId) || !int.TryParse(parts[2], out int userId))
            {
                Console.WriteLine("Usage: remove <groupId> <userId>");
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
                Console.WriteLine("You must be logged in to use this command.");
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
                    Console.WriteLine("Group not found.");
                    return;
                }
                
                // Verify user is a participant
                var participants = threadParticipantService.GetByThreadId(groupId);
                if (!participants.Any(p => p.UserId == _currentUser.UserId))
                {
                    Console.WriteLine("You are not a member of this group.");
                    return;
                }
                
                // Clear the console for a clean chat interface
                Console.Clear();
                Console.WriteLine($"=== Real-time chat: {thread.Title} ===");
                Console.WriteLine("Type your message and press Enter to send. Type /exit to leave the chat.");
                Console.WriteLine("-------------------------------------------");
                
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
                    Console.Write($"{_currentUser.Username}> ");
                    string userInput = Console.ReadLine() ?? string.Empty;
                    
                    // Check if user wants to exit
                    if (userInput.Trim().ToLower() == "/exit")
                    {
                        inChatSession = false;
                        cts.Cancel();
                        Console.WriteLine("Exiting chat session...");
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
                        if (result.success)
                        {
                            // Update thread's LastMessageAt
                            thread.LastMessageAt = DateTime.UtcNow;
                            chatThreadService.Update(thread);
                        }
                        else
                        {
                            Console.WriteLine($"Error sending message: {result.message}");
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
                
                Console.WriteLine("Returned to command mode. Type 'help' for available commands.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in chat session: {ex.Message}");
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
                var sender = userService.GetById(msg.UserId)?.Username ?? "Unknown";
                string displayMessage = msg.WasModified ? msg.ModeratedMessage! : msg.OriginalMessage;
                Console.WriteLine($"[{msg.CreatedAt:HH:mm:ss}] {sender}: {displayMessage}");
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
                            var sender = userService.GetById(msg.UserId)?.Username ?? "Unknown";
                            string displayMessage = msg.WasModified ? msg.ModeratedMessage! : msg.OriginalMessage;
                            Console.WriteLine($"[{msg.CreatedAt:HH:mm:ss}] {sender}: {displayMessage}");
                        }
                        
                        // Redisplay the prompt and whatever the user was typing
                        Console.Write($"{_currentUser.Username}> ");
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
                    Console.WriteLine($"Error checking for messages: {ex.Message}");
                }
            }
        }
    }
}
