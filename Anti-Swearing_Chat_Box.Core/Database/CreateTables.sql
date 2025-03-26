-- Create Users table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastLoginAt DATETIME2,
    TrustScore DECIMAL(3,2) NOT NULL DEFAULT 1.00,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT CHK_TrustScore CHECK (TrustScore >= 0 AND TrustScore <= 1)
);

-- Create Threads table
CREATE TABLE Threads (
    ThreadId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastMessageAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Create ThreadParticipants table (Many-to-Many between Users and Threads)
CREATE TABLE ThreadParticipants (
    ThreadId INT NOT NULL,
    UserId INT NOT NULL,
    JoinedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_ThreadParticipants PRIMARY KEY (ThreadId, UserId),
    CONSTRAINT FK_ThreadParticipants_Threads FOREIGN KEY (ThreadId) REFERENCES Threads(ThreadId),
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
    CONSTRAINT FK_MessageHistory_Threads FOREIGN KEY (ThreadId) REFERENCES Threads(ThreadId),
    CONSTRAINT FK_MessageHistory_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Create UserWarnings table
CREATE TABLE UserWarnings (
    WarningId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    WarningMessage NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UserWarnings_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Create indexes for better performance
CREATE INDEX IX_MessageHistory_ThreadId ON MessageHistory(ThreadId);
CREATE INDEX IX_MessageHistory_UserId ON MessageHistory(UserId);
CREATE INDEX IX_UserWarnings_UserId ON UserWarnings(UserId);
CREATE INDEX IX_ThreadParticipants_UserId ON ThreadParticipants(UserId);
CREATE INDEX IX_ThreadParticipants_ThreadId ON ThreadParticipants(ThreadId); 