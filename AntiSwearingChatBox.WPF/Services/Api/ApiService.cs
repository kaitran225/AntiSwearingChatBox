using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using AntiSwearingChatBox.WPF.Models.Api;
using AntiSwearingChatBox.WPF.Models;

namespace AntiSwearingChatBox.WPF.Services.Api
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private HubConnection? _hubConnection;
        private string _token = string.Empty;
        private int _currentUserId;
        private string _currentUsername = string.Empty;
        
        public UserModel? CurrentUser { get; private set; }
        
        public event Action<ChatMessage>? OnMessageReceived;
        public event Action<ChatThread>? OnThreadCreated;
        public event Action<int, string>? OnUserJoinedThread;
        
        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl)
            };
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
                    
                    return threads ?? [];
                }
                
                return [];
            }
            catch (Exception)
            {
                return [];
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
            catch (Exception)
            {
                return new ChatThread();
            }
        }
        
        public async Task<ChatThread> CreatePrivateChatAsync(int otherUserId, string title = "")
        {
            try
            {
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
            catch (Exception)
            {
                return new ChatThread();
            }
        }
        
        public async Task<List<UserModel>> GetUsersAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_token))
                {
                    return GetDummyUsers();
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                
                var response = await _httpClient.GetAsync($"{ApiConfig.BaseUrl}/api/users");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    var users = JsonConvert.DeserializeObject<List<UserModel>>(content);
                    
                    if (users == null || users.Count == 0)
                    {
                        return GetDummyUsers();
                    }
                    else
                    {
                        return users;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return GetDummyUsers();
                }
            }
            catch (Exception)
            {
                return GetDummyUsers();
            }
        }
        
        private List<UserModel> GetDummyUsers()
        {
            return
            [
                new() { UserId = 101, Username = "alice", Email = "alice@example.com" },
                new() { UserId = 102, Username = "bob", Email = "bob@example.com" },
                new() { UserId = 103, Username = "charlie", Email = "charlie@example.com" },
                new() { UserId = 104, Username = "diana", Email = "diana@example.com" },
                new() { UserId = 105, Username = "evan", Email = "evan@example.com" }
            ];
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
                string endpoint = $"{ApiConfig.ThreadsEndpoint}/{threadId}/messages";
                
                if (since.HasValue)
                {
                    string formattedDate = since.Value.ToString("o"); // ISO 8601 format
                    endpoint += $"?since={Uri.EscapeDataString(formattedDate)}";
                }
                
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    var messages = JsonConvert.DeserializeObject<List<ChatMessage>>(content, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    });
                                        
                    return messages ?? [];
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                }
                
                return [];
            }
            catch (Exception)
            {
                return [];
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
                    
                    var result = messages?
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(limit)
                        .ToList() ?? [];
                        
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                }
                
                return [];
            }
            catch (Exception)
            {
                return [];
            }
        }
        
        public async Task<ChatMessage> SendMessageAsync(int threadId, string content)
        {
            try
            {
                if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
                {
                    try
                    {
                        await ConnectToHubAsync();
                    }
                    catch (Exception)
                    {
                    }
                }
                
                var url = $"{ApiConfig.ThreadsEndpoint}/{threadId}/messages";
                var data = new
                {
                    UserId = _currentUserId,
                    Message = content
                };

                var jsonContent = CreateJsonContent(data);

                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    try 
                    {
                        var result = JsonConvert.DeserializeObject<SendMessageResult>(responseContent);
                        
                        if (result != null)
                        {
                            return result.MessageHistory;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                
                return new ChatMessage();
            }
            catch (Exception)
            {
                return new ChatMessage();
            }
        }
        
        public async Task ConnectToHubAsync()
        {
            if (string.IsNullOrEmpty(_token))
            {
                return;
            }

            try {
                await DisconnectFromHubAsync();
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(ApiConfig.ChatHubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_token)!;
                        
                        options.Headers["Authorization"] = $"Bearer {_token}";
                        
                        options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | 
                                          Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                        options.SkipNegotiation = false;
                        
                    })
                    .WithAutomaticReconnect(new[] { 
                        TimeSpan.FromSeconds(1), 
                        TimeSpan.FromSeconds(2), 
                        TimeSpan.FromSeconds(5), 
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(20)
                    })
                    .Build();

                _hubConnection.On<string, string, int, DateTime, int, string, bool>("ReceiveMessage", 
                    (username, message, userId, timestamp, threadId, originalMessage, containsProfanity) =>
                    {
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
                
                _hubConnection.Closed += async (error) =>
                {
                    try
                    {
                        await Task.Delay(new Random().Next(0, 5) * 1000);
                        
                        if (_hubConnection == null)
                        {
                            await ConnectToHubAsync();
                            return;
                        }
                        
                        try
                        {
                            await _hubConnection.StartAsync();
                            
                            if (_currentUserId > 0 && !string.IsNullOrEmpty(_currentUsername))
                            {
                                await _hubConnection.InvokeAsync("JoinChat", _currentUsername, _currentUserId);
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            _hubConnection = null;
                            await ConnectToHubAsync();
                        }
                    }
                    catch (Exception)
                    {
                    }
                };
                
                _hubConnection.Reconnecting += (error) =>
                {
                    return Task.CompletedTask;
                };
                
                _hubConnection.Reconnected += async (connectionId) =>
                {
                    try
                    {
                        if (_currentUserId > 0 && !string.IsNullOrEmpty(_currentUsername))
                        {
                            await _hubConnection.InvokeAsync("JoinChat", _currentUsername, _currentUserId);
                        }
                    }
                    catch (Exception)
                    {
                    }
                };

                await _hubConnection.StartAsync();
                
                if (_currentUserId > 0 && !string.IsNullOrEmpty(_currentUsername))
                {
                    try 
                    {
                        await _hubConnection.InvokeAsync("JoinChat", _currentUsername, _currentUserId);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception) {
            }
        }
        
        public async Task DisconnectFromHubAsync()
        {
            try
            {
                if (_hubConnection != null)
                {
                    var hubConnectionToDispose = _hubConnection;
                    _hubConnection = null;
                    
                    try
                    {
                        await hubConnectionToDispose.StopAsync();
                        await hubConnectionToDispose.DisposeAsync();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {   
                _hubConnection = null;
            }
        }

        public async Task<bool> IsHubConnectedAsync()
        {
            try
            {
                if (_hubConnection == null)
                {
                    return false;
                }

                bool isConnected = _hubConnection.State == HubConnectionState.Connected;
                
                return isConnected;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<List<ChatMessage>> GetMessagesAsync(int threadId)
        {
            return GetMessagesAsync(threadId, null);
        }

        public async Task JoinThreadChatGroupAsync(int threadId)
        {
            try
            {
                if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
                {
                    await ConnectToHubAsync();
                    
                    if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
                    {
                        return;
                    }
                }
                
                await _hubConnection.InvokeAsync("JoinThread", threadId);
            }
            catch (Exception)
            {
            }
        }

        public async Task LeaveThreadChatGroupAsync(int threadId)
        {
            try
            {
                if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
                {
                    return;
                }
                
                await _hubConnection.InvokeAsync("LeaveThreadGroup", threadId, _currentUserId);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                }
            }
        }

        #region AI Service Methods
        
        public async Task<string> GenerateTextAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_token))
            {
                return "Error: Authentication required";
            }
            
            try
            {
                var request = new { Prompt = prompt };
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync("api/Gemini/generate", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeAnonymousType(responseContent, new { Text = string.Empty });
                    return result?.Text ?? string.Empty;
                }
                
                return $"Error: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        
        public async Task<ModerationResult> ModerateChatMessageAsync(string message)
        {
            try
            {
                var request = new { Message = message };
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync("api/Gemini/moderate", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ModerationResult>(responseContent) ?? 
                           new ModerationResult { OriginalMessage = message, ModeratedMessage = message, WasModified = false };
                }
                
                return new ModerationResult
                {
                    OriginalMessage = message,
                    ModeratedMessage = message,
                    WasModified = false
                };
            }
            catch (Exception)
            {
                return new ModerationResult
                {
                    OriginalMessage = message,
                    ModeratedMessage = message,
                    WasModified = false
                };
            }
        }
        
        public async Task<ProfanityDetectionResult> DetectProfanityAsync(string message, bool verbose = false)
        {
            try
            {
                var request = new { 
                    Message = message,
                    IncludeDetails = verbose 
                };
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync("api/Gemini/detect-profanity", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ProfanityDetectionResult>(responseContent) ?? 
                           new ProfanityDetectionResult { OriginalMessage = message, ContainsProfanity = false };
                }
                
                return new ProfanityDetectionResult
                {
                    OriginalMessage = message,
                    ContainsProfanity = false
                };
            }
            catch (Exception)
            {
                return new ProfanityDetectionResult
                {
                    OriginalMessage = message,
                    ContainsProfanity = false
                };
            }
        }
        
        public async Task<ContextFilterResult> ContextAwareFilteringAsync(string message, string conversationContext)
        {
            try
            {
                var request = new { Message = message, ConversationContext = conversationContext };
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync("api/Gemini/context-filter", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ContextFilterResult>(responseContent) ?? 
                           new ContextFilterResult { OriginalMessage = message, ModeratedMessage = message, WasModified = false };
                }
                
                return new ContextFilterResult
                {
                    OriginalMessage = message,
                    ModeratedMessage = message,
                    WasModified = false
                };
            }
            catch (Exception)
            {
                return new ContextFilterResult
                {
                    OriginalMessage = message,
                    ModeratedMessage = message,
                    WasModified = false
                };
            }
        }

        public async Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string message)
        {
            try
            {
                var request = new { Message = message };
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync("api/Gemini/analyze-sentiment", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<SentimentAnalysisResult>(responseContent) ?? 
                           new SentimentAnalysisResult { SentimentScore = 5, ToxicityLevel = "none" };
                }
                
                return new SentimentAnalysisResult
                {
                    SentimentScore = 5,
                    ToxicityLevel = "none",
                    RequiresIntervention = false
                };
            }
            catch (Exception)
            {
                return new SentimentAnalysisResult
                {
                    SentimentScore = 5,
                    ToxicityLevel = "none",
                    RequiresIntervention = false
                };
            }
        }
        
        public async Task<DeescalationResult> GenerateDeescalationResponseAsync(string message)
        {
            try
            {
                var request = new { Message = message };
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync("api/Gemini/de-escalate", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<DeescalationResult>(responseContent) ?? 
                           new DeescalationResult { HarmfulMessage = message, DeescalationResponse = "No response generated." };
                }
                
                return new DeescalationResult
                {
                    HarmfulMessage = message,
                    DeescalationResponse = "No response generated.",
                    ResponseStrategy = "Error occurred."
                };
            }
            catch (Exception)
            {
                return new DeescalationResult
                {
                    HarmfulMessage = message,
                    DeescalationResponse = "No response generated.",
                    ResponseStrategy = "Error occurred."
                };
            }
        }
        
        public async Task<MessageHistoryAnalysisResult> ReviewMessageHistoryAsync(List<string> messages)
        {
            try
            {
                var request = new { Messages = messages };
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync("api/Gemini/review-message-history", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<MessageHistoryAnalysisResult>(responseContent) ?? 
                           new MessageHistoryAnalysisResult { MessageCount = messages.Count };
                }
                
                return new MessageHistoryAnalysisResult
                {
                    MessageCount = messages.Count,
                    OverallAssessment = "Error analyzing messages."
                };
            }
            catch (Exception)
            {
                return new MessageHistoryAnalysisResult
                {
                    MessageCount = messages.Count,
                    OverallAssessment = "Error analyzing messages."
                };
            }
        }
        
        public async Task<AlternativeMessageResult> SuggestAlternativeMessageAsync(string message)
        {
            try
            {
                var request = new { Message = message };
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync("api/Gemini/suggest-alternative", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AlternativeMessageResult>(responseContent) ?? 
                           new AlternativeMessageResult { OriginalMessage = message, SuggestedAlternative = message };
                }
                
                return new AlternativeMessageResult
                {
                    OriginalMessage = message,
                    SuggestedAlternative = message,
                    Explanation = "Error occurred."
                };
            }
            catch (Exception)
            {
                return new AlternativeMessageResult
                {
                    OriginalMessage = message,
                    SuggestedAlternative = message,
                    Explanation = "Error occurred."
                };
            }
        }
        
        public async Task<MultiLanguageModerationResult> ModerateMultiLanguageMessageAsync(string message, string language)
        {
            try
            {
                var request = new { Message = message, Language = language };
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync("api/Gemini/moderate-multi-language", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<MultiLanguageModerationResult>(responseContent) ?? 
                           new MultiLanguageModerationResult { OriginalMessage = message, ModeratedMessage = message, Language = language, WasModified = false };
                }
                
                return new MultiLanguageModerationResult
                {
                    OriginalMessage = message,
                    ModeratedMessage = message,
                    Language = language,
                    WasModified = false
                };
            }
            catch (Exception)
            {
                return new MultiLanguageModerationResult
                {
                    OriginalMessage = message,
                    ModeratedMessage = message,
                    Language = language,
                    WasModified = false
                };
            }
        }
       
        public async Task<UserReputationResult> AnalyzeUserReputationAsync(List<string> messages, int priorWarnings)
        {
            try
            {
                var request = new { Messages = messages, PriorWarnings = priorWarnings };
                var content = CreateJsonContent(request);
                
                var response = await _httpClient.PostAsync("api/Gemini/analyze-user-reputation", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserReputationResult>(responseContent) ?? 
                           new UserReputationResult { ReputationScore = 50, Trustworthiness = "medium" };
                }
                
                return new UserReputationResult
                {
                    ReputationScore = 50,
                    Trustworthiness = "medium",
                    Analysis = "Error analyzing user reputation."
                };
            }
            catch (Exception)
            {
                return new UserReputationResult
                {
                    ReputationScore = 50,
                    Trustworthiness = "medium",
                    Analysis = "Error analyzing user reputation."
                };
            }
        }
        
        #endregion

        public async Task<bool> UpdateThreadSwearingScoreAsync(int threadId, int score)
        {
            try
            {
                string url = $"{ApiConfig.BaseUrl}/api/threads/{threadId}/swearing-score";
                var content = new StringContent(
                    JsonConvert.SerializeObject(new { Score = score }),
                    Encoding.UTF8,
                    "application/json");
                    
                if (!string.IsNullOrEmpty(_token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                }
                
                var response = await _httpClient.PutAsync(url, content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CloseThreadAsync(int threadId)
        {
            try
            {
                string url = $"{ApiConfig.BaseUrl}/api/threads/{threadId}/close";
                
                if (!string.IsNullOrEmpty(_token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                }
                
                var response = await _httpClient.PostAsync(url, null);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 