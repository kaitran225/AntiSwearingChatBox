USE AntiSwearingChatBox;
GO

-- Check if the SwearingScore column already exists
IF NOT EXISTS (SELECT * FROM sys.columns 
                WHERE object_id = OBJECT_ID('ChatThreads') 
                AND name = 'SwearingScore')
BEGIN
    -- Add SwearingScore column to ChatThreads table
    ALTER TABLE ChatThreads ADD SwearingScore INT NOT NULL DEFAULT 0;
    PRINT 'Added SwearingScore column to ChatThreads table';
END
ELSE
BEGIN
    PRINT 'SwearingScore column already exists';
END

-- Check if the IsClosed column already exists
IF NOT EXISTS (SELECT * FROM sys.columns 
                WHERE object_id = OBJECT_ID('ChatThreads') 
                AND name = 'IsClosed')
BEGIN
    -- Add IsClosed column to ChatThreads table
    ALTER TABLE ChatThreads ADD IsClosed BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsClosed column to ChatThreads table';
END
ELSE
BEGIN
    PRINT 'IsClosed column already exists';
END

-- Check if the index already exists
IF NOT EXISTS (SELECT * FROM sys.indexes 
                WHERE name = 'IX_ChatThreads_IsClosed' 
                AND object_id = OBJECT_ID('ChatThreads'))
BEGIN
    -- Add an index to improve query performance for frequent lookups
    CREATE INDEX IX_ChatThreads_IsClosed ON ChatThreads(IsClosed);
    PRINT 'Created index IX_ChatThreads_IsClosed';
END
ELSE
BEGIN
    PRINT 'Index IX_ChatThreads_IsClosed already exists';
END 