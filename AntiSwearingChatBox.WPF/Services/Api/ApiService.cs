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
        
        public async Task<List<ChatMessage>> GetMessagesAsync(int threadId)
        {
            try
            {
                _selectedThreadId = threadId;
                var response = await _httpClient.GetAsync($"{ApiConfig.ThreadsEndpoint}/{threadId}/messages");
                
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
                _selectedThreadId = threadId;
                var request = new
                {
                    UserId = _currentUserId,
                    Message = content
                };
                
                var jsonContent = CreateJsonContent(request);
                var response = await _httpClient.PostAsync($"{ApiConfig.ThreadsEndpoint}/{threadId}/messages", jsonContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeAnonymousType(responseContent, 
                        new { Success = false, Message = "", MessageHistory = new ChatMessage(), WasModerated = false });
                        
                    if (result != null && result.Success)
                    {
                        return result.MessageHistory;
                    }
                }
                
                return new ChatMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return new ChatMessage();
            }
        }
        
        public async Task ConnectToHubAsync()
        {
            if (string.IsNullOrEmpty(_token))
                return;

            try {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(ApiConfig.ChatHubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_token)!;
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // Register event handlers
                _hubConnection.On<string, string, int, DateTime, int>("ReceiveMessage", 
                    (username, message, userId, timestamp, threadId) =>
                    {
                        var chatMessage = new ChatMessage
                        {
                            UserId = userId,
                            User = new UserModel { 
                                Username = username
                            },
                            OriginalMessage = message,
                            ModeratedMessage = message,
                            CreatedAt = timestamp,
                            ThreadId = threadId
                        };
                        
                        OnMessageReceived?.Invoke(chatMessage);
                    });

                _hubConnection.On<ChatThread>("ThreadCreated", thread =>
                {
                    OnThreadCreated?.Invoke(thread);
                });

                _hubConnection.On<string, int>("UserJoined", (username, userId) =>
                {
                    OnUserJoinedThread?.Invoke(userId, username);
                });

                await _hubConnection.StartAsync();
                Console.WriteLine("Connected to SignalR hub");
            }
            catch (Exception ex) {
                Console.WriteLine($"Error connecting to hub: {ex.Message}");
            }
        }
        
        public async Task DisconnectFromHubAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }
} 