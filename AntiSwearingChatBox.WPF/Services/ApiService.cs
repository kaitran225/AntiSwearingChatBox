using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntiSwearingChatBox.WPF.Models;
using AntiSwearingChatBox.WPF.Components;

namespace AntiSwearingChatBox.WPF.Services
{
    public class ApiService
    {
        public User? CurrentUser { get; private set; }
        
        public event Action<ChatMessageViewModel>? OnMessageReceived;
        public event Action<ChatThread>? OnThreadCreated;
        
        public async Task<(bool success, string token, string message)> LoginAsync(string username, string password)
        {
            // Simplified login implementation
            CurrentUser = new User { UserId = 1, Username = username, Email = $"{username}@example.com" };
            return (true, "sample-token", "Login successful");
        }
        
        public async Task<(bool success, string message)> RegisterAsync(string username, string email, string password)
        {
            // Simplified registration implementation
            return (true, "Registration successful");
        }
        
        public async Task<List<ChatThread>> GetUserThreadsAsync(int userId)
        {
            // Return sample data
            return new List<ChatThread>
            {
                new ChatThread { Id = 1, Title = "General Chat", LastMessageAt = DateTime.Now },
                new ChatThread { Id = 2, Title = "Tech Support", LastMessageAt = DateTime.Now.AddHours(-2) }
            };
        }
        
        public async Task ConnectToHubAsync()
        {
            // Placeholder for SignalR connection
            Console.WriteLine("Connected to hub");
        }
        
        public async Task DisconnectFromHubAsync()
        {
            // Placeholder for SignalR disconnection
            Console.WriteLine("Disconnected from hub");
        }

        public async Task<List<Message>> GetThreadMessagesAsync(int threadId)
        {
            // Return sample messages
            return new List<Message>
            {
                new Message
                { 
                    MessageId = 1,
                    UserId = CurrentUser?.UserId ?? 1,
                    Text = "Hello there!",
                    CreatedAt = DateTime.Now.AddMinutes(-5),
                    User = CurrentUser
                },
                new Message
                { 
                    MessageId = 2,
                    UserId = 2,
                    Text = "Hi! How can I help you today?",
                    CreatedAt = DateTime.Now.AddMinutes(-4),
                    User = new User { UserId = 2, Username = "Support", Email = "support@example.com" }
                }
            };
        }
        
        public async Task<(bool success, string message)> SendMessageAsync(int threadId, string content)
        {
            // Simulate sending a message
            return (true, "Message sent successfully");
        }
    }
    
    public class User
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
    
    public class ChatThread
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime LastMessageAt { get; set; }
    }

    public class Message
    {
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? ModeratedText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 