using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.WPF.Services.Api;
using Newtonsoft.Json;

namespace AntiSwearingChatBox.WPF.View
{
    /// <summary>
    /// Interaction logic for AITestPage.xaml
    /// </summary>
    public partial class AITestPage : Page
    {
        private readonly IApiService _apiService;
        
        public AITestPage()
        {
            InitializeComponent();
            _apiService = Services.ServiceProvider.ApiService;
        }
        
        private async void RunTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = InputTextBox.Text.Trim();
                if (string.IsNullOrEmpty(message))
                {
                    MessageBox.Show("Please enter a message to test.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Disable button while processing
                RunTestButton.IsEnabled = false;
                ResultsTextBox.Text = "Processing...";
                
                // Get selected test type
                var selectedItem = TestTypeComboBox.SelectedItem as ComboBoxItem;
                string testType = selectedItem?.Content.ToString() ?? "Profanity Detection";
                
                // Run the selected test
                object result = null;
                
                switch (testType)
                {
                    case "Profanity Detection":
                        result = await _apiService.DetectProfanityAsync(message);
                        break;
                        
                    case "Message Moderation":
                        result = await _apiService.ModerateChatMessageAsync(message);
                        break;
                        
                    case "Sentiment Analysis":
                        result = await _apiService.AnalyzeSentimentAsync(message);
                        break;
                        
                    case "Context-Aware Filtering":
                        // Simple context for testing
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
                
                // Format and display the result
                string formattedResult = FormatResult(result);
                ResultsTextBox.Text = formattedResult;
            }
            catch (Exception ex)
            {
                ResultsTextBox.Text = $"Error occurred: {ex.Message}\n\n{ex.StackTrace}";
            }
            finally
            {
                // Re-enable button
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
                // Pretty print the JSON representation of the result
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