# Anti-Swearing Chat Box - Technical Requirements

## 1. System Architecture

### 1.1 Technology Stack
- Frontend: WPF (.NET 9.0) with MVVM architecture
- Backend: ASP.NET Core Web API
- Database: SQL Server
- Real-time Communication: SignalR
- Authentication: JWT with refresh tokens
- AI/ML: Python-based service with FastAPI
- UI Framework: MaterialDesignInXAML
- Dependency Injection: Microsoft.Extensions.DependencyInjection
- Logging: Serilog

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
├── Anti-Swearing_Chat_Box.Core/           # Core domain and business logic
│   ├── Models/                            # Domain models
│   │   ├── User.cs
│   │   ├── Message.cs
│   │   ├── ChatThread.cs
│   │   └── Warning.cs
│   ├── Interfaces/                        # Core interfaces
│   │   ├── IAuthenticationService.cs
│   │   ├── IChatService.cs
│   │   ├── IAIService.cs
│   │   └── INotificationService.cs
│   └── Constants/                         # Application constants
│       └── AppConstants.cs
│
├── Anti-Swearing_Chat_Box.Services/       # Service implementations
│   ├── Authentication/                    # Authentication services
│   │   ├── AuthenticationService.cs
│   │   └── JwtService.cs
│   ├── Chat/                              # Chat services
│   │   ├── ChatService.cs
│   │   └── SignalRService.cs
│   └── Notification/                      # Notification services
│       └── NotificationService.cs
│
├── Anti-Swearing_Chat_Box.Repositories/   # Data access layer
│   ├── Context/                           # Database context
│   │   └── ApplicationDbContext.cs
│   ├── Repositories/                      # Repository implementations
│   │   ├── UserRepository.cs
│   │   ├── MessageRepository.cs
│   │   └── ChatThreadRepository.cs
│   └── Migrations/                        # Database migrations
│
├── Anti-Swearing_Chat_Box.AI/            # AI/ML components
│   ├── Services/                          # AI services
│   │   ├── AIService.cs
│   │   └── ContentAnalysisService.cs
│   ├── Models/                            # AI models
│   │   └── ContentAnalysisModel.cs
│   └── Utilities/                         # AI utilities
│       └── TextProcessing.cs
│
└── Anti-Swearing_Chat_Box.Presentation/   # WPF UI Layer
    ├── Views/                             # WPF Views
    │   ├── MainWindow.xaml
    │   ├── LoginWindow.xaml
    │   ├── RegisterWindow.xaml
    │   ├── ChatWindow.xaml
    │   └── SettingsWindow.xaml
    ├── ViewModels/                        # ViewModels
    │   ├── BaseViewModel.cs
    │   ├── MainViewModel.cs
    │   ├── LoginViewModel.cs
    │   ├── RegisterViewModel.cs
    │   ├── ChatViewModel.cs
    │   └── SettingsViewModel.cs
    ├── Controls/                          # Custom controls
    │   ├── ChatBubble.xaml
    │   ├── LoadingSpinner.xaml
    │   └── CustomButton.xaml
    ├── Themes/                            # Theme resources
    │   ├── DarkTheme.xaml
    │   └── LightTheme.xaml
    └── Resources/                         # Other resources
        ├── Styles/
        └── Icons/
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