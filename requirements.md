# Anti-Swearing Chat Box - Technical Requirements

## 1. System Architecture

### 1.1 Technology Stack
- Frontend: WPF (.NET 7.0) with MVVM architecture
- Backend: ASP.NET Core Web API
- Database: SQL Server
- Real-time Communication: SignalR
- Authentication: JWT with refresh tokens
- AI/ML: Python-based service with FastAPI
- UI Framework: MaterialDesignInXAML
- Dependency Injection: Microsoft.Extensions.DependencyInjection
- Logging: Serilog
- Testing: MSTest

### 1.2 System Components
1. WPF Client Application
   - Main Window
   - Login/Register Windows
   - Chat Window
   - Settings Window
2. Authentication Service
3. Chat Service
4. AI Moderation Service
5. Database Service
6. Notification Service

## 2. Core Features

### 2.1 User Interface
- Theme Support:
  - Dark Mode (Cyberpunk Pastel):
    - Primary Background: #1A1B2E (Deep Navy)
    - Secondary Background: #2A2B3E (Rich Purple)
    - Text Color: #E6E6FA (Lavender)
    - Accent Color: #FF69B4 (Hot Pink)
    - Secondary Accent: #00FFFF (Cyan)
    - Error Color: #FF6B6B (Soft Red)
    - Success Color: #98FB98 (Pale Green)
    - Neon Glow Effects:
      - Button Hover: #FF69B4 with 20% opacity glow
      - Text Highlight: #00FFFF with 15% opacity glow
      - Border Accents: #FF69B4 with 10% opacity glow
  - Light Mode (Pastel Cyber):
    - Primary Background: #F0F8FF (Alice Blue)
    - Secondary Background: #E6E6FA (Lavender)
    - Text Color: #2A2B3E (Deep Purple)
    - Accent Color: #FF69B4 (Hot Pink)
    - Secondary Accent: #00CED1 (Dark Turquoise)
    - Error Color: #FF6B6B (Soft Red)
    - Success Color: #98FB98 (Pale Green)
    - Neon Glow Effects:
      - Button Hover: #FF69B4 with 15% opacity glow
      - Text Highlight: #00CED1 with 10% opacity glow
      - Border Accents: #FF69B4 with 8% opacity glow
- Responsive Design:
  - Minimum Window Size: 800x600
  - Adaptive Layout
  - Grid-based UI
- Custom Controls:
  - Modern Chat Bubbles
  - Animated Loading Indicators
  - Custom Buttons
  - Custom TextBoxes
  - Custom ScrollBars
- UI Elements:
  - Chat Bubbles:
    - User Messages: Gradient from #FF69B4 to #FFB6C1 (Light Pink)
    - Other Messages: Gradient from #00FFFF to #00CED1 (Cyan)
    - Message Time: #E6E6FA (Lavender) in dark mode, #2A2B3E (Deep Purple) in light mode
  - Buttons:
    - Primary: #FF69B4 (Hot Pink) with neon glow
    - Secondary: #00FFFF (Cyan) with neon glow
    - Disabled: #808080 (Gray) with reduced opacity
  - Input Fields:
    - Background: #2A2B3E (Rich Purple) in dark mode, #F0F8FF (Alice Blue) in light mode
    - Border: #FF69B4 (Hot Pink) with 2px width
    - Focus Border: #00FFFF (Cyan) with 2px width and glow
  - Scrollbars:
    - Thumb: #FF69B4 (Hot Pink) with 20% opacity
    - Track: #1A1B2E (Deep Navy) in dark mode, #F0F8FF (Alice Blue) in light mode
- Visual Effects:
  - Neon Glow: Applied to interactive elements
  - Gradient Overlays: Subtle gradients for depth
  - Hover Animations: Smooth color transitions
  - Loading Spinner: Rotating neon gradient
  - Message Animations: Fade-in with slight glow
- Typography:
  - Headers: 'Orbitron' font for cyberpunk feel
  - Body Text: 'Roboto' for readability
  - Accent Text: 'Orbitron' for special elements
- Icons:
  - Line style with neon glow
  - Cyberpunk-inspired design
  - Animated state changes
- Layout:
  - Grid-based with neon grid lines
  - Rounded corners (8px radius)
  - Subtle shadows with neon tint
  - Responsive design with minimum 800x600 window size

### 2.2 User Management
- Registration:
  - Email validation
  - Password requirements: minimum 8 characters, 1 uppercase, 1 number, 1 special character
  - Username requirements: 3-20 characters, alphanumeric with underscores
  - Email verification required
- Authentication:
  - JWT-based authentication
  - Refresh token rotation
  - Session timeout: 30 minutes
  - Remember me functionality
- Profile Management:
  - Avatar upload (max 2MB, supported formats: JPG, PNG)
  - Profile information update
  - Password change
  - Account deletion

### 2.3 Chat System
- Real-time Messaging:
  - Message delivery confirmation
  - Typing indicators
  - Read receipts
  - Message status (sent, delivered, read)
- Message Features:
  - Text messages
  - Emoji support
  - Message deletion (within 5 minutes)
  - Message editing (within 5 minutes)
- Thread Management:
  - Thread creation with unique IDs
  - Thread status tracking (active, locked)
  - Thread history preservation
  - Thread search functionality

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
- Message delivery latency: < 100ms
- AI analysis latency: < 500ms
- System uptime: 99.9%
- Concurrent users: 10,000+
- Message storage: 7 years retention
- Database backup: Daily

### 3.2 Security Requirements
- HTTPS/TLS 1.3
- Data encryption at rest (AES-256)
- Input validation and sanitization
- XSS protection
- CSRF protection
- Rate limiting
- IP blocking after 5 failed login attempts

### 3.3 Database Schema
```sql
-- Users
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY,
    Username NVARCHAR(20) UNIQUE,
    Email NVARCHAR(255) UNIQUE,
    PasswordHash NVARCHAR(255),
    CreatedAt DATETIME2,
    LastLogin DATETIME2,
    IsActive BIT
);

-- ChatThreads
CREATE TABLE ChatThreads (
    ThreadId UNIQUEIDENTIFIER PRIMARY KEY,
    UserAId UNIQUEIDENTIFIER,
    UserBId UNIQUEIDENTIFIER,
    Status NVARCHAR(20),
    CreatedAt DATETIME2,
    LastActivity DATETIME2,
    WarningCount INT
);

-- Messages
CREATE TABLE Messages (
    MessageId UNIQUEIDENTIFIER PRIMARY KEY,
    ThreadId UNIQUEIDENTIFIER,
    SenderId UNIQUEIDENTIFIER,
    Content NVARCHAR(MAX),
    SentAt DATETIME2,
    Status NVARCHAR(20)
);

-- Warnings
CREATE TABLE Warnings (
    WarningId UNIQUEIDENTIFIER PRIMARY KEY,
    ThreadId UNIQUEIDENTIFIER,
    UserId UNIQUEIDENTIFIER,
    WarningLevel INT,
    Message NVARCHAR(MAX),
    CreatedAt DATETIME2
);
```

## 4. Project Structure
```
Anti-Swearing_Chat_Box/
├── Anti-Swearing_Chat_Box.sln
├── Anti-Swearing_Chat_Box/
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── Views/
│   │   ├── LoginWindow.xaml
│   │   ├── RegisterWindow.xaml
│   │   ├── ChatWindow.xaml
│   │   └── SettingsWindow.xaml
│   ├── ViewModels/
│   │   ├── LoginViewModel.cs
│   │   ├── RegisterViewModel.cs
│   │   ├── ChatViewModel.cs
│   │   └── SettingsViewModel.cs
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Message.cs
│   │   ├── ChatThread.cs
│   │   └── Warning.cs
│   ├── Services/
│   │   ├── AuthenticationService.cs
│   │   ├── ChatService.cs
│   │   ├── AIService.cs
│   │   └── NotificationService.cs
│   ├── Helpers/
│   │   ├── RelayCommand.cs
│   │   └── ThemeManager.cs
│   └── Resources/
│       ├── Styles/
│       │   ├── ButtonStyles.xaml
│       │   ├── TextBoxStyles.xaml
│       │   └── ChatBubbleStyles.xaml
│       └── Themes/
│           ├── DarkTheme.xaml
│           └── LightTheme.xaml
└── Anti-Swearing_Chat_Box.Tests/
    ├── ViewModelTests/
    ├── ServiceTests/
    └── ModelTests/
```

## 5. AI Model Specifications

### 5.1 Model Architecture
- Base model: BERT or RoBERTa
- Fine-tuning approach: Transfer learning
- Context window: 512 tokens
- Batch size: 32
- Learning rate: 2e-5

### 5.2 Training Data
- Dataset size: 100,000+ messages
- Class distribution: 80% clean, 20% offensive
- Data augmentation techniques
- Regular retraining schedule

### 5.3 Model Performance Metrics
- Accuracy: > 95%
- False positive rate: < 1%
- False negative rate: < 2%
- Inference time: < 100ms

## 6. Monitoring and Logging

### 6.1 Metrics
- Active users
- Message throughput
- AI analysis latency
- Error rates
- Warning frequency
- Thread lock rate

### 6.2 Logging
- Application logs
- Security logs
- AI model logs
- Performance metrics
- Error tracking

## 7. Testing Requirements

### 7.1 Test Types
- Unit tests
- Integration tests
- UI tests
- Performance tests
- Security tests

### 7.2 Test Coverage
- Code coverage: > 80%
- Critical path coverage: 100%
- UI component coverage: > 70% 