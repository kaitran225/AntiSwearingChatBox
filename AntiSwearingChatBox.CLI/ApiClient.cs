using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AntiSwearingChatBox.CLI
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private string _token;
        private User _currentUser;

        public ApiClient(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public User CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public void SetAuthToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<(bool success, string message, User user)> LoginAsync(string username, string password)
        {
            var loginData = new { Username = username, Password = password };
            var json = JsonConvert.SerializeObject(loginData);

            var response = await _httpClient.PostAsync("api/auth/login", 
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeAnonymousType(responseJson, 
                    new { Success = false, Message = "", Token = "", User = new User() });

                if (result.Success)
                {
                    _currentUser = result.User;
                    SetAuthToken(result.Token);
                    return (true, result.Message, result.User);
                }
                return (false, result.Message, null);
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeAnonymousType(errorJson, new { Success = false, Message = "" });
                return (false, error?.Message ?? "Unknown error", null);
            }
        }

        public async Task<(bool success, string message)> RegisterAsync(string username, string email, string password)
        {
            var registerData = new { Username = username, Email = email, Password = password };
            var json = JsonConvert.SerializeObject(registerData);

            var response = await _httpClient.PostAsync("api/auth/register",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeAnonymousType(responseJson, new { Success = false, Message = "" });

            return (result.Success, result.Message);
        }

        public async Task<User[]> GetAllUsersAsync()
        {
            var response = await _httpClient.GetAsync("api/auth/users");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<User[]>(json);
            }
            
            return Array.Empty<User>();
        }

        public async Task<ChatThread[]> GetUserThreadsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/chat/threads?userId={userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ChatThread[]>(json);
            }
            
            return Array.Empty<ChatThread>();
        }

        public async Task<ChatThread> GetThreadByIdAsync(int threadId)
        {
            var response = await _httpClient.GetAsync($"api/chat/threads/{threadId}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ChatThread>(json);
            }
            
            return null;
        }

        public async Task<EnrichedMessage[]> GetThreadMessagesAsync(int threadId)
        {
            var response = await _httpClient.GetAsync($"api/chat/threads/{threadId}/messages");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<EnrichedMessage[]>(json);
            }
            
            return Array.Empty<EnrichedMessage>();
        }

        public async Task<(bool success, string message, MessageHistory message)> SendMessageAsync(int threadId, int userId, string text)
        {
            var messageData = new { UserId = userId, Message = text };
            var json = JsonConvert.SerializeObject(messageData);

            var response = await _httpClient.PostAsync($"api/chat/threads/{threadId}/messages",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeAnonymousType(responseJson, 
                new { Success = false, Message = "", MessageHistory = new MessageHistory(), WasModerated = false });

            return (result.Success, result.Message, result.MessageHistory);
        }

        public async Task<EnrichedParticipant[]> GetThreadParticipantsAsync(int threadId)
        {
            var response = await _httpClient.GetAsync($"api/chat/threads/{threadId}/participants");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<EnrichedParticipant[]>(json);
            }
            
            return Array.Empty<EnrichedParticipant>();
        }

        public async Task<(bool success, string message, ChatThread thread)> CreateThreadAsync(string title, bool isPrivate, int creatorUserId, int? otherUserId = null)
        {
            var threadData = new { Title = title, IsPrivate = isPrivate, CreatorUserId = creatorUserId, OtherUserId = otherUserId };
            var json = JsonConvert.SerializeObject(threadData);

            var response = await _httpClient.PostAsync("api/chat/threads",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeAnonymousType(responseJson, 
                new { Success = false, Message = "", Thread = new ChatThread() });

            return (result.Success, result.Message, result.Thread);
        }

        public async Task<(bool success, string message)> AddParticipantAsync(int threadId, int userId, int requestedByUserId)
        {
            var participantData = new { UserId = userId, RequestedByUserId = requestedByUserId };
            var json = JsonConvert.SerializeObject(participantData);

            var response = await _httpClient.PostAsync($"api/chat/threads/{threadId}/participants",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeAnonymousType(responseJson, new { Success = false, Message = "" });

            return (result.Success, result.Message);
        }

        public async Task<(bool success, string message)> RemoveParticipantAsync(int threadId, int userId, int requestedByUserId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/chat/threads/{threadId}/participants/{userId}?requestedByUserId={requestedByUserId}");
            var response = await _httpClient.SendAsync(request);

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeAnonymousType(responseJson, new { Success = false, Message = "" });

            return (result.Success, result.Message);
        }

        public async Task<(bool success, bool found, ChatThread thread)> FindPersonalChatAsync(int userId, int otherUserId)
        {
            var response = await _httpClient.GetAsync($"api/chat/personal-chat?userId={userId}&otherUserId={otherUserId}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeAnonymousType(json, 
                    new { Success = false, Found = false, Thread = new ChatThread() });

                return (result.Success, result.Found, result.Thread);
            }
            
            return (false, false, null);
        }

        public async Task<(string filteredText, bool wasModified)> FilterProfanityAsync(string text)
        {
            var filterData = new { Text = text };
            var json = JsonConvert.SerializeObject(filterData);

            var response = await _httpClient.PostAsync("api/ai/filter-profanity",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeAnonymousType(responseJson, 
                new { Success = false, OriginalText = "", FilteredText = "", WasModified = false });

            return (result.FilteredText, result.WasModified);
        }

        public void Logout()
        {
            _token = null;
            _currentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ChatThread
    {
        public int ThreadId { get; set; }
        public string Title { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsActive { get; set; }
        public bool ModerationEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
    }

    public class MessageHistory
    {
        public int MessageId { get; set; }
        public int ThreadId { get; set; }
        public int UserId { get; set; }
        public string OriginalMessage { get; set; }
        public string ModeratedMessage { get; set; }
        public bool WasModified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ThreadParticipant
    {
        public int ParticipantId { get; set; }
        public int ThreadId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class EnrichedMessage
    {
        public MessageHistory Message { get; set; }
        public User User { get; set; }
    }

    public class EnrichedParticipant
    {
        public ThreadParticipant Participant { get; set; }
        public User User { get; set; }
    }
} 