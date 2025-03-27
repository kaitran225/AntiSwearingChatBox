# Anti-Swearing Chat Box - Technical Requirements

## 1. System Architecture

### 1.1 Technology Stack
- Frontend: WPF (.NET 9.0) with MVVM architecture
- Backend: ASP.NET Core Web API
- Database: SQL Server
- Real-time Communication: SignalR with persistent connections
- Authentication: JWT with refresh tokens
- AI/ML: Python-based service with FastAPI
- UI Framework: MaterialDesignInXAML
- Dependency Injection: Microsoft.Extensions.DependencyInjection
- Logging: Serilog
- Message Queue: RabbitMQ (for offline message delivery)

### 1.2 System Components
1. WPF Client Application
   - Main Window
   - Login/Register Windows
   - Chat Window (primary focus)
   - Settings Window
2. Authentication Service
3. Chat Service (central component)
4. Presence Service (online status tracking)
5. AI Moderation Service
6. Database Service
7. Notification Service
8. Message Queue Service

## 2. Core Features

### 2.1 User Interface
- Color System:
  - Primary Colors:
    - #A280FF (Bright Purple) - Primary brand color, main buttons, active states
    - #8F66FF (Royal Purple) - Secondary actions, links, highlights
    - #7C4CFF (Deep Purple) - Tertiary actions, focus states
    - #6933FF (Deep Purple) - Special actions, important UI elements
    
  - Accent Colors:
    - #47D068 (Bright Green) - Success states, online status, positive actions
    - #C7B3FF (Light Purple) - Subtle highlights, secondary information
    - #F8F5FF (Light Purple/White) - Background in light mode, text in dark mode
    
  - Neutral Colors:
    - #FFFFFF (Pure White) - Pure white backgrounds, high contrast elements
    - #B3B3B3 (Gray) - Secondary text, disabled states
    - #616161 (Dark Gray) - Tertiary text, borders in dark mode
    - #333333 (Very Dark Gray) - Primary text in light mode
    - #1C1C1C (Almost Black) - Secondary background in dark mode
    - #191919 (Dark Gray) - Alternative dark backgrounds
    - #0E0E0E (Black) - Primary background in dark mode
    
  - Transparent Colors:
    - #D9D9D9 15% - Subtle overlays, disabled backgrounds
    - #FFFFFF 51% - Light overlays, hover states in light mode
    - #8F66FF 50% - Purple overlays, focus indicators
    - #8F66FF 12% - Very subtle purple accents
    - #8F66FF 88% - Strong purple overlays, active states

- Theme Support:
  - Dark Mode:
    - Primary Background: #0E0E0E (Black)
    - Secondary Background: #1C1C1C (Almost Black)
    - Text Color: #F8F5FF (Light Purple/White)
    - Accent Colors:
      - #47D068 (Bright Green)
      - #A280FF (Bright Purple)
      - #8F66FF (Royal Purple)
      - #7C4CFF (Deep Purple)
    - Status Colors:
      - Online: #47D068 (Bright Green)
      - Error: #FF4D4D (Red)
      - Success: #47D068 (Bright Green)
    - Subtle Effects:
      - Button Hover: rgba(162, 128, 255, 0.1)
      - Text Highlight: rgba(143, 102, 255, 0.05)
      - Border Accents: rgba(124, 76, 255, 0.1)
      - Overlays: rgba(217, 217, 217, 0.15)
      - Focus States: rgba(143, 102, 255, 0.88)
  
  - Light Mode:
    - Primary Background: #F8F5FF (Light Purple/White)
    - Secondary Background: #FFFFFF (Pure White)
    - Text Color: #333333 (Very Dark Gray)
    - Accent Colors:
      - #47D068 (Bright Green)
      - #A280FF (Bright Purple)
      - #8F66FF (Royal Purple)
      - #7C4CFF (Deep Purple)
    - Status Colors:
      - Online: #47D068 (Bright Green)
      - Error: #FF4D4D (Red)
      - Success: #47D068 (Bright Green)
    - Subtle Effects:
      - Button Hover: rgba(143, 102, 255, 0.12)
      - Text Highlight: rgba(143, 102, 255, 0.5)
      - Border Accents: rgba(143, 102, 255, 0.88)
      - Overlays: rgba(255, 255, 255, 0.51)
      - Focus States: rgba(143, 102, 255, 0.88)

- Responsive Design:
  - Minimum Window Size: 800x600
  - Adaptive Layout
  - Grid-based UI
- Custom Controls:
  - Modern Chat Bubbles
  - Loading Indicators
  - Material Design Buttons
  - Custom TextBoxes
  - Custom ScrollBars
- UI Elements:
  - Chat Bubbles:
    - User Messages: #A280FF (Bright Purple) with #FFFFFF text
    - Other Messages: #F8F5FF (Light Purple/White) with #333333 text
    - System Messages: #7C4CFF (Deep Purple) with #FFFFFF text
    - Message Time: #B3B3B3 (Gray)
    - Hover State: rgba(143, 102, 255, 0.12)
  - Buttons:
    - Primary: #8F66FF (Royal Purple)
    - Secondary: #7C4CFF (Deep Purple)
    - Disabled: #616161 (Dark Gray)
    - Hover: rgba(143, 102, 255, 0.88)
    - Focus Ring: rgba(143, 102, 255, 0.5)
  - Input Fields:
    - Background: #F8F5FF (Light Purple/White) in light mode, #1C1C1C (Almost Black) in dark mode
    - Border: #A280FF (Bright Purple)
    - Focus Border: #8F66FF (Royal Purple)
    - Placeholder Text: #B3B3B3 (Gray)
    - Disabled: rgba(217, 217, 217, 0.15)
  - Scrollbars:
    - Thumb: rgba(162, 128, 255, 0.5)
    - Track: #F8F5FF (Light Purple/White) in light mode, #0E0E0E (Black) in dark mode
    - Hover: rgba(143, 102, 255, 0.88)
- Visual Effects:
  - Subtle Shadows: For depth and elevation
  - Smooth Transitions: For state changes
  - Loading Spinner: Simple rotating animation
  - Message Animations: Fade-in effect
- Typography:
  - Headers: 'Segoe UI' for modern look
  - Body Text: 'Segoe UI' for readability
  - Accent Text: 'Segoe UI' with different weights
- Icons:
  - Material Design icons
  - Clean, minimal style
  - Consistent with theme
- Layout:
  - Grid-based layout
  - Rounded corners (4px radius)
  - Subtle shadows
  - Responsive design with minimum 800x600 window size

### 2.2 User Authentication & Management
- Registration:
  - Email validation
  - Password requirements: minimum 8 characters, 1 uppercase, 1 number, 1 special character
  - Username requirements: 3-20 characters, alphanumeric with underscores
  - Email verification with token
  - Account activation upon email verification
  - Prevention of duplicate usernames and emails
  - Timeout for verification (24 hours)
- Authentication:
  - JWT-based authentication
  - Access token (short-lived, 30 minutes)
  - Refresh token rotation (long-lived, 14 days)
  - Secure token storage
  - Token validation middleware
  - Session timeout after 30 minutes of inactivity
  - Remember me functionality (extends refresh token validity)
  - Protection against brute force attacks
  - Account lockout after 5 failed attempts
- Password Management:
  - Secure password reset flow
  - Reset token expiration (1 hour)
  - Password change validation
  - Password strength meter on registration/change
  - Prevention of password reuse
- Session Management:
  - Concurrent session handling
  - Forced logout for security concerns
  - Activity tracking
  - Login history with IP and device information
- Profile Management:
  - Profile information update
  - Password change
  - Account deletion

### 2.3 Realtime Chat System
- Connection Management:
  - Persistent connections using SignalR
  - Automatic reconnection with exponential backoff
  - Connection state tracking (connected, connecting, disconnected)
  - Multi-device support for same user account
  - Graceful connection handling across network changes

- Presence System:
  - Real-time online status indicators
  - Custom status messages
  - Away/Busy/Available states
  - Last seen timestamps for offline users
  - Typing indicators with debounce control
  - Read receipts with timestamp

- Message Handling:
  - Instant message delivery (<100ms latency target)
  - Message delivery confirmation
  - Message status tracking (sent, delivered, read)
  - Unread message counters
  - Message synchronization across devices
  - Offline message queueing and delivery upon reconnection
  - Message history loading with pagination (50 messages per request)
  - Real-time message updates for editing/deletion

- Message Features:
  - Text messages with rich formatting
  - Emoji support with picker
  - Message reactions (likes, etc.)
  - Message threading and replies
  - Message editing (within 5 minutes)
  - Message deletion (within 5 minutes)
  - Message search with highlighting

- Group Chat:
  - Group creation and management
  - Member roles (admin, moderator, member)
  - Group invitations
  - Real-time updates when members join/leave
  - Group settings and permissions
  - Group message delivery guarantees

- Thread Management:
  - Thread creation with unique IDs
  - Thread status tracking (active, locked)
  - Thread history preservation
  - Thread search functionality
  - Thread notifications
  - Thread list with sorting options (recent, unread)

- UI Real-time Elements:
  - Live typing indicators
  - Message status indicators
  - Real-time user status updates
  - Animated message transitions
  - Unread message highlights
  - New message notifications
  - Real-time thread updates

### 2.4 AI Moderation System
- Content Analysis:
  - Real-time message scanning
  - Context-aware analysis
  - Multi-language support (initial: English)
  - False positive prevention
- Warning System:
  - Warning levels (1-3)
  - Warning message templates
  - Warning history tracking
  - Warning appeal process
- Thread Locking:
  - Automatic thread closure after 3 warnings
  - Lock status persistence
  - New thread creation facilitation
  - Lock history tracking

## 3. Technical Specifications

### 3.1 Performance Requirements
- Authentication response time: < 500ms
- Token validation time: < 100ms
- API response time: < 200ms
- Real-time message delivery: < 100ms
- Connection establishment: < 2s
- Reconnection time: < 5s
- Message synchronization: < 1s
- System uptime: 99.9%
- Support for 10,000+ concurrent users
- Efficient handling of 1000+ messages per second

### 3.2 Security Requirements
- HTTPS/TLS 1.3
- Data encryption at rest (AES-256)
- Password hashing with bcrypt or Argon2
- CSRF protection with anti-forgery tokens
- XSS protection
- Input validation and sanitization
- Rate limiting for authentication attempts
- IP blocking after 5 failed login attempts
- Secure HTTP headers (X-XSS-Protection, Content-Security-Policy)
- JWT with appropriate signature algorithm (RS256)
- Secure cookie settings (HttpOnly, Secure, SameSite)

### 3.3 Database Schema
```sql
-- Users
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(20) UNIQUE NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    VerificationToken NVARCHAR(255),
    ResetToken NVARCHAR(255),
    Gender NVARCHAR(50),
    IsVerified BIT NOT NULL DEFAULT 0,
    TokenExpiration DATETIME2,
    Role NVARCHAR(50) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME2 NOT NULL,
    LastLoginAt DATETIME2,
    LastSeenAt DATETIME2,
    CurrentStatus NVARCHAR(20) DEFAULT 'Offline',
    CustomStatusMessage NVARCHAR(100),
    TrustScore DECIMAL(5,2) NOT NULL DEFAULT 100.00,
    IsActive BIT NOT NULL DEFAULT 1
);

-- RefreshTokens
CREATE TABLE RefreshTokens (
    TokenId UNIQUEIDENTIFIER PRIMARY KEY,
    UserId INT NOT NULL,
    Token NVARCHAR(255) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    CreatedByIp NVARCHAR(45),
    RevokedAt DATETIME2,
    RevokedByIp NVARCHAR(45),
    ReplacedByToken NVARCHAR(255),
    ReasonRevoked NVARCHAR(255),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- LoginHistory
CREATE TABLE LoginHistory (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId INT NOT NULL,
    LoginTime DATETIME2 NOT NULL,
    IpAddress NVARCHAR(45),
    UserAgent NVARCHAR(255),
    IsSuccessful BIT NOT NULL,
    FailureReason NVARCHAR(255),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- UserConnections
CREATE TABLE UserConnections (
    ConnectionId NVARCHAR(128) PRIMARY KEY,
    UserId INT NOT NULL,
    ConnectionStartTime DATETIME2 NOT NULL,
    LastHeartbeat DATETIME2 NOT NULL,
    UserAgent NVARCHAR(255),
    DeviceType NVARCHAR(50),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- ChatThreads
CREATE TABLE ChatThreads (
    ThreadId UNIQUEIDENTIFIER PRIMARY KEY,
    ThreadName NVARCHAR(100),
    ThreadType NVARCHAR(20) NOT NULL, -- 'Private', 'Group'
    CreatorId INT,
    Status NVARCHAR(20) NOT NULL, -- 'Active', 'Locked', 'Archived'
    CreatedAt DATETIME2 NOT NULL,
    LastActivity DATETIME2 NOT NULL,
    WarningCount INT DEFAULT 0,
    IsEncrypted BIT DEFAULT 0,
    FOREIGN KEY (CreatorId) REFERENCES Users(UserId)
);

-- ThreadParticipants
CREATE TABLE ThreadParticipants (
    ParticipantId UNIQUEIDENTIFIER PRIMARY KEY,
    ThreadId UNIQUEIDENTIFIER NOT NULL,
    UserId INT NOT NULL,
    Role NVARCHAR(20) DEFAULT 'Member', -- 'Admin', 'Moderator', 'Member'
    JoinedAt DATETIME2 NOT NULL,
    LeftAt DATETIME2,
    LastReadMessageId UNIQUEIDENTIFIER,
    NotificationPreference NVARCHAR(20) DEFAULT 'All', -- 'All', 'Mentions', 'None'
    FOREIGN KEY (ThreadId) REFERENCES ChatThreads(ThreadId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Messages
CREATE TABLE Messages (
    MessageId UNIQUEIDENTIFIER PRIMARY KEY,
    ThreadId UNIQUEIDENTIFIER NOT NULL,
    SenderId INT NOT NULL,
    ReplyToId UNIQUEIDENTIFIER,
    Content NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 NOT NULL,
    EditedAt DATETIME2,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Sent', -- 'Sent', 'Delivered', 'Read', 'Failed', 'Deleted'
    IsModerated BIT DEFAULT 0,
    ModeratedContent NVARCHAR(MAX),
    FOREIGN KEY (ThreadId) REFERENCES ChatThreads(ThreadId),
    FOREIGN KEY (SenderId) REFERENCES Users(UserId),
    FOREIGN KEY (ReplyToId) REFERENCES Messages(MessageId)
);

-- MessageDeliveryStatus
CREATE TABLE MessageDeliveryStatus (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MessageId UNIQUEIDENTIFIER NOT NULL,
    UserId INT NOT NULL,
    DeliveredAt DATETIME2,
    ReadAt DATETIME2,
    FOREIGN KEY (MessageId) REFERENCES Messages(MessageId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- MessageReactions
CREATE TABLE MessageReactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MessageId UNIQUEIDENTIFIER NOT NULL,
    UserId INT NOT NULL,
    ReactionType NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (MessageId) REFERENCES Messages(MessageId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Warnings
CREATE TABLE Warnings (
    WarningId UNIQUEIDENTIFIER PRIMARY KEY,
    ThreadId UNIQUEIDENTIFIER NOT NULL,
    UserId INT NOT NULL,
    MessageId UNIQUEIDENTIFIER,
    WarningLevel INT NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    CreatedById INT NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Active', -- 'Active', 'Resolved', 'Appealed'
    FOREIGN KEY (ThreadId) REFERENCES ChatThreads(ThreadId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (MessageId) REFERENCES Messages(MessageId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
);
```

## 4. Authentication Workflow

### 4.1 Registration Flow
1. User enters email, username, and password
2. Client validates input format (email format, password strength)
3. Submit request to registration endpoint
4. Server validates input and checks for duplicate username/email
5. Server creates user with verification token
6. Server sends verification email with token
7. User clicks verification link
8. Server validates token and marks account as verified
9. User redirected to login page with success message

### 4.2 Login Flow
1. User enters username/email and password
2. Client sends credentials to authentication endpoint
3. Server validates credentials
4. If valid:
   - Generate JWT access token (30 min expiry)
   - Generate refresh token (14 days expiry)
   - Store refresh token in database
   - Return both tokens to client
   - Record successful login
5. If invalid:
   - Return error message
   - Increment failed login counter
   - Lock account if threshold reached
   - Record failed login attempt

### 4.3 Token Refresh Flow
1. Access token expires
2. Client sends refresh token to refresh endpoint
3. Server validates refresh token
4. If valid:
   - Revoke old refresh token
   - Generate new access token
   - Generate new refresh token
   - Return new tokens to client
5. If invalid:
   - Return error requiring re-authentication

### 4.4 Password Reset Flow
1. User requests password reset
2. Server generates reset token
3. Server sends email with reset link
4. User clicks link and enters new password
5. Server validates token and updates password
6. User redirected to login with success message

## 5. Realtime Chat Implementation

### 5.1 SignalR Hub Architecture
```csharp
public class ChatHub : Hub
{
    // Connection management
    public override async Task OnConnectedAsync() { ... }
    public override async Task OnDisconnectedAsync(Exception exception) { ... }
    
    // Presence methods
    public async Task UpdateStatus(string status) { ... }
    public async Task NotifyTyping(string threadId) { ... }
    
    // Messaging methods
    public async Task SendMessage(string threadId, string content, string replyToId = null) { ... }
    public async Task MarkMessageAsRead(string messageId) { ... }
    public async Task EditMessage(string messageId, string newContent) { ... }
    public async Task DeleteMessage(string messageId) { ... }
    public async Task AddReaction(string messageId, string reactionType) { ... }
    
    // Thread management
    public async Task CreateThread(string name, List<string> participantIds) { ... }
    public async Task AddToThread(string threadId, List<string> userIds) { ... }
    public async Task LeaveThread(string threadId) { ... }
}
```

### 5.2 Client Connection Management
```csharp
public class SignalRService
{
    private HubConnection _connection;
    private readonly ITokenService _tokenService;
    private readonly ILogger<SignalRService> _logger;
    
    // Connection setup
    public async Task InitializeConnection() { ... }
    
    // Reconnection logic
    private async Task ReconnectWithBackoff() { ... }
    
    // Message handling
    public async Task SendMessage(MessageDto message) { ... }
    
    // Event handlers
    private void RegisterEventHandlers() { ... }
    
    // Connection state management
    public ConnectionState GetConnectionState() { ... }
}
```

### 5.3 Offline Message Handling
1. When a client goes offline:
   - Server marks client connection as inactive
   - Messages are stored in database with 'Sent' status
   - Critical messages are queued in RabbitMQ for guaranteed delivery

2. When a client reconnects:
   - Client requests all undelivered messages since last connection
   - Server retrieves pending messages and sends in batches
   - Client acknowledges message receipt
   - Server updates message status to 'Delivered'

3. Message synchronization:
   - Client maintains local message store
   - Periodic sync to handle missing messages
   - Conflict resolution for edited messages

### 5.4 Scaling Considerations
- Multiple SignalR instances with sticky sessions
- Redis backplane for SignalR with Azure SignalR Service option
- Message deduplication mechanisms
- Separate read/write database operations
- Thread/channel sharding for high-volume scenarios
- Caching of frequently accessed data (user presence, thread metadata)

## 6. Project Structure
```
AntiSwearingChatBox/
├── AntiSwearingChatBox.Core/           # Core domain and business logic
│   ├── Models/                         # Domain models
│   ├── Interfaces/                     # Core interfaces
│   ├── Constants/                      # Application constants
│   └── Database/                       # Database-related components
│
├── AntiSwearingChatBox.Service/        # Service implementations
│   ├── IServices/                      # Service interfaces
│   │   ├── IAuthenticationService.cs
│   │   ├── ITokenService.cs
│   │   ├── IUserService.cs
│   │   ├── IChatService.cs
│   │   ├── IPresenceService.cs
│   │   ├── IMessageService.cs
│   │   └── ...
│   └── Services/                       # Service implementations
│       ├── AuthenticationService.cs
│       ├── TokenService.cs
│       ├── UserService.cs
│       ├── ChatService.cs
│       ├── PresenceService.cs
│       ├── SignalRService.cs
│       └── ...
│
├── AntiSwearingChatBox.Repository/     # Data access layer
│   ├── Context/                        # Database context
│   ├── Migrations/                     # Database migrations
│   └── Models/                         # Database models
│       ├── User.cs
│       ├── RefreshToken.cs
│       ├── Message.cs
│       ├── ChatThread.cs
│       └── ...
│
├── AntiSwearingChatBox.AI/             # AI/ML components
│   ├── Services/                       # AI services
│   └── Models/                         # AI models
│
└── AntiSwearingChatBox.App/            # WPF UI Layer
    ├── Views/                          # WPF Views
    │   ├── MainWindow.xaml
    │   ├── LoginWindow.xaml
    │   ├── RegisterWindow.xaml
    │   ├── ChatWindow.xaml
    │   ├── ThreadListView.xaml
    │   ├── MessageView.xaml
    │   └── ...
    ├── ViewModels/                     # ViewModels
    │   ├── BaseViewModel.cs
    │   ├── MainViewModel.cs
    │   ├── LoginViewModel.cs
    │   ├── RegisterViewModel.cs
    │   ├── ChatViewModel.cs
    │   ├── ThreadViewModel.cs
    │   ├── MessageViewModel.cs
    │   └── ...
    ├── Controls/                       # Custom controls
    │   ├── ChatBubble.xaml
    │   ├── TypingIndicator.xaml
    │   ├── StatusIndicator.xaml
    │   ├── UserAvatar.xaml
    │   └── ...
    ├── Themes/                         # Theme resources
    └── Resources/                      # Other resources
```

## 7. Service Interfaces

### 7.1 IAuthenticationService
```csharp
public interface IAuthenticationService
{
    Task<AuthenticationResponse> AuthenticateAsync(string username, string password, string ipAddress);
    Task<AuthenticationResponse> RefreshTokenAsync(string token, string ipAddress);
    Task RevokeTokenAsync(string token, string ipAddress);
    Task<bool> RegisterUserAsync(RegisterRequest model);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string password);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}
```

### 7.2 ITokenService
```csharp
public interface ITokenService
{
    string GenerateJwtToken(User user);
    RefreshToken GenerateRefreshToken(string ipAddress);
    Task<User> ValidateTokenAsync(string token);
    Task<bool> ValidateRefreshTokenAsync(int userId, string token);
    Task<RefreshToken> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string ipAddress, string reason = "Revoked without replacement");
}
```

### 7.3 IChatService
```csharp
public interface IChatService
{
    Task<IEnumerable<ChatThreadDto>> GetUserThreadsAsync(int userId);
    Task<ChatThreadDto> GetThreadByIdAsync(string threadId);
    Task<ChatThreadDto> CreateThreadAsync(string name, int creatorId, IEnumerable<int> participantIds, bool isGroup);
    Task<bool> AddUsersToThreadAsync(string threadId, IEnumerable<int> userIds);
    Task<bool> RemoveUserFromThreadAsync(string threadId, int userId);
    Task<bool> UpdateThreadStatusAsync(string threadId, string status);
    Task<PagedResult<MessageDto>> GetThreadMessagesAsync(string threadId, int page, int pageSize);
    Task<MessageDto> GetMessageByIdAsync(string messageId);
    Task<IEnumerable<MessageDto>> GetUnreadMessagesAsync(int userId);
}
```

### 7.4 IMessageService
```csharp
public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(string threadId, int senderId, string content, string replyToId = null);
    Task<bool> UpdateMessageStatusAsync(string messageId, int userId, string status);
    Task<bool> MarkMessageAsReadAsync(string messageId, int userId);
    Task<bool> MarkThreadMessagesAsReadAsync(string threadId, int userId);
    Task<MessageDto> EditMessageAsync(string messageId, int userId, string newContent);
    Task<bool> DeleteMessageAsync(string messageId, int userId);
    Task<bool> AddReactionAsync(string messageId, int userId, string reactionType);
    Task<bool> RemoveReactionAsync(string messageId, int userId, string reactionType);
    Task<IEnumerable<MessageDeliveryStatusDto>> GetMessageStatusesAsync(string messageId);
}
```

### 7.5 IPresenceService
```csharp
public interface IPresenceService
{
    Task<bool> UpdateUserStatusAsync(int userId, string status, string customMessage = null);
    Task<string> GetUserStatusAsync(int userId);
    Task<DateTime?> GetLastSeenAsync(int userId);
    Task<bool> UpdateLastSeenAsync(int userId);
    Task<bool> NotifyTypingAsync(string threadId, int userId);
    Task<IEnumerable<UserStatusDto>> GetUsersStatusAsync(IEnumerable<int> userIds);
    Task<IEnumerable<UserConnectionDto>> GetUserConnectionsAsync(int userId);
    Task<bool> DisconnectUserAsync(int userId, string connectionId);
}
```

## 8. Monitoring and Logging

### 8.1 Authentication Metrics
- Login success rate
- Registration completion rate
- Failed login attempts
- Password reset requests
- Token refresh frequency
- Account lockouts

### 8.2 Realtime Communication Metrics
- Connection success rate
- Reconnection frequency
- Message delivery latency
- Message delivery success rate
- SignalR connection count
- Concurrent users
- Connection duration
- Message throughput
- Hub method invocation rate
- Error rate by method

### 8.3 Logging
- Authentication events (success/failure)
- Token issuance and validation
- Connection events (connect, disconnect, reconnect)
- Message delivery events
- Thread creation and updates
- Warning and moderation events
- Performance bottlenecks
- Error conditions with context 