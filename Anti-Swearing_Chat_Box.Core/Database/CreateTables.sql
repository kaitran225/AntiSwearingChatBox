-- Create Users table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastLoginAt DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1,
    TrustScore DECIMAL(3,2) NOT NULL DEFAULT 1.00,
    CONSTRAINT CHK_TrustScore CHECK (TrustScore >= 0 AND TrustScore <= 1)
);

-- Create ModerationSettings table
CREATE TABLE ModerationSettings (
    SettingId INT IDENTITY(1,1) PRIMARY KEY,
    LanguageCode NVARCHAR(10) NOT NULL,
    SensitivityLevel NVARCHAR(20) NOT NULL,
    IsEnabled BIT NOT NULL DEFAULT 1,
    LastUpdated DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy INT,
    CONSTRAINT FK_ModerationSettings_Users FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId)
);

-- Create FilteringRules table
CREATE TABLE FilteringRules (
    RuleId INT IDENTITY(1,1) PRIMARY KEY,
    RuleType NVARCHAR(50) NOT NULL,
    IsEnabled BIT NOT NULL DEFAULT 1,
    SensitivityLevel DECIMAL(3,2) NOT NULL DEFAULT 0.5,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastUpdated DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedBy INT,
    CONSTRAINT FK_FilteringRules_Users FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId),
    CONSTRAINT CHK_SensitivityLevel CHECK (SensitivityLevel >= 0 AND SensitivityLevel <= 1)
);

-- Create AllowedExceptions table
CREATE TABLE AllowedExceptions (
    ExceptionId INT IDENTITY(1,1) PRIMARY KEY,
    RuleId INT NOT NULL,
    Term NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AllowedExceptions_FilteringRules FOREIGN KEY (RuleId) REFERENCES FilteringRules(RuleId)
);

-- Create AlwaysFilterTerms table
CREATE TABLE AlwaysFilterTerms (
    TermId INT IDENTITY(1,1) PRIMARY KEY,
    RuleId INT NOT NULL,
    Term NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AlwaysFilterTerms_FilteringRules FOREIGN KEY (RuleId) REFERENCES FilteringRules(RuleId)
);

-- Create UserWarnings table
CREATE TABLE UserWarnings (
    WarningId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    WarningType NVARCHAR(50) NOT NULL,
    WarningLevel NVARCHAR(20) NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    ExpiresAt DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_UserWarnings_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Create MessageHistory table
CREATE TABLE MessageHistory (
    MessageId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    OriginalMessage NVARCHAR(MAX) NOT NULL,
    ModeratedMessage NVARCHAR(MAX),
    WasModified BIT NOT NULL DEFAULT 0,
    ContainsProfanity BIT NOT NULL DEFAULT 0,
    SentimentScore DECIMAL(3,2),
    ToxicityLevel DECIMAL(3,2),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_MessageHistory_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT CHK_SentimentScore CHECK (SentimentScore >= 0 AND SentimentScore <= 1),
    CONSTRAINT CHK_ToxicityLevel CHECK (ToxicityLevel >= 0 AND ToxicityLevel <= 1)
);

-- Create DetectedTerms table
CREATE TABLE DetectedTerms (
    DetectionId INT IDENTITY(1,1) PRIMARY KEY,
    MessageId INT NOT NULL,
    Term NVARCHAR(100) NOT NULL,
    TermType NVARCHAR(50) NOT NULL,
    ConfidenceScore DECIMAL(3,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_DetectedTerms_MessageHistory FOREIGN KEY (MessageId) REFERENCES MessageHistory(MessageId),
    CONSTRAINT CHK_ConfidenceScore CHECK (ConfidenceScore >= 0 AND ConfidenceScore <= 1)
);

-- Create UserReputation table
CREATE TABLE UserReputation (
    ReputationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ReputationScore DECIMAL(3,2) NOT NULL DEFAULT 1.00,
    LastCalculatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UserReputation_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT CHK_ReputationScore CHECK (ReputationScore >= 0 AND ReputationScore <= 1)
);

-- Create indexes for better performance
CREATE INDEX IX_UserWarnings_UserId ON UserWarnings(UserId);
CREATE INDEX IX_UserWarnings_CreatedAt ON UserWarnings(CreatedAt);
CREATE INDEX IX_MessageHistory_UserId ON MessageHistory(UserId);
CREATE INDEX IX_MessageHistory_CreatedAt ON MessageHistory(CreatedAt);
CREATE INDEX IX_DetectedTerms_MessageId ON DetectedTerms(MessageId);
CREATE INDEX IX_DetectedTerms_Term ON DetectedTerms(Term); 