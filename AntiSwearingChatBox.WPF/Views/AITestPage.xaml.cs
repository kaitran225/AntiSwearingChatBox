using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.WPF.Services.Api;
using Newtonsoft.Json;

namespace AntiSwearingChatBox.WPF.View
{
    public partial class AITestPage : Page
    {
        private readonly IApiService _apiService;
        
        public AITestPage()
        {
            InitializeComponent();
            
            _apiService = Services.ServiceProvider.ApiService;
            Loaded += AITestPage_Loaded;
        }
        
        private async void AITestPage_Loaded(object sender, RoutedEventArgs e)
        {
            ResultsTextBox.Text = "Initializing connection to server...";
            
            try
            {
                if (_apiService == null)
                {
                    ResultsTextBox.Text = "Error: Could not initialize API service.";
                    return;
                }
                
                bool isConnected = await _apiService.IsHubConnectedAsync();
                if (!isConnected)
                {
                    ResultsTextBox.Text = "Connecting to server...";
                    await _apiService.ConnectToHubAsync();
                    
                    isConnected = await _apiService.IsHubConnectedAsync();
                    if (!isConnected)
                    {
                        ResultsTextBox.Text = "Warning: Could not connect to real-time services.\r\nYou can still use the test functions.";
                    }
                    else
                    {
                        ResultsTextBox.Text = "Connected successfully. Ready to test AI functions.";
                    }
                }
                else
                {
                    ResultsTextBox.Text = "Connected to server. Ready to test AI functions.";
                }
            }
            catch (Exception ex)
            {
                ResultsTextBox.Text = $"Error initializing connection: {ex.Message}\r\n\r\nYou can still use the test functions.";
            }
        }
        
        private async void RunTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_apiService == null)
                {
                    ResultsTextBox.Text = "Error: API service is not initialized.";
                    return;
                }
                
                string message = InputTextBox.Text.Trim();
                if (string.IsNullOrEmpty(message))
                {
                    MessageBox.Show("Please enter a message to test.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                RunTestButton.IsEnabled = false;
                ResultsTextBox.Text = "Processing...";
                
                var selectedItem = TestTypeComboBox.SelectedItem as ComboBoxItem;
                string testType = selectedItem?.Content.ToString() ?? "Profanity Detection";
                
                bool verboseMode = true;
                
                object? result = null;
                
                switch (testType)
                {
                    case "Profanity Detection":
                        result = await _apiService.DetectProfanityAsync(message, verboseMode);
                        break;
                        
                    case "Message Moderation":
                        result = await _apiService.ModerateChatMessageAsync(message);
                        break;
                        
                    case "Sentiment Analysis":
                        result = await _apiService.AnalyzeSentimentAsync(message);
                        break;
                        
                    case "Context-Aware Filtering":
                        string context = "Previous messages in this conversation were about technology and programming.";
                        result = await _apiService.ContextAwareFilteringAsync(message, context);
                        break;
                        
                    case "Alternative Suggestion":
                        result = await _apiService.SuggestAlternativeMessageAsync(message);
                        break;
                        
                    case "De-escalation Response":
                        result = await _apiService.GenerateDeescalationResponseAsync(message);
                        break;
                        
                    default:
                        result = new { Error = "Unknown test type selected." };
                        break;
                }
                
                string formattedResult = FormatResult(result);
                ResultsTextBox.Text = formattedResult;
            }
            catch (Exception ex)
            {
                ResultsTextBox.Text = $"Error occurred: {ex.Message}\n\n{ex.StackTrace}";
            }
            finally
            {
                RunTestButton.IsEnabled = true;
            }
        }
        
        private void ClearResultsButton_Click(object sender, RoutedEventArgs e)
        {
            ResultsTextBox.Clear();
        }
        
        private string FormatResult(object result)
        {
            if (result == null)
                return "No result returned.";
                
            try
            {
                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                return json;
            }
            catch (Exception ex)
            {
                return $"Error formatting result: {ex.Message}";
            }
        }
    }
} 