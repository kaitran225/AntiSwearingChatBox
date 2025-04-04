-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AntiSwearingChatBox')
BEGIN
    CREATE DATABASE AntiSwearingChatBox;
    PRINT 'Created database AntiSwearingChatBox';
END
GO

USE AntiSwearingChatBox;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
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
    PRINT 'Created table Users';
END
GO

-- Create ChatThreads table 
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatThreads')
BEGIN
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
        AutoDeleteAfterDays INT NULL,
        SwearingScore INT NOT NULL DEFAULT 0,
        IsClosed BIT NOT NULL DEFAULT 0
    );
    PRINT 'Created table ChatThreads with SwearingScore and IsClosed columns';
END
ELSE
BEGIN

    IF NOT EXISTS (SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID('ChatThreads') 
                    AND name = 'SwearingScore')
    BEGIN
        ALTER TABLE ChatThreads ADD SwearingScore INT NOT NULL DEFAULT 0;
        PRINT 'Added SwearingScore column to existing ChatThreads table';
    END
    IF NOT EXISTS (SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID('ChatThreads') 
                    AND name = 'IsClosed')
    BEGIN
        ALTER TABLE ChatThreads ADD IsClosed BIT NOT NULL DEFAULT 0;
        PRINT 'Added IsClosed column to existing ChatThreads table';
    END
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ThreadParticipants')
BEGIN
    CREATE TABLE ThreadParticipants (
        ThreadId INT NOT NULL,
        UserId INT NOT NULL,
        JoinedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_ThreadParticipants PRIMARY KEY (ThreadId, UserId),
        CONSTRAINT FK_ThreadParticipants_ChatThreads FOREIGN KEY (ThreadId) REFERENCES ChatThreads(ThreadId),
        CONSTRAINT FK_ThreadParticipants_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
    );
    PRINT 'Created table ThreadParticipants';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FilteredWords')
BEGIN
    CREATE TABLE FilteredWords (
        WordId INT IDENTITY(1,1) PRIMARY KEY,
        Word NVARCHAR(100) NOT NULL UNIQUE,
        SeverityLevel INT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT CHK_SeverityLevel CHECK (SeverityLevel >= 1 AND SeverityLevel <= 3)
    );
    PRINT 'Created table FilteredWords';
END
GO

-- Create MessageHistory table 
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MessageHistory')
BEGIN
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
    PRINT 'Created table MessageHistory';
END
GO

-- Create UserWarnings table 
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserWarnings')
BEGIN
    CREATE TABLE UserWarnings (
        WarningId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        ThreadId INT NOT NULL,
        WarningMessage NVARCHAR(500) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_UserWarnings_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
        CONSTRAINT FK_UserWarnings_ChatThreads FOREIGN KEY (ThreadId) REFERENCES ChatThreads(ThreadId)
    );
    PRINT 'Created table UserWarnings';
END
GO

-- Create indexes 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MessageHistory_ThreadId' AND object_id = OBJECT_ID('MessageHistory'))
BEGIN
    CREATE INDEX IX_MessageHistory_ThreadId ON MessageHistory(ThreadId);
    PRINT 'Created index IX_MessageHistory_ThreadId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MessageHistory_UserId' AND object_id = OBJECT_ID('MessageHistory'))
BEGIN
    CREATE INDEX IX_MessageHistory_UserId ON MessageHistory(UserId);
    PRINT 'Created index IX_MessageHistory_UserId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserWarnings_UserId' AND object_id = OBJECT_ID('UserWarnings'))
BEGIN
    CREATE INDEX IX_UserWarnings_UserId ON UserWarnings(UserId);
    PRINT 'Created index IX_UserWarnings_UserId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserWarnings_ThreadId' AND object_id = OBJECT_ID('UserWarnings'))
BEGIN
    CREATE INDEX IX_UserWarnings_ThreadId ON UserWarnings(ThreadId);
    PRINT 'Created index IX_UserWarnings_ThreadId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ThreadParticipants_UserId' AND object_id = OBJECT_ID('ThreadParticipants'))
BEGIN
    CREATE INDEX IX_ThreadParticipants_UserId ON ThreadParticipants(UserId);
    PRINT 'Created index IX_ThreadParticipants_UserId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ThreadParticipants_ThreadId' AND object_id = OBJECT_ID('ThreadParticipants'))
BEGIN
    CREATE INDEX IX_ThreadParticipants_ThreadId ON ThreadParticipants(ThreadId);
    PRINT 'Created index IX_ThreadParticipants_ThreadId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ChatThreads_IsClosed' AND object_id = OBJECT_ID('ChatThreads'))
BEGIN
    CREATE INDEX IX_ChatThreads_IsClosed ON ChatThreads(IsClosed);
    PRINT 'Created index IX_ChatThreads_IsClosed';
END

PRINT 'Database setup complete'; 