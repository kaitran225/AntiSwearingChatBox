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
                var response = await _httpClient.GetAsync(ApiConfig.ThreadsEndpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var threads = JsonConvert.DeserializeObject<List<ChatThread>>(content);
                    return threads ?? new List<ChatThread>();
                }
                
                return new List<ChatThread>();
            }
            catch
            {
                return new List<ChatThread>();
            }
        }
        
        public async Task<ChatThread> CreateThreadAsync(string name)
        {
            try
            {
                var request = new { 
                    Name = name,
                    UserId = _currentUserId 
                };
                
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync(ApiConfig.ThreadsEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    if (result != null && result.Success == true && result.Thread != null)
                    {
                        var thread = JsonConvert.DeserializeObject<ChatThread>(result.Thread.ToString());
                        return thread ?? new ChatThread();
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
                var response = await _httpClient.GetAsync($"{ApiConfig.MessagesEndpoint}?threadId={threadId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var messages = JsonConvert.DeserializeObject<List<ChatMessage>>(content);
                    return messages ?? new List<ChatMessage>();
                }
                
                return new List<ChatMessage>();
            }
            catch
            {
                return new List<ChatMessage>();
            }
        }
        
        public async Task<ChatMessage> SendMessageAsync(int threadId, string content)
        {
            try
            {
                var request = new
                {
                    ThreadId = threadId,
                    Content = content,
                    UserId = _currentUserId
                };
                
                var jsonContent = CreateJsonContent(request);
                var response = await _httpClient.PostAsync(ApiConfig.MessagesEndpoint, jsonContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var message = JsonConvert.DeserializeObject<ChatMessage>(responseContent);
                    return message ?? new ChatMessage();
                }
                
                return new ChatMessage();
            }
            catch
            {
                return new ChatMessage();
            }
        }
        
        // SignalR Hub Methods
        public async Task ConnectToHubAsync()
        {
            if (string.IsNullOrEmpty(_token))
                return;

            try {
                // Check if the hub is available first
                var response = await _httpClient.GetAsync(ApiConfig.ChatHubUrl);
                if (!response.IsSuccessStatusCode) {
                    Console.WriteLine($"SignalR hub not available: {response.StatusCode}");
                    return;
                }
                
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(ApiConfig.ChatHubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_token);
                    })
                    .WithAutomaticReconnect()
                    .Build();

                _hubConnection.On<ChatMessage>("ReceiveMessage", message =>
                {
                    OnMessageReceived?.Invoke(message);
                });

                _hubConnection.On<ChatThread>("ThreadCreated", thread =>
                {
                    OnThreadCreated?.Invoke(thread);
                });

                _hubConnection.On<int, string>("UserJoinedThread", (userId, username) =>
                {
                    OnUserJoinedThread?.Invoke(userId, username);
                });

                await _hubConnection.StartAsync();
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
    }
} 