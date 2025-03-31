using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.App.Services
{
    public class ApiService
    {
        private string? _authToken;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public User? CurrentUser { get; private set; }

        public void SetAuthToken(string token)
        {
            _authToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuthToken()
        {
            _authToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseJson, _jsonOptions);
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
        }

        // Auth methods
        public async Task<(bool success, string token, string message)> LoginAsync(string username, string password)
        {
            try
            {
                var loginData = new { Username = username, Password = password };
                var response = await PostAsync<LoginResponse>("api/auth/login", loginData);
                if (response.Success)
                {
                    SetAuthToken(response.Token);
                    CurrentUser = response.User;
                }
                return (response.Success, response.Token, response.Message);
            }
            catch (Exception ex)
            {
                return (false, null, $"Login error: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var registerData = new { Username = username, Email = email, Password = password };
                var response = await PostAsync<RegisterResponse>("api/auth/register", registerData);
                return (response.Success, response.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Registration error: {ex.Message}");
            }
        }

        // Chat methods
        public async Task<List<ChatThread>> GetUserThreadsAsync(int userId)
        {
            try
            {
                var response = await GetAsync<List<ChatThread>>($"api/chat/threads?userId={userId}");
                return response;
            }
            catch (Exception)
            {
                return new List<ChatThread>();
            }
        }

        public async Task<List<Message>> GetThreadMessagesAsync(int threadId)
        {
            try
            {
                var response = await GetAsync<List<Message>>($"api/chat/threads/{threadId}/messages");
                return response;
            }
            catch (Exception)
            {
                return new List<Message>();
            }
        }

        public async Task<(bool success, string message, Message sentMessage)> SendMessageAsync(int threadId, string text)
        {
            try
            {
                var messageData = new { Message = text };
                var response = await PostAsync<SendMessageResponse>($"api/chat/threads/{threadId}/messages", messageData);
                return (response.Success, response.Message, response.MessageHistory?.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Send message error: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message, ChatThread thread)> CreateThreadAsync(string title, bool isPrivate, int? otherUserId = null)
        {
            try
            {
                var threadData = new { Title = title, IsPrivate = isPrivate, OtherUserId = otherUserId };
                var response = await PostAsync<CreateThreadResponse>("api/chat/threads", threadData);
                return (response.Success, response.Message, response.Thread);
            }
            catch (Exception ex)
            {
                return (false, $"Create thread error: {ex.Message}", null);
            }
        }

        // Helper classes
        public class LoginResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string Token { get; set; }
            public User User { get; set; }
        }

        public class RegisterResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public class SendMessageResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public MessageHistory MessageHistory { get; set; }
            public bool WasModerated { get; set; }
        }

        public class CreateThreadResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public ChatThread Thread { get; set; }
        }

        public class User
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
        }

        public class ChatThread
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public bool IsPrivate { get; set; }
            public DateTime CreatedAt { get; set; }
            public int CreatorUserId { get; set; }
            public List<Participant> Participants { get; set; }
            public Message LastMessage { get; set; }
        }

        public class Participant
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int ThreadId { get; set; }
            public DateTime JoinedAt { get; set; }
            public User User { get; set; }
        }

        public class Message
        {
            public int Id { get; set; }
            public int ThreadId { get; set; }
            public int UserId { get; set; }
            public string Text { get; set; }
            public DateTime CreatedAt { get; set; }
            public User User { get; set; }
        }

        public class MessageHistory
        {
            public int Id { get; set; }
            public int ThreadId { get; set; }
            public int UserId { get; set; }
            public string OriginalMessage { get; set; }
            public string ModeratedMessage { get; set; }
            public DateTime CreatedAt { get; set; }
            public User User { get; set; }
            public Message Message { get; set; }
        }
    }
} 