using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;

namespace AntiSwearingChatBox.App.Views
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly IMessageHistoryService _messageHistoryService;
        
        public ChatWindow(IMessageHistoryService messageHistoryService = null)
        {
            _messageHistoryService = messageHistoryService;
            
            InitializeComponent();
            
            // Initialize conversation list with sample data
            InitializeSampleData();
            
            // Attach event handlers for admin dashboard and chat menu
            var adminButton = this.FindName("AdminDashboardButton") as System.Windows.Controls.Button;
            if (adminButton != null)
            {
                adminButton.Click += AdminDashboardButton_Click;
            }
            
            var closeAdminButton = this.FindName("CloseAdminDashboardButton") as System.Windows.Controls.Button;
            if (closeAdminButton != null)
            {
                closeAdminButton.Click += CloseAdminDashboardButton_Click;
            }
            
            var chatMenuButton = this.FindName("ChatMenuButton") as System.Windows.Controls.Button;
            if (chatMenuButton != null)
            {
                chatMenuButton.Click += ChatMenuButton_Click;
            }
        }

        private void InitializeSampleData()
        {
            // Add sample contacts to conversation list
            ConversationList.AddConversation(new Components.ContactViewModel
            {
                Id = "1",
                Name = "John Doe",
                Initials = "JD",
                LastMessage = "Let's meet tomorrow at 2pm",
                LastMessageTime = "12:42 PM",
                IsActive = true,
                IsOnline = true,
                Status = "Active now"
            });

            ConversationList.AddConversation(new Components.ContactViewModel
            {
                Id = "2",
                Name = "Group Team",
                Initials = "GT",
                LastMessage = "Alice: I've sent the files",
                LastMessageTime = "10:22 AM",
                HasUnread = true,
                UnreadCount = 3
            });

            ConversationList.AddConversation(new Components.ContactViewModel
            {
                Id = "3",
                Name = "Sarah Johnson",
                Initials = "SJ",
                LastMessage = "Can you review the document?",
                LastMessageTime = "Tuesday"
            });

            ConversationList.AddGroup(new Components.ContactViewModel
            {
                Id = "4",
                Name = "Project Team",
                Initials = "PT",
                LastMessage = "James: Meeting at 3pm tomorrow",
                LastMessageTime = "Yesterday"
            });

            // Set up chat with initial contact
            ChatView.CurrentContact = new Components.ContactViewModel
            {
                Id = "1",
                Name = "John Doe",
                Initials = "JD",
                Status = "Active now"
            };

            // Add sample messages
            ChatView.Messages.Add(new Components.MessageViewModel
            {
                IsSent = false,
                Text = "Hey there! How's the project coming along? Would you be available for a meeting tomorrow to discuss the next steps?",
                Timestamp = "12:31 PM",
                Avatar = "JD",
                Background = (Brush)Application.Current.Resources["SecondaryBackgroundBrush"],
                BorderBrush = (Brush)Application.Current.Resources["BorderBrush"]
            });

            ChatView.Messages.Add(new Components.MessageViewModel
            {
                IsSent = true,
                Text = "Hi John! The project is going well. I've completed most of the initial requirements. A meeting tomorrow sounds good. How about 2 PM?",
                Timestamp = "12:33 PM",
                Avatar = "ME",
                Background = (Brush)Application.Current.Resources["PrimaryBackgroundBrush"],
                BorderBrush = (Brush)Application.Current.Resources["PrimaryGreenBrush"]
            });

            ChatView.Messages.Add(new Components.MessageViewModel
            {
                IsSent = false,
                Text = "Perfect! 2 PM works for me. I'll set up the meeting and send you the invite. Could you prepare a short summary of what you've done so far?",
                Timestamp = "12:35 PM",
                Avatar = "JD",
                Background = (Brush)Application.Current.Resources["SecondaryBackgroundBrush"],
                BorderBrush = (Brush)Application.Current.Resources["BorderBrush"]
            });

            ChatView.Messages.Add(new Components.MessageViewModel
            {
                IsSent = true,
                Text = "Sure thing! I'll have everything ready. See you tomorrow!",
                Timestamp = "12:36 PM",
                Avatar = "ME",
                Background = (Brush)Application.Current.Resources["PrimaryBackgroundBrush"],
                BorderBrush = (Brush)Application.Current.Resources["PrimaryGreenBrush"]
            });
        }

        #region Event Handlers
        
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void ConversationList_ConversationSelected(object sender, string id)
        {
            // Update current contact in chat view
            var contact = ConversationList.Conversations.FirstOrDefault(c => c.Id == id);
            if (contact != null)
            {
                ChatView.CurrentContact = contact;
                
                // In a real app, we would load messages for this contact
                // For demo, we'll just keep the existing messages
            }
            else
            {
                var group = ConversationList.Groups.FirstOrDefault(g => g.Id == id);
                if (group != null)
                {
                    ChatView.CurrentContact = group;
                    
                    // In a real app, we would load messages for this group
                    // For demo, we'll just keep the existing messages
                }
            }
        }
        
        private void ConversationList_NewChatRequested(object sender, EventArgs e)
        {
            // In a real app, this would show a UI to select a contact to start a new chat with
            MessageBox.Show("New chat functionality would be implemented here.");
        }
        
        private void ConversationList_AddConversationRequested(object sender, EventArgs e)
        {
            // In a real app, this would show a UI to add a new contact or group
            MessageBox.Show("Add conversation functionality would be implemented here.");
        }
        
        private void ChatView_MessageSent(object sender, string message)
        {
            // In a real app, this would send the message to the server
            // For demo, we'll simulate a response
            
            // Update last message in conversation list
            ConversationList.UpdateConversation(
                ChatView.CurrentContact.Id,
                "You: " + message,
                DateTime.Now.ToString("h:mm tt"));
            
            // Save message to database if the service is available
            if (_messageHistoryService != null)
            {
                try
                {
                    // Create message history object
                    var messageHistory = new MessageHistory
                    {
                        // Parse IDs assuming they are numeric strings
                        ThreadId = int.TryParse(ChatView.CurrentContact.Id, out int threadId) ? threadId : 1,
                        UserId = 1, // Current user ID, would come from authentication in real app
                        OriginalMessage = message,
                        ModeratedMessage = message, // No moderation applied in this example
                        WasModified = false,
                        CreatedAt = DateTime.Now
                    };
                    
                    // Add message to database
                    var result = _messageHistoryService.Add(messageHistory);
                    
                    if (!result.success)
                    {
                        // Log error or show message to user in real application
                        System.Diagnostics.Debug.WriteLine($"Error saving message: {result.message}");
                    }
                }
                catch (Exception ex)
                {
                    // Log error or show message to user in real application
                    System.Diagnostics.Debug.WriteLine($"Exception saving message: {ex.Message}");
                }
            }
            
            // Simulate a response after a short delay
            Task.Delay(2000).ContinueWith(_ =>
            {
                // Run on UI thread
                Dispatcher.Invoke(() =>
                {
                    // Add a response message
                    ChatView.AddReceivedMessage(
                        "Thanks for your message! This is an automated response.",
                        ChatView.CurrentContact.Name,
                        ChatView.CurrentContact.Initials);
                    
                    // Update conversation list
                    ConversationList.UpdateConversation(
                        ChatView.CurrentContact.Id,
                        ChatView.CurrentContact.Name + ": Thanks for your message!",
                        DateTime.Now.ToString("h:mm tt"),
                        true);
                    
                    // Save response to database if the service is available
                    if (_messageHistoryService != null)
                    {
                        try
                        {
                            // Create message history object for the response
                            var responseMessage = new MessageHistory
                            {
                                // Parse IDs assuming they are numeric strings
                                ThreadId = int.TryParse(ChatView.CurrentContact.Id, out int threadId) ? threadId : 1,
                                UserId = int.TryParse(ChatView.CurrentContact.Id, out int userId) ? userId : 2, // Contact's ID
                                OriginalMessage = "Thanks for your message! This is an automated response.",
                                ModeratedMessage = "Thanks for your message! This is an automated response.", // No moderation applied
                                WasModified = false,
                                CreatedAt = DateTime.Now
                            };
                            
                            // Add response to database
                            var result = _messageHistoryService.Add(responseMessage);
                            
                            if (!result.success)
                            {
                                // Log error or show message to user in real application
                                System.Diagnostics.Debug.WriteLine($"Error saving response: {result.message}");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error or show message to user in real application
                            System.Diagnostics.Debug.WriteLine($"Exception saving response: {ex.Message}");
                        }
                    }
                });
            });
        }
        
        private void ChatView_PhoneCallRequested(object sender, EventArgs e)
        {
            MessageBox.Show($"Initiating phone call with {ChatView.CurrentContact.Name}...");
        }
        
        private void ChatView_VideoCallRequested(object sender, EventArgs e)
        {
            MessageBox.Show($"Initiating video call with {ChatView.CurrentContact.Name}...");
        }
        
        private void ChatView_MenuRequested(object sender, EventArgs e)
        {
            MessageBox.Show("Chat menu options would be shown here.");
        }
        
        private void ChatView_AttachmentRequested(object sender, EventArgs e)
        {
            MessageBox.Show("Attachment selection UI would be shown here.");
        }
        
        private void ConversationList_AdminDashboardRequested(object sender, EventArgs e)
        {
            AdminDashboardPanel.Visibility = Visibility.Visible;
        }
        
        private void AdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle admin dashboard visibility
            var adminPanel = this.FindName("AdminDashboardPanel") as System.Windows.Controls.Grid;
            if (adminPanel != null)
            {
                adminPanel.Visibility = Visibility.Visible;
            }
        }
        
        private void CloseAdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide admin dashboard
            var adminPanel = this.FindName("AdminDashboardPanel") as System.Windows.Controls.Grid;
            if (adminPanel != null)
            {
                adminPanel.Visibility = Visibility.Collapsed;
            }
        }
        
        private void ChatMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle chat menu popup
            var chatMenuPopup = this.FindName("ChatMenuPopup") as System.Windows.Controls.Primitives.Popup;
            if (chatMenuPopup != null)
            {
                chatMenuPopup.IsOpen = !chatMenuPopup.IsOpen;
            }
        }
        
        #endregion
    }
} 