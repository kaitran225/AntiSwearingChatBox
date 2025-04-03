using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using AntiSwearingChatBox.WPF.Models.Api;
using System.Linq;
using System.Threading;
using AntiSwearingChatBox.WPF.Models;
using Microsoft.Extensions.Logging;

namespace AntiSwearingChatBox.WPF.Services.Api
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private HubConnection? _hubConnection;
        private string _token = string.Empty;
        private int _currentUserId;
        private string _currentUsername = string.Empty;
        private int _selectedThreadId;
        
        public UserModel? CurrentUser { get; private set; }
        
        public event Action<ChatMessage>? OnMessageReceived;
        public event Action<ChatThread>? OnThreadCreated;
        public event Action<int, string>? OnUserJoinedThread;
        
        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(ApiConfig.BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        private void SetAuthorizationHeader(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        private StringContent CreateJsonContent<T>(T data)
        {
            var json = JsonConvert.SerializeObject(data);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
        
        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            try
            {
                var loginRequest = new LoginRequest
                {
                    Username = username,
                    Password = password
                };
                
                var content = CreateJsonContent(loginRequest);
                var response = await _httpClient.PostAsync(ApiConfig.LoginEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<AuthResponse>(responseContent);
                    
                    if (result != null && result.Success)
                    {
                        _currentUserId = result.UserId;
                        _currentUsername = username;
                        SetAuthorizationHeader(result.Token);
                        
                        // Set current user
                        CurrentUser = new UserModel
                        {
                            UserId = result.UserId,
                            Username = username
                        };
                    }
                    
                    return result ?? new AuthResponse { Success = false, Message = "Failed to deserialize response" };
                }
                
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Login failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Login error: {ex.Message}"
                };
            }
        }
        
        public async Task<AuthResponse> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var registerRequest = new RegisterRequest
                {
                    Username = username,
                    Email = email,
                    Password = password
                };
                
                var content = CreateJsonContent(registerRequest);
                var response = await _httpClient.PostAsync(ApiConfig.RegisterEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AuthResponse>(responseContent) ?? 
                           new AuthResponse { Success = false, Message = "Failed to deserialize response" };
                }
                
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Registration failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Registration error: {ex.Message}"
                };
            }
        }
        
        public async Task<List<ChatThread>> GetThreadsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiConfig.ThreadsEndpoint}?userId={_currentUserId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var threads = JsonConvert.DeserializeObject<List<ChatThread>>(content);
                    return threads ?? new List<ChatThread>();
                }
                
                return new List<ChatThread>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading threads: {ex.Message}");
                return new List<ChatThread>();
            }
        }
        
        public async Task<ChatThread> CreateThreadAsync(string name)
        {
            try
            {
                var request = new { 
                    Title = name,
                    IsPrivate = false,
                    CreatorUserId = _currentUserId,
                    OtherUserId = (int?)null
                };
                
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync(ApiConfig.ThreadsEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeAnonymousType(responseContent, 
                        new { Success = false, Message = "", Thread = new ChatThread() });
                    
                    if (result != null && result.Success)
                    {
                        return result.Thread;
                    }
                }
                
                return new ChatThread();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating thread: {ex.Message}");
                return new ChatThread();
            }
        }
        
        public async Task<ChatThread> CreatePrivateChatAsync(int otherUserId, string title = "")
        {
            try
            {
                // Get user details to create appropriate chat title if not provided
                if (string.IsNullOrEmpty(title))
                {
                    var users = await GetUsersAsync();
                    var otherUser = users.FirstOrDefault(u => u.UserId == otherUserId);
                    title = otherUser?.Username ?? $"Chat with User {otherUserId}";
                }
                
                var request = new { 
                    Title = title,
                    IsPrivate = true,
                    CreatorUserId = _currentUserId,
                    OtherUserId = otherUserId
                };
                
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync(ApiConfig.ThreadsEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeAnonymousType(responseContent, 
                        new { Success = false, Message = "", Thread = new ChatThread() });
                    
                    if (result != null && result.Success)
                    {
                        return result.Thread;
                    }
                }
                
                return new ChatThread();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating private chat: {ex.Message}");
                return new ChatThread();
            }
        }
        
        public async Task<List<UserModel>> GetUsersAsync()
        {
            try
            {
                // Make sure we have a token set
                if (string.IsNullOrEmpty(_token))
                {
                    Console.WriteLine("Error: No authentication token available. Please login first.");
                    // Return dummy users instead of an empty list if not authenticated
                    return GetDummyUsers();
                }

                // Ensure authorization header is set for this request
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                
                // Use the correct endpoint as defined in the controller
                var response = await _httpClient.GetAsync($"{ApiConfig.BaseUrl}/api/users");
                
                Console.WriteLine($"GetUsersAsync response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"User response content: {content.Substring(0, Math.Min(100, content.Length))}...");
                    
                    var users = JsonConvert.DeserializeObject<List<UserModel>>(content);
                    
                    if (users == null || users.Count == 0)
                    {
                        Console.WriteLine("Warning: No users returned from API or deserialization failed");
                        // If no users returned but request was successful, return dummy users
                        return GetDummyUsers();
                    }
                    else
                    {
                        Console.WriteLine($"Successfully retrieved {users.Count} users");
                        return users;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error getting users. Status: {response.StatusCode}, Content: {errorContent}");
                    // Return dummy users when API request fails
                    return GetDummyUsers();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetUsersAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                // Return dummy users when exception occurs
                return GetDummyUsers();
            }
        }
        
        // Helper method to provide dummy users when API fails
        private List<UserModel> GetDummyUsers()
        {
            Console.WriteLine("Generating dummy users since API didn't return any");
            return new List<UserModel>
            {
                new UserModel { UserId = 101, Username = "alice", Email = "alice@example.com" },
                new UserModel { UserId = 102, Username = "bob", Email = "bob@example.com" },
                new UserModel { UserId = 103, Username = "charlie", Email = "charlie@example.com" },
                new UserModel { UserId = 104, Username = "diana", Email = "diana@example.com" },
                new UserModel { UserId = 105, Username = "evan", Email = "evan@example.com" }
            };
        }
        
        public async Task<bool> JoinThreadAsync(int threadId)
        {
            try
            {
                var request = new { 
                    ThreadId = threadId,
                    UserId = _currentUserId
                };
                
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync($"{ApiConfig.ThreadsEndpoint}/{threadId}/join", content);
                
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<List<ChatMessage>> GetMessagesAsync(int threadId, DateTime? since = null)
        {
            try
            {
                _selectedThreadId = threadId;
                string endpoint = $"{ApiConfig.ThreadsEndpoint}/{threadId}/messages";
                
                // Add since parameter if provided
                if (since.HasValue)
                {
                    string formattedDate = since.Value.ToString("o"); // ISO 8601 format
                    endpoint += $"?since={Uri.EscapeDataString(formattedDate)}";
                    Console.WriteLine($"Fetching messages since {formattedDate}");
                }
                
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Message response: {content.Substring(0, Math.Min(100, content.Length))}...");
                    
                    var messages = JsonConvert.DeserializeObject<List<ChatMessage>>(content, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    });
                    
                    Console.WriteLine($"Deserialized {messages?.Count ?? 0} messages");
                    
                    if (messages != null && messages.Count > 0)
                    {
                        foreach (var msg in messages)
                        {
                            Console.WriteLine($"Message: {msg.OriginalMessage} from {msg.SenderName} at {msg.Timestamp}");
                        }
                    }
                    
                    return messages ?? new List<ChatMessage>();
                }
                else
                {
                    Console.WriteLine($"Error getting messages: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error content: {errorContent}");
                }
                
                return new List<ChatMessage>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading messages: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner error: {ex.InnerException.Message}");
                }
                return new List<ChatMessage>();
            }
        }
        
        public async Task<List<ChatMessage>> GetMessagesAsync(int threadId, int limit)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiConfig.ThreadsEndpoint}/{threadId}/messages");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    var messages = JsonConvert.DeserializeObject<List<ChatMessage>>(content, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    });
                    
                    // Sort by date descending and apply limit manually
                    var result = messages?
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(limit)
                        .ToList() ?? new List<ChatMessage>();
                        
                    return result;
                }
                else
                {
                    Console.WriteLine($"Error getting messages with limit: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error content: {errorContent}");
                }
                
                return new List<ChatMessage>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading messages with limit: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner error: {ex.InnerException.Message}");
                }
                return new List<ChatMessage>();
            }
        }
        
        public async Task<ChatMessage> SendMessageAsync(int threadId, string content)
        {
            try
            {
                // Log request details for debugging
                Console.WriteLine($"Sending message to API. URL: {ApiConfig.ThreadsEndpoint}/{threadId}/messages, ThreadID: {threadId}, Content length: {content.Length}");
                
                var url = $"{ApiConfig.ThreadsEndpoint}/{threadId}/messages";
                var data = new
                {
                    UserId = _currentUserId,
                    Message = content
                };

                var jsonContent = CreateJsonContent(data);
                Console.WriteLine($"Request data: {await jsonContent.ReadAsStringAsync()}");

                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: Status={response.StatusCode}, Content={responseContent}");
                
                if (response.IsSuccessStatusCode)
                {
                    try 
                    {
                        var result = JsonConvert.DeserializeObject<SendMessageResult>(responseContent);
                        
                        if (result != null)
                        {
                            // REMOVED: Do not manually invoke SignalR anymore
                            // The server already sends the message to all clients via SignalR when we call the API endpoint
                            // This was causing duplicate messages to be saved to the database
                            
                            return result.MessageHistory;
                        }
                        else
                        {
                            Console.WriteLine("Failed to deserialize API response");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing successful API response: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"API returned error: {response.StatusCode}, {responseContent}");
                }
                
                return new ChatMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return new ChatMessage();
            }
        }
        
        public async Task ConnectToHubAsync()
        {
            if (string.IsNullOrEmpty(_token))
            {
                Console.WriteLine("Cannot connect to SignalR - no authentication token available");
                return;
            }

            try {
                // First disconnect any existing connection to ensure clean state
                await DisconnectFromHubAsync();
                
                // Double check we're using the correct URL
                Console.WriteLine($"Initializing SignalR connection to {ApiConfig.ChatHubUrl}...");
                
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(ApiConfig.ChatHubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_token)!;
                        
                        // Add detailed logging
                        Console.WriteLine($"Setting up SignalR connection with token: {_token.Substring(0, Math.Min(10, _token.Length))}...");
                        
                        // Add explicit auth header
                        options.Headers["Authorization"] = $"Bearer {_token}";
                        
                        // Try different transport modes to ensure connectivity
                        // First try WebSockets for best performance
                        options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | 
                                          Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                                          
                        // Don't skip negotiation as it may be required
                        options.SkipNegotiation = false;
                        
                        Console.WriteLine("SignalR: Configured for WebSockets and LongPolling transports");
                    })
                    .WithAutomaticReconnect(new[] { 
                        TimeSpan.FromSeconds(1), 
                        TimeSpan.FromSeconds(2), 
                        TimeSpan.FromSeconds(5), 
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(20)
                    })
                    .Build();

                // Register event handlers - ensure we match the server's full parameter list
                _hubConnection.On<string, string, int, DateTime, int, string, bool>("ReceiveMessage", 
                    (username, message, userId, timestamp, threadId, originalMessage, containsProfanity) =>
                    {
                        Console.WriteLine($"SignalR: RECEIVED MESSAGE: From={username}, Thread={threadId}, Message={message}, ContainsProfanity={containsProfanity}");
                        
                        var chatMessage = new ChatMessage
                        {
                            UserId = userId,
                            User = new UserModel { 
                                Username = username
                            },
                            OriginalMessage = originalMessage,
                            ModeratedMessage = message,
                            CreatedAt = timestamp,
                            ThreadId = threadId,
                            WasModified = containsProfanity
                        };
                        
                        Console.WriteLine($"SignalR: Invoking OnMessageReceived event handler with message from {username}");
                        OnMessageReceived?.Invoke(chatMessage);
                    });

                _hubConnection.On<ChatThread>("ThreadCreated", thread =>
                {
                    Console.WriteLine($"SignalR: THREAD CREATED: {thread.Name} (ID: {thread.ThreadId})");
                    OnThreadCreated?.Invoke(thread);
                });

                _hubConnection.On<string, int>("UserJoined", (username, userId) =>
                {
                    Console.WriteLine($"SignalR: USER JOINED: {username} (ID: {userId})");
                    OnUserJoinedThread?.Invoke(userId, username);
                });
                
                // Add comprehensive connection state handling
                _hubConnection.Closed += async (error) =>
                {
                    Console.WriteLine($"SignalR: CONNECTION CLOSED with error: {error?.Message}");
                    try
                    {
                        // Wait a bit and try to reconnect
                        await Task.Delay(new Random().Next(0, 5) * 1000);
                        
                        Console.WriteLine("SignalR: Attempting to restart connection after closure");
                        await _hubConnection.StartAsync();
                        Console.WriteLine($"SignalR: Connection restarted with state: {_hubConnection.State}");
                        
                        // Re-join the chat after reconnection
                        if (_currentUserId > 0 && !string.IsNullOrEmpty(_currentUsername))
                        {
                            Console.WriteLine($"SignalR: Re-joining chat as {_currentUsername} after reconnection");
                            await _hubConnection.InvokeAsync("JoinChat", _currentUsername, _currentUserId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"SignalR: ERROR reconnecting after connection closed: {ex.Message}");
                    }
                };
                
                // Add handler for reconnection
                _hubConnection.Reconnecting += (error) =>
                {
                    Console.WriteLine($"SignalR: RECONNECTING after error: {error?.Message}");
                    return Task.CompletedTask;
                };
                
                // Add handler for successful reconnection
                _hubConnection.Reconnected += async (connectionId) =>
                {
                    Console.WriteLine($"SignalR: RECONNECTED with ID: {connectionId}");
                    
                    // Re-join chat after successful reconnection
                    try
                    {
                        if (_currentUserId > 0 && !string.IsNullOrEmpty(_currentUsername))
                        {
                            Console.WriteLine($"SignalR: Re-joining chat as {_currentUsername} after successful reconnection");
                            await _hubConnection.InvokeAsync("JoinChat", _currentUsername, _currentUserId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"SignalR: ERROR re-joining chat after reconnection: {ex.Message}");
                    }
                };

                // Try to connect
                Console.WriteLine("SignalR: Starting connection...");
                await _hubConnection.StartAsync();
                Console.WriteLine($"SignalR: Connection STARTED successfully with state: {_hubConnection.State}");
                
                // Join the chat after connection
                if (_currentUserId > 0 && !string.IsNullOrEmpty(_currentUsername))
                {
                    Console.WriteLine($"SignalR: Joining chat as {_currentUsername} (ID: {_currentUserId})");
                    try 
                    {
                        await _hubConnection.InvokeAsync("JoinChat", _currentUsername, _currentUserId);
                        Console.WriteLine($"SignalR: Successfully joined chat as {_currentUsername}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"SignalR: Error joining chat: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"SignalR: ERROR connecting to hub: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"SignalR: Inner exception: {ex.InnerException.Message}");
                }
            }
        }
        
        public async Task DisconnectFromHubAsync()
        {
            try
            {
                if (_hubConnection != null)
                {
                    await _hubConnection.StopAsync();
                    await _hubConnection.DisposeAsync();
                    _hubConnection = null;
                    Console.WriteLine("SignalR hub connection stopped and disposed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting from hub: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        public async Task<bool> IsHubConnectedAsync()
        {
            try
            {
                // Check if hub connection is initialized and in the Connected state
                if (_hubConnection == null)
                {
                    Console.WriteLine("SignalR hub connection is null");
                    return false;
                }

                // Check connection state
                bool isConnected = _hubConnection.State == HubConnectionState.Connected;
                Console.WriteLine($"SignalR hub connection state: {_hubConnection.State}");
                
                return isConnected;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking hub connection: {ex.Message}");
                return false;
            }
        }

        // Simple overload that calls the version with since parameter
        public Task<List<ChatMessage>> GetMessagesAsync(int threadId)
        {
            return GetMessagesAsync(threadId, null);
        }

        public async Task JoinThreadChatGroupAsync(int threadId)
        {
            try
            {
                // Check if we have a valid connection
                if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
                {
                    Console.WriteLine($"SignalR: Cannot join thread group {threadId} - no active connection");
                    await ConnectToHubAsync();
                    
                    if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
                    {
                        Console.WriteLine($"SignalR: Still unable to connect after retry. Cannot join thread {threadId}");
                        return;
                    }
                }
                
                // Join the thread group
                Console.WriteLine($"SignalR: Joining thread group {threadId}");
                await _hubConnection.InvokeAsync("JoinThread", threadId);
                Console.WriteLine($"SignalR: Successfully joined thread group {threadId}");
                
                // Update selected thread ID
                _selectedThreadId = threadId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR: Error joining thread group {threadId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"SignalR: Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        public async Task LeaveThreadChatGroupAsync(int threadId)
        {
            try
            {
                if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
                {
                    Console.WriteLine($"SignalR: Cannot leave thread group - hub is not connected (State: {_hubConnection?.State.ToString() ?? "null"})");
                    return;
                }
                
                // Call the LeaveThreadGroup method on the SignalR hub
                Console.WriteLine($"SignalR: Leaving thread group for thread {threadId} as user {_currentUserId}");
                await _hubConnection.InvokeAsync("LeaveThreadGroup", threadId, _currentUserId);
                Console.WriteLine($"SignalR: Successfully left thread group {threadId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR: Error leaving thread group: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"SignalR: Inner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
} 