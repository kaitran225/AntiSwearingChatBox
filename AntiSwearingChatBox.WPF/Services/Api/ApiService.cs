using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AntiSwearingChatBox.WPF.Models.Api;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace AntiSwearingChatBox.WPF.Services.Api
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private HubConnection? _hubConnection;
        private string _token = string.Empty;
        private int _currentUserId;
        private string _currentUsername = string.Empty;
        
        public event Action<ChatMessage>? OnMessageReceived;
        public event Action<ChatThread>? OnThreadCreated;
        public event Action<int, string>? OnUserJoinedThread;
        
        public ApiService()
        {
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            });
        }
        
        private void SetAuthorizationHeader(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
        
        private StringContent CreateJsonContent<T>(T data)
        {
            var json = JsonConvert.SerializeObject(data);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
        
        // Authentication Methods
        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    Username = username,
                    Password = password
                };
                
                var content = CreateJsonContent(request);
                var response = await _httpClient.PostAsync(ApiConfig.LoginEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);
                    
                    if (authResponse != null && authResponse.Success)
                    {
                        SetAuthorizationHeader(authResponse.Token);
                        _currentUserId = authResponse.UserId;
                        _currentUsername = authResponse.Username;
                        CurrentUser = authResponse.User;
                    }
                    
                    return authResponse ?? new AuthResponse { Success = false, Message = "Failed to parse response" };
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
                var request = new RegisterRequest
                {
                    Username = username,
                    Email = email,
                    Password = password
                };
                
                var content = CreateJsonContent(request);
                var response = await _httpClient.PostAsync(ApiConfig.RegisterEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);
                    
                    return authResponse ?? new AuthResponse { Success = false, Message = "Failed to parse response" };
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
        
        // Chat Thread Methods
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
                    OtherUserId = (int?)null // For personal chats
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
        
        // Chat Message Methods
        public async Task<List<ChatMessage>> GetMessagesAsync(int threadId)
        {
            try
            {
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
                            // Make sure Content and Username are properly set
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
        
        public async Task<ChatMessage> SendMessageAsync(int threadId, string content)
        {
            try
            {
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
        
        // SignalR Hub Methods
        public async Task ConnectToHubAsync()
        {
            if (string.IsNullOrEmpty(_token))
                return;

            try {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(ApiConfig.ChatHubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_token);
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // Register event handlers
                _hubConnection.On<string, string, int, DateTime>("ReceiveMessage", 
                    (username, message, userId, timestamp) =>
                    {
                        var chatMessage = new ChatMessage
                        {
                            User = new UserModel { Username = username },
                            ModeratedMessage = message,
                            UserId = userId,
                            CreatedAt = timestamp
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
            
                // Once connected, join the chat with our user info
                await _hubConnection.InvokeAsync("JoinChat", _currentUsername, _currentUserId);
            
                Console.WriteLine("Successfully connected to SignalR hub");
            }
            catch (Exception ex) {
                Console.WriteLine($"SignalR connection error: {ex.Message}");
            }
        }

        public async Task DisconnectFromHubAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }

        public UserModel? CurrentUser { get; private set; }
    }
} 