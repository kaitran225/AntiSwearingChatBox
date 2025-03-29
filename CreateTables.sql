IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AntiSwearingChatBox')
BEGIN
    CREATE DATABASE AntiSwearingChatBox;
END
GO

USE AntiSwearingChatBox;
GO

-- Create Users table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    VerificationToken NVARCHAR(100) NULL,
    ResetToken NVARCHAR(100) NULL,
    Gender NVARCHAR(10) NULL,
    IsVerified BIT NOT NULL DEFAULT 0,
    TokenExpiration DATETIME2 NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastLoginAt DATETIME2,
    TrustScore DECIMAL(3,2) NOT NULL DEFAULT 1.00,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT CHK_TrustScore CHECK (TrustScore >= 0 AND TrustScore <= 1),
    CONSTRAINT CHK_Gender CHECK (Gender IN ('Male', 'Female', 'Other', NULL)),
    CONSTRAINT CHK_Role CHECK (Role IN ('Admin', 'Moderator', 'User'))
);

-- Create ChatThreads table
CREATE TABLE ChatThreads (
    ThreadId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastMessageAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    IsPrivate BIT NOT NULL DEFAULT 0,
    AllowAnonymous BIT NOT NULL DEFAULT 0,
    ModerationEnabled BIT NOT NULL DEFAULT 1,
    MaxParticipants INT NULL,
    AutoDeleteAfterDays INT NULL
);

-- Create ThreadParticipants table (Many-to-Many between Users and ChatThreads)
CREATE TABLE ThreadParticipants (
    ThreadId INT NOT NULL,
    UserId INT NOT NULL,
    JoinedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_ThreadParticipants PRIMARY KEY (ThreadId, UserId),
    CONSTRAINT FK_ThreadParticipants_ChatThreads FOREIGN KEY (ThreadId) REFERENCES ChatThreads(ThreadId),
    CONSTRAINT FK_ThreadParticipants_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Create FilteredWords table
CREATE TABLE FilteredWords (
    WordId INT IDENTITY(1,1) PRIMARY KEY,
    Word NVARCHAR(100) NOT NULL UNIQUE,
    SeverityLevel INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CHK_SeverityLevel CHECK (SeverityLevel >= 1 AND SeverityLevel <= 3)
);

-- Create MessageHistory table
CREATE TABLE MessageHistory (
    MessageId INT IDENTITY(1,1) PRIMARY KEY,
    ThreadId INT NOT NULL,
    UserId INT NOT NULL,
    OriginalMessage NVARCHAR(MAX) NOT NULL,
    ModeratedMessage NVARCHAR(MAX),
    WasModified BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_MessageHistory_ChatThreads FOREIGN KEY (ThreadId) REFERENCES ChatThreads(ThreadId),
    CONSTRAINT FK_MessageHistory_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Create UserWarnings table
CREATE TABLE UserWarnings (
    WarningId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ThreadId INT NOT NULL,
    WarningMessage NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UserWarnings_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_UserWarnings_ChatThreads FOREIGN KEY (ThreadId) REFERENCES ChatThreads(ThreadId)
);

-- Create indexes for better performance
CREATE INDEX IX_MessageHistory_ThreadId ON MessageHistory(ThreadId);
CREATE INDEX IX_MessageHistory_UserId ON MessageHistory(UserId);
CREATE INDEX IX_UserWarnings_UserId ON UserWarnings(UserId);
CREATE INDEX IX_UserWarnings_ThreadId ON UserWarnings(ThreadId);
CREATE INDEX IX_ThreadParticipants_UserId ON ThreadParticipants(UserId);
CREATE INDEX IX_ThreadParticipants_ThreadId ON ThreadParticipants(ThreadId);