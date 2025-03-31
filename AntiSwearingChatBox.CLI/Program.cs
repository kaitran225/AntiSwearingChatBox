using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.CLI
{
    class Program
    {
        private static ApiClient? _apiClient;
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

        static async Task Main(string[] args)
        {
            // Set console colors
            Console.BackgroundColor = Colors.Background;
            Console.ForegroundColor = Colors.NormalText;
            Console.Clear();
            
            ColorWriteLine("=== AntiSwearingChatBox CLI Client ===", Colors.Primary);
            
            // Initialize API client
            string apiUrl = "http://localhost:5000"; // Adjust this URL to match your API server

            // Try to read from appsettings.json if available
            try
            {
                // If we had an appsettings.json, we would read the URL from there
            }
            catch
            {
                ColorWriteLine($"Using default API URL: {apiUrl}", Colors.Warning);
            }

            _apiClient = new ApiClient(apiUrl);
            
            // Start command loop
            while (_isRunning)
            {
                DisplayPrompt();
                string command = Console.ReadLine() ?? string.Empty;
                await ProcessCommandAsync(command);
            }
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
        
        private static void DisplayPrompt()
        {
            string prompt = _apiClient?.IsAuthenticated == true ? _apiClient.CurrentUser?.Username ?? "Guest" : "Guest";
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
                    await ListItemsAsync(parts);
                    break;
                    
                case "create":
                    await CreateItemAsync(parts);
                    break;
                    
                case "chat":
                    if (parts.Length < 2)
                    {
                        ColorWriteLine("Usage: chat <groupId|username>", Colors.Warning);
                        return;
                    }
                    
                    if (int.TryParse(parts[1], out int chatId))
                    {
                        // Numeric ID - treat as a group ID
                        await EnterChatAsync(chatId);
                    }
                    else
                    {
                        // String - treat as a username
                        string targetName = parts[1];
                        await EnterChatAsyncByUsername(targetName);
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
            Console.WriteLine("  chat <username>                     - Start or continue a personal chat with a user");
            Console.WriteLine("  add <groupId> <userId>              - Add a user to a group (non-personal chats only)");
            Console.WriteLine("  remove <groupId> <userId>           - Remove a user from a group");
            Console.WriteLine();
        }
        
        private static async Task LoginAsync(string[] parts)
        {
            if (_apiClient?.IsAuthenticated == true)
            {
                ColorWriteLine("You are already logged in. Please logout first.", Colors.Warning);
                return;
            }
            
            if (parts.Length < 3)
            {
                ColorWriteLine("Usage: login <username> <password>", Colors.Warning);
                return;
            }
            
            string username = parts[1];
            string password = parts[2];
            
            try
            {
                var (success, message, user) = await _apiClient!.LoginAsync(username, password);
                
                if (success && user != null)
                {
                    ColorWriteLine($"Welcome, {user.Username ?? "User"}!", Colors.Success);
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
            if (_apiClient?.IsAuthenticated == true)
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
                var (success, message) = await _apiClient!.RegisterAsync(username, email, password);
                
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
            if (_apiClient?.IsAuthenticated != true)
            {
                ColorWriteLine("You are not logged in.", Colors.Warning);
                return;
            }
            
            string username = _apiClient.CurrentUser?.Username ?? "User";
            _apiClient.Logout();
            ColorWriteLine($"Logged out {username} successfully.", Colors.Success);
        }
        
        private static async Task ListItemsAsync(string[] parts)
        {
            if (_apiClient?.IsAuthenticated != true)
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
                    await ListGroupsAsync();
                    break;
                    
                case "users":
                    await ListUsersAsync();
                    break;
                    
                case "messages":
                    if (parts.Length < 3 || !int.TryParse(parts[2], out int groupId))
                    {
                        ColorWriteLine("Usage: list messages <groupId>", Colors.Warning);
                        return;
                    }
                    await ListMessagesAsync(groupId);
                    break;
                    
                default:
                    ColorWriteLine($"Unknown item type: {itemType}. Valid types are: groups, users, messages", Colors.Warning);
                    break;
            }
        }
        
        private static async Task ListGroupsAsync()
        {
            try
            {
                var threads = await _apiClient!.GetUserThreadsAsync(_apiClient.CurrentUser?.UserId ?? 0) ?? Array.Empty<ChatThread>();
                
                // Filter non-private threads (group chats)
                var groupThreads = threads.Where(t => !t.IsPrivate).ToList();
                
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
                    Console.WriteLine($"{thread.ThreadId} | {thread.Title ?? "Unnamed"} | {thread.CreatedAt:g} | {thread.LastMessageAt:g}");
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error listing groups: {ex.Message}", Colors.Error);
            }
        }
        
        private static async Task ListUsersAsync()
        {
            try
            {
                var users = await _apiClient?.GetAllUsersAsync()! ?? Array.Empty<User>();
                
                ColorWriteLine("\nUsers:", Colors.Secondary);
                ColorWriteLine("ID | Username | Email | Role | Active", Colors.Secondary);
                ColorWriteLine("-------------------------------------------", Colors.Secondary);
                
                foreach (var user in users)
                {
                    Console.WriteLine($"{user.UserId} | {user.Username ?? "Unknown"} | {user.Email ?? "No email"} | {user.Role ?? "User"} | {(user.IsActive ? "Yes" : "No")}");
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error listing users: {ex.Message}", Colors.Error);
            }
        }
        
        private static async Task ListMessagesAsync(int groupId)
        {
            try
            {
                // First verify thread exists and user is a participant
                var thread = await _apiClient!.GetThreadByIdAsync(groupId);
                if (thread == null)
                {
                    ColorWriteLine("Group not found.", Colors.Error);
                    return;
                }
                
                // Get messages
                var messages = await _apiClient!.GetThreadMessagesAsync(groupId) ?? Array.Empty<EnrichedMessage>();
                
                if (messages.Length == 0)
                {
                    ColorWriteLine("No messages in this group yet.", Colors.Warning);
                    return;
                }
                
                ColorWriteLine($"\nMessages in {thread.Title ?? "Unnamed Chat"}:", Colors.Secondary);
                ColorWriteLine("Time          User          Message", Colors.Secondary);
                ColorWriteLine("---------------------------------------", Colors.Secondary);
                
                foreach (var msg in messages)
                {
                    PrintFormattedMessage(msg);
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error listing messages: {ex.Message}", Colors.Error);
            }
        }
        
        private static void PrintFormattedMessage(EnrichedMessage? msg)
        {
            if (msg == null)
            {
                Console.WriteLine("(null message)");
                return;
            }
            
            var sender = msg.User?.Username ?? "Unknown";
            string displayMessage = msg.Message?.WasModified == true 
                ? msg.Message.ModeratedMessage ?? "No message content" 
                : msg.Message?.OriginalMessage ?? "No message content";
            
            // Format time
            ColorWrite($"{msg.Message?.CreatedAt.ToString("HH:mm:ss") ?? "??:??:??"}  ", Colors.Timestamp);
            
            // Format username with padding to align messages
            string paddedUsername = sender.PadRight(14);
            ColorWrite(paddedUsername, Colors.Username);
            
            // Print the message
            Console.WriteLine(displayMessage);
        }
        
        private static async Task CreateItemAsync(string[] parts)
        {
            if (_apiClient?.IsAuthenticated != true)
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
                var (success, message, thread) = await _apiClient!.CreateThreadAsync(
                    name, false, _apiClient.CurrentUser?.UserId ?? 0);
                
                if (success && thread != null)
                {
                    ColorWriteLine($"Group '{name}' created successfully with ID {thread.ThreadId}", Colors.Success);
                    ColorWriteLine($"Remember to add at least 2 more members for a proper group chat.", Colors.Warning);
                }
                else
                {
                    ColorWriteLine($"Failed to create group: {message}", Colors.Error);
                }
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error creating group: {ex.Message}", Colors.Error);
            }
        }
        
        private static async Task AddMemberAsync(string[] parts)
        {
            if (_apiClient?.IsAuthenticated != true)
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
                var (success, message) = await _apiClient.AddParticipantAsync(
                    groupId, userId, _apiClient.CurrentUser?.UserId ?? 0);
                
                if (success)
                {
                    ColorWriteLine("User added successfully to the group.", Colors.Success);
                }
                else
                {
                    ColorWriteLine($"Failed to add user: {message}", Colors.Error);
                }
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error adding member to group: {ex.Message}", Colors.Error);
            }
        }
        
        private static async Task RemoveMemberAsync(string[] parts)
        {
            if (_apiClient?.IsAuthenticated != true)
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
                var (success, message) = await _apiClient.RemoveParticipantAsync(
                    groupId, userId, _apiClient.CurrentUser?.UserId ?? 0);
                
                if (success)
                {
                    ColorWriteLine("User removed successfully from the group.", Colors.Success);
                }
                else
                {
                    ColorWriteLine($"Failed to remove user: {message}", Colors.Error);
                }
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error removing member from group: {ex.Message}", Colors.Error);
            }
        }
        
        private static async Task EnterChatAsync(int chatId)
        {
            if (_apiClient?.IsAuthenticated != true)
            {
                ColorWriteLine("You must be logged in to use this command.", Colors.Error);
                return;
            }
            
            try
            {
                // We're treating this as a thread ID for a group chat
                var thread = await _apiClient.GetThreadByIdAsync(chatId);
                if (thread == null)
                {
                    ColorWriteLine("Chat group not found.", Colors.Error);
                    return;
                }
                
                // Verify the user is a participant
                var participants = await _apiClient.GetThreadParticipantsAsync(chatId);
                if (!participants.Any(p => p?.User?.UserId == _apiClient.CurrentUser?.UserId))
                {
                    ColorWriteLine("You are not a member of this chat group.", Colors.Error);
                    return;
                }
                
                await EnterChatSessionAsync(chatId);
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error entering chat: {ex.Message}", Colors.Error);
            }
        }
        
        private static async Task EnterChatSessionAsync(int threadId)
        {
            try
            {
                // Get thread details
                var thread = await _apiClient!.GetThreadByIdAsync(threadId);
                if (thread == null)
                {
                    ColorWriteLine("Chat not found.", Colors.Error);
                    return;
                }
                
                // Get participants
                var participants = await _apiClient.GetThreadParticipantsAsync(threadId);
                
                // Check if current user is a participant
                if (!participants.Any(p => p?.User?.UserId == _apiClient?.CurrentUser?.UserId))
                {
                    ColorWriteLine("You are not a member of this chat.", Colors.Error);
                    return;
                }
                
                // Get chat type for display
                string chatType = thread.IsPrivate && participants.Length == 2 ? "Personal chat" : "Group chat";
                
                // Clear the console for a clean chat interface
                Console.Clear();
                ColorWriteLine($"=== {chatType}: {thread.Title ?? "Unnamed Chat"} ===", Colors.Primary);
                ColorWriteLine("Type your message and press Enter to send. Type /exit to leave the chat.", Colors.Secondary);
                ColorWriteLine("---------------------------------------", Colors.Secondary);
                
                // Display recent messages
                var recentMessages = await _apiClient.GetThreadMessagesAsync(threadId);
                foreach (var msg in recentMessages.TakeLast(20))
                {
                    PrintFormattedMessage(msg);
                }
                
                // Start chat session
                bool inChatSession = true;
                DateTime lastMessageTime = DateTime.UtcNow;
                
                // Use a cancellation token to handle the background polling task
                using var cts = new CancellationTokenSource();
                var pollingTask = StartMessagePollingAsync(threadId, lastMessageTime, cts.Token);
                
                while (inChatSession)
                {
                    // Display prompt
                    ColorWrite($"{_apiClient?.CurrentUser?.Username ?? "User"}> ", Colors.Primary);
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
                        var (success, message, _) = await _apiClient!.SendMessageAsync(
                            threadId, _apiClient?.CurrentUser?.UserId ?? 0, userInput);
                        
                        if (!success)
                        {
                            ColorWriteLine($"Error sending message: {message}", Colors.Error);
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
        
        private static async Task StartMessagePollingAsync(int threadId, DateTime lastMessageTime, CancellationToken cancellationToken)
        {
            if (_apiClient == null) return;
            
            // Get the polling interval from configuration or use default
            int pollingIntervalMs = 1000; // Default to 1 second
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Wait for the polling interval
                    await Task.Delay(pollingIntervalMs, cancellationToken);
                    
                    // Get messages
                    var allMessages = await _apiClient.GetThreadMessagesAsync(threadId) ?? Array.Empty<EnrichedMessage>();
                    
                    // Find new messages from other users
                    var newMessages = allMessages
                        .Where(m => m?.Message?.CreatedAt > lastMessageTime && 
                               m?.User?.UserId != _apiClient.CurrentUser?.UserId)
                        .OrderBy(m => m?.Message?.CreatedAt)
                        .ToList();
                    
                    if (newMessages.Any())
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
                        foreach (var msg in newMessages)
                        {
                            PrintFormattedMessage(msg);
                        }
                        
                        // Redisplay the prompt
                        ColorWrite($"{_apiClient.CurrentUser?.Username ?? "User"}> ", Colors.Primary);
                        
                        // Update the last message time
                        var lastMsg = newMessages.LastOrDefault();
                        if (lastMsg?.Message?.CreatedAt != null)
                        {
                            lastMessageTime = lastMsg.Message.CreatedAt;
                        }
                    }
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
                    await Task.Delay(pollingIntervalMs * 2, cancellationToken); // Wait longer on error
                }
            }
        }
        
        private static async Task EnterChatAsyncByUsername(string username)
        {
            if (_apiClient?.IsAuthenticated != true)
            {
                ColorWriteLine("You must be logged in to use this command.", Colors.Error);
                return;
            }
            
            try
            {
                // Get all users and check if the username exists
                var users = await _apiClient.GetAllUsersAsync();
                var targetUser = users.FirstOrDefault(u => u?.Username?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);
                
                if (targetUser == null)
                {
                    // Try to find a chat group with this name
                    var (success, found, matchingThreads) = await _apiClient.FindThreadsByNameAsync(username, _apiClient.CurrentUser?.UserId ?? 0);
                    
                    if (success && found && matchingThreads?.Length > 0)
                    {
                        // If there are multiple matches, take the exact match if available, otherwise take the first match
                        var exactMatch = matchingThreads.FirstOrDefault(t => t?.Title?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);
                        var threadToUse = exactMatch ?? matchingThreads[0];
                        
                        ColorWriteLine($"Found group chat with name '{threadToUse.Title}'.", Colors.Success);
                        await EnterChatSessionAsync(threadToUse.ThreadId);
                        return;
                    }
                    
                    ColorWriteLine($"No user or chat group found with name '{username}'.", Colors.Error);
                    return;
                }
                
                // It's a user, find or create personal chat
                var (chatSuccess, chatFound, thread) = await _apiClient.FindPersonalChatAsync(
                    _apiClient.CurrentUser?.UserId ?? 0, targetUser.UserId);
                
                if (chatSuccess && chatFound && thread != null)
                {
                    ColorWriteLine($"Continuing existing chat with {targetUser.Username}...", Colors.Success);
                    await EnterChatSessionAsync(thread.ThreadId);
                }
                else
                {
                    // Create new personal chat
                    var title = $"Chat between {_apiClient.CurrentUser?.Username ?? "User"} and {targetUser.Username ?? "Unknown"}";
                    
                    var createResult = await _apiClient.CreateThreadAsync(
                        title, true, _apiClient.CurrentUser?.UserId ?? 0, targetUser.UserId);
                    
                    if (!createResult.success || createResult.thread == null)
                    {
                        ColorWriteLine($"Failed to create personal chat: {createResult.message}", Colors.Error);
                        return;
                    }
                    
                    ColorWriteLine($"Started new chat with {targetUser.Username}.", Colors.Success);
                    await EnterChatSessionAsync(createResult.thread.ThreadId);
                }
            }
            catch (Exception ex)
            {
                ColorWriteLine($"Error entering chat: {ex.Message}", Colors.Error);
            }
        }
    }
}
