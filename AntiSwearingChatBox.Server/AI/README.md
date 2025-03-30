# Anti-Swearing Chat Box AI Module

This module provides integration with Google's Gemini AI API for text generation and chat message moderation.

## Overview

The AI module provides the following functionality:
- Text generation capabilities via Gemini AI
- Message moderation to detect and replace inappropriate language
- REST API endpoints for external applications to use these features

## Configuration

Add your Gemini API key to your application's configuration:

```json
{
  "GeminiSettings": {
    "ApiKey": "YOUR_API_KEY",
    "ModelName": "gemini-1.5-pro"
  }
}
```

The default API key is already set, but it's recommended to use your own API key in production.

## Integration

Add the Gemini services to your application's service collection in your `Program.cs` or `Startup.cs`:

```csharp
using AntiSwearingChatBox.AI;

// Add Gemini services
services.AddGeminiServices(Configuration);
```

## API Endpoints

The module exposes the following API endpoints:

### Generate Text

```
POST /api/gemini/generate
Content-Type: application/json

{
  "prompt": "Your text prompt here"
}
```

### Moderate Chat Message

```
POST /api/gemini/moderate
Content-Type: application/json

{
  "message": "Message to be moderated"
}
```

## Direct Service Usage

You can also inject and use the `GeminiService` directly in your code:

```csharp
using AntiSwearingChatBox.AI;

public class YourService
{
    private readonly GeminiService _geminiService;

    public YourService(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<string> ProcessUserMessage(string message)
    {
        // Check and moderate the message content
        string moderatedMessage = await _geminiService.ModerateChatMessageAsync(message);
        return moderatedMessage;
    }
}
``` 