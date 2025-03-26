# Anti-Swearing Chat Box Console Test Application

This console application is designed to test the AI moderation functionality of the Anti-Swearing Chat Box application directly, without requiring the full UI.

## Features

- Test the Gemini AI text generation capabilities
- Test the message moderation functionality that detects and replaces inappropriate language
- Uses the same AI service as the main application

## How to Use

1. Build the application:
   ```
   dotnet build
   ```

2. Run the application:
   ```
   dotnet run
   ```

3. Follow the on-screen menu to select test options:
   - Option 1: Test Text Generation - Enter a prompt and see the AI-generated response
   - Option 2: Test Message Moderation - Enter a message and see how the AI moderates it
   - Option 3: Exit the application

## Configuration

The application uses the `appsettings.json` file to configure the Gemini AI service:

```json
{
  "GeminiSettings": {
    "ApiKey": "YOUR_API_KEY",
    "ModelName": "gemini-1.5-pro"
  }
}
```

You can modify this file to use your own API key or change the model if needed.

## Dependencies

- Mscc.GenerativeAI - Google's Gemini API client
- Microsoft.Extensions.Configuration - For loading configuration from appsettings.json
- Anti-Swearing_Chat_Box.AI - The AI module from the main application

## Testing Tips

When testing message moderation, try including inappropriate language or swear words to see how the AI moderates the content. The application will show both the original message and the moderated version for comparison. 