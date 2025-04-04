-- Add SwearingScore and IsClosed columns to ChatThreads table
ALTER TABLE ChatThreads ADD SwearingScore INT NOT NULL DEFAULT 0;
ALTER TABLE ChatThreads ADD IsClosed BIT NOT NULL DEFAULT 0;

-- Update any existing threads with high swearing scores to be closed
-- This would be done if we had data about swearing scores, but for now just initialize everything

-- Add an index to improve query performance for frequent lookups
CREATE INDEX IX_ChatThreads_IsClosed ON ChatThreads(IsClosed); 