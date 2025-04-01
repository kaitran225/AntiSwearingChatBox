using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using AntiSwearingChatBox.WPF.Commands;

namespace AntiSwearingChatBox.WPF.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private string _messageText = string.Empty;
        
        public string MessageText
        {
            get => _messageText;
            set => SetProperty(ref _messageText, value);
        }
        
        public ICommand SendMessageCommand { get; }
        
        public ChatViewModel(string username)
        {
            SendMessageCommand = new RelayCommand(ExecuteSendMessage);
        }
        
        private void ExecuteSendMessage()
        {
            // Simply clear the message text for now
            MessageText = string.Empty;
        }
    }
    
    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
} 