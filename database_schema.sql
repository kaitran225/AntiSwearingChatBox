-- Create Database
CREATE DATABASE AntiSwearingChatBox;
GO

USE AntiSwearingChatBox;
GO

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(20) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    DisplayName NVARCHAR(50) NOT NULL,
    AvatarPath NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLogin DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE UserSettings (
    UserSettingsId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    IsDarkTheme BIT NOT NULL DEFAULT 1,
    NotificationsEnabled BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_UserSettings_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE Contacts (
    ContactId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ContactUserId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending/Accepted/Blocked
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Contacts_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Contacts_ContactUsers FOREIGN KEY (ContactUserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_Contacts_UserContact UNIQUE (UserId, ContactUserId)
);

CREATE TABLE Conversations (
    ConversationId INT IDENTITY(1,1) PRIMARY KEY,
    StartedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastActivity DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsLocked BIT NOT NULL DEFAULT 0,
    WarningCount INT NOT NULL DEFAULT 0
);

CREATE TABLE ConversationParticipants (
    ParticipantId INT IDENTITY(1,1) PRIMARY KEY,
    ConversationId INT NOT NULL,
    UserId INT NOT NULL,
    JoinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ConversationParticipants_Conversations FOREIGN KEY (ConversationId) REFERENCES Conversations(ConversationId),
    CONSTRAINT FK_ConversationParticipants_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_ConversationParticipants UNIQUE (ConversationId, UserId)
);

CREATE TABLE Messages (
    MessageId INT IDENTITY(1,1) PRIMARY KEY,
    ConversationId INT NOT NULL,
    SenderId INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsEdited BIT NOT NULL DEFAULT 0,
    EditedAt DATETIME2 NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME2 NULL,
    WasAnalyzed BIT NOT NULL DEFAULT 0,
    ContainsBadWords BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Messages_Conversations FOREIGN KEY (ConversationId) REFERENCES Conversations(ConversationId),
    CONSTRAINT FK_Messages_Users FOREIGN KEY (SenderId) REFERENCES Users(UserId)
);

CREATE TABLE Warnings (
    WarningId INT IDENTITY(1,1) PRIMARY KEY,
    ConversationId INT NOT NULL,
    MessageId INT NOT NULL,
    UserId INT NOT NULL,
    WarningLevel INT NOT NULL, -- 1, 2, or 3
    Reason NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Warnings_Conversations FOREIGN KEY (ConversationId) REFERENCES Conversations(ConversationId),
    CONSTRAINT FK_Warnings_Messages FOREIGN KEY (MessageId) REFERENCES Messages(MessageId),
    CONSTRAINT FK_Warnings_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE INDEX IX_Messages_ConversationId ON Messages(ConversationId);
CREATE INDEX IX_Messages_SenderId ON Messages(SenderId);
CREATE INDEX IX_Messages_SentAt ON Messages(SentAt);
CREATE INDEX IX_Messages_ContainsBadWords ON Messages(ContainsBadWords);
CREATE INDEX IX_ConversationParticipants_UserId ON ConversationParticipants(UserId);
CREATE INDEX IX_ConversationParticipants_ConversationId ON ConversationParticipants(ConversationId);
CREATE INDEX IX_Contacts_UserId ON Contacts(UserId);
CREATE INDEX IX_Contacts_ContactUserId ON Contacts(ContactUserId);
CREATE INDEX IX_Contacts_Status ON Contacts(Status);
CREATE INDEX IX_Conversations_IsLocked ON Conversations(IsLocked);
CREATE INDEX IX_Conversations_LastActivity ON Conversations(LastActivity);