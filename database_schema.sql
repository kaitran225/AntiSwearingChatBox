-- Create Database
CREATE DATABASE AntiSwearingChatBox;
GO

USE AntiSwearingChatBox;
GO

-- Users Table
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(20) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastLogin DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1,
    IsAdmin BIT NOT NULL DEFAULT 0,
    CONSTRAINT CHK_Username CHECK (LEN(Username) >= 3 AND LEN(Username) <= 20),
    CONSTRAINT CHK_Email CHECK (Email LIKE '%@%.%')
);

-- AIModelSettings Table
CREATE TABLE AIModelSettings (
    SettingId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModelName NVARCHAR(50) NOT NULL,
    SensitivityLevel INT NOT NULL DEFAULT 5,
    WarningThreshold INT NOT NULL DEFAULT 3,
    IsActive BIT NOT NULL DEFAULT 1,
    LastUpdated DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_AIModelSettings_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId),
    CONSTRAINT CHK_SensitivityLevel CHECK (SensitivityLevel >= 1 AND SensitivityLevel <= 10),
    CONSTRAINT CHK_WarningThreshold CHECK (WarningThreshold >= 1 AND WarningThreshold <= 5)
);

-- BadWords Table
CREATE TABLE BadWords (
    WordId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Word NVARCHAR(50) NOT NULL UNIQUE,
    Severity INT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    AddedBy UNIQUEIDENTIFIER NOT NULL,
    AddedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_BadWords_AddedBy FOREIGN KEY (AddedBy) REFERENCES Users(UserId),
    CONSTRAINT CHK_Severity CHECK (Severity >= 1 AND Severity <= 3)
);

-- ChatThreads Table
CREATE TABLE ChatThreads (
    ThreadId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserAId UNIQUEIDENTIFIER NOT NULL,
    UserBId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastActivity DATETIME2 NOT NULL DEFAULT GETDATE(),
    WarningCount INT NOT NULL DEFAULT 0,
    IsLocked BIT NOT NULL DEFAULT 0,
    LockedBy UNIQUEIDENTIFIER,
    CONSTRAINT FK_ChatThreads_UserA FOREIGN KEY (UserAId) REFERENCES Users(UserId),
    CONSTRAINT FK_ChatThreads_UserB FOREIGN KEY (UserBId) REFERENCES Users(UserId),
    CONSTRAINT FK_ChatThreads_LockedBy FOREIGN KEY (LockedBy) REFERENCES Users(UserId),
    CONSTRAINT CHK_Status CHECK (Status IN ('Active', 'Locked')),
    CONSTRAINT CHK_WarningCount CHECK (WarningCount >= 0 AND WarningCount <= 3)
);

-- Messages Table
CREATE TABLE Messages (
    MessageId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ThreadId UNIQUEIDENTIFIER NOT NULL,
    SenderId UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Sent',
    IsDeleted BIT NOT NULL DEFAULT 0,
    IsFlagged BIT NOT NULL DEFAULT 0,
    FlaggedBy UNIQUEIDENTIFIER,
    FlaggedAt DATETIME2,
    CONSTRAINT FK_Messages_Thread FOREIGN KEY (ThreadId) REFERENCES ChatThreads(ThreadId),
    CONSTRAINT FK_Messages_Sender FOREIGN KEY (SenderId) REFERENCES Users(UserId),
    CONSTRAINT FK_Messages_FlaggedBy FOREIGN KEY (FlaggedBy) REFERENCES Users(UserId),
    CONSTRAINT CHK_Status CHECK (Status IN ('Sent', 'Delivered', 'Read'))
);

-- Warnings Table
CREATE TABLE Warnings (
    WarningId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ThreadId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    WarningLevel INT NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_Warnings_Thread FOREIGN KEY (ThreadId) REFERENCES ChatThreads(ThreadId),
    CONSTRAINT FK_Warnings_User FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Warnings_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT CHK_WarningLevel CHECK (WarningLevel >= 1 AND WarningLevel <= 3)
);

-- Create Indexes
CREATE INDEX IX_Messages_ThreadId ON Messages(ThreadId);
CREATE INDEX IX_Messages_SenderId ON Messages(SenderId);
CREATE INDEX IX_Messages_SentAt ON Messages(SentAt);
CREATE INDEX IX_Warnings_ThreadId ON Warnings(ThreadId);
CREATE INDEX IX_Warnings_UserId ON Warnings(UserId);
CREATE INDEX IX_ChatThreads_UserAId ON ChatThreads(UserAId);
CREATE INDEX IX_ChatThreads_UserBId ON ChatThreads(UserBId);
CREATE INDEX IX_BadWords_Word ON BadWords(Word);
CREATE INDEX IX_AIModelSettings_IsActive ON AIModelSettings(IsActive);

-- Create Trigger for LastActivity
CREATE TRIGGER TR_UpdateLastActivity
ON Messages
AFTER INSERT
AS
BEGIN
    UPDATE ChatThreads
    SET LastActivity = GETDATE()
    FROM ChatThreads c
    INNER JOIN inserted i ON c.ThreadId = i.ThreadId;
END;
GO

-- Admin Stored Procedures
CREATE PROCEDURE sp_UpdateAIModelSettings
    @ModelName NVARCHAR(50),
    @SensitivityLevel INT,
    @WarningThreshold INT,
    @UpdatedBy UNIQUEIDENTIFIER
AS
BEGIN
    UPDATE AIModelSettings
    SET 
        SensitivityLevel = @SensitivityLevel,
        WarningThreshold = @WarningThreshold,
        LastUpdated = GETDATE(),
        UpdatedBy = @UpdatedBy
    WHERE ModelName = @ModelName;
END;
GO

CREATE PROCEDURE sp_AddBadWord
    @Word NVARCHAR(50),
    @Severity INT,
    @AddedBy UNIQUEIDENTIFIER
AS
BEGIN
    INSERT INTO BadWords (Word, Severity, AddedBy)
    VALUES (@Word, @Severity, @AddedBy);
END;
GO

CREATE PROCEDURE sp_GetFlaggedMessages
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SELECT 
        m.MessageId,
        m.Content,
        m.SentAt,
        m.IsFlagged,
        m.FlaggedAt,
        u1.Username AS SenderUsername,
        u2.Username AS FlaggedByUsername,
        ct.ThreadId
    FROM Messages m
    JOIN Users u1 ON m.SenderId = u1.UserId
    JOIN Users u2 ON m.FlaggedBy = u2.UserId
    JOIN ChatThreads ct ON m.ThreadId = ct.ThreadId
    WHERE m.IsFlagged = 1
    ORDER BY m.FlaggedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE PROCEDURE sp_GetActiveAIModelSettings
AS
BEGIN
    SELECT 
        SettingId,
        ModelName,
        SensitivityLevel,
        WarningThreshold,
        IsActive,
        LastUpdated,
        u.Username AS UpdatedByUsername
    FROM AIModelSettings ams
    JOIN Users u ON ams.UpdatedBy = u.UserId
    WHERE ams.IsActive = 1;
END;
GO

CREATE PROCEDURE sp_GetBadWordsList
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SELECT 
        WordId,
        Word,
        Severity,
        IsActive,
        AddedAt,
        u.Username AS AddedByUsername
    FROM BadWords bw
    JOIN Users u ON bw.AddedBy = u.UserId
    ORDER BY bw.AddedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- User Stored Procedures
CREATE PROCEDURE sp_GetUserChatThreads
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SELECT 
        ct.ThreadId,
        ct.CreatedAt,
        ct.LastActivity,
        ct.Status,
        ct.WarningCount,
        ct.IsLocked,
        CASE 
            WHEN ct.UserAId = @UserId THEN u2.Username
            ELSE u1.Username
        END AS OtherUsername
    FROM ChatThreads ct
    JOIN Users u1 ON ct.UserAId = u1.UserId
    JOIN Users u2 ON ct.UserBId = u2.UserId
    WHERE ct.UserAId = @UserId OR ct.UserBId = @UserId
    ORDER BY ct.LastActivity DESC;
END;
GO

CREATE PROCEDURE sp_GetThreadMessages
    @ThreadId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SELECT 
        m.MessageId,
        m.Content,
        m.SentAt,
        m.Status,
        m.IsDeleted,
        m.IsFlagged,
        u.Username AS SenderUsername
    FROM Messages m
    JOIN Users u ON m.SenderId = u.UserId
    WHERE m.ThreadId = @ThreadId
    ORDER BY m.SentAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO 