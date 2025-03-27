-- Create database if not exists
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

----------------------------------
-- INITIALIZATION DATA
----------------------------------

-- Insert Users
INSERT INTO Users (Username, Email, PasswordHash, Gender, IsVerified, Role, CreatedAt, LastLoginAt, TrustScore, IsActive)
VALUES 
('john_doe', 'john.doe@cybriadev.com', 'hash1234567890', 'Male', 1, 'Admin', '2023-01-01', '2024-06-01', 1.00, 1),
('jane_smith', 'jane.smith@cybriadev.com', 'hash1234567890', 'Female', 1, 'Moderator', '2023-01-15', '2024-06-01', 0.95, 1),
('mike_jones', 'mike.jones@cybriadev.com', 'hash1234567890', 'Male', 1, 'User', '2023-02-01', '2024-05-30', 0.90, 1),
('sarah_conner', 'sarah.conner@cybriadev.com', 'hash1234567890', 'Female', 1, 'User', '2023-02-15', '2024-05-29', 0.92, 1),
('alex_walker', 'alex.walker@cybriadev.com', 'hash1234567890', 'Male', 1, 'User', '2023-03-01', '2024-05-28', 0.88, 1),
('lisa_jones', 'lisa.jones@cybriadev.com', 'hash1234567890', 'Female', 1, 'User', '2023-03-15', '2024-05-27', 0.91, 1),
('david_brown', 'david.brown@cybriadev.com', 'hash1234567890', 'Male', 1, 'User', '2023-04-01', '2024-05-26', 0.85, 1),
('emma_davis', 'emma.davis@cybriadev.com', 'hash1234567890', 'Female', 1, 'User', '2023-04-15', '2024-05-25', 0.93, 1),
('michael_green', 'michael.green@cybriadev.com', 'hash1234567890', 'Male', 1, 'Moderator', '2023-05-01', '2024-05-24', 0.97, 1),
('olivia_wilson', 'olivia.wilson@cybriadev.com', 'hash1234567890', 'Female', 1, 'User', '2023-05-15', '2024-05-23', 0.89, 1);

-- Insert FilteredWords
INSERT INTO FilteredWords (Word, SeverityLevel, CreatedAt)
VALUES 
('damn', 1, '2023-01-05'),
('hell', 1, '2023-01-05'),
('ass', 2, '2023-01-05'),
('shit', 2, '2023-01-05'),
('fuck', 3, '2023-01-05'),
('bitch', 3, '2023-01-05'),
('crap', 1, '2023-03-10'),
('wtf', 2, '2023-03-10'),
('bastard', 2, '2023-04-15'),
('dumbass', 2, '2023-04-15');

-- Insert ChatThreads (3 per user = 30 threads)
INSERT INTO ChatThreads (Title, CreatedAt, LastMessageAt, IsActive, IsPrivate, AllowAnonymous, ModerationEnabled)
VALUES
-- John's threads
('Welcome to the Team', '2023-01-02', '2024-05-30', 1, 0, 0, 1),
('Project Alpha Discussion', '2023-01-05', '2024-05-29', 1, 0, 0, 1),
('Admin Channel', '2023-01-10', '2024-06-01', 1, 1, 0, 1),

-- Jane's threads
('UI/UX Team Chat', '2023-01-20', '2024-05-28', 1, 0, 0, 1),
('Design Review', '2023-01-25', '2024-05-27', 1, 0, 0, 1),
('Moderators Only', '2023-02-01', '2024-06-01', 1, 1, 0, 1),

-- Mike's threads
('Sports Discussion', '2023-02-05', '2024-05-26', 1, 0, 1, 1),
('Weekend Plans', '2023-02-10', '2024-05-25', 1, 0, 0, 1),
('Coding Help', '2023-02-15', '2024-05-30', 1, 0, 0, 1),

-- Sarah's threads
('Book Club', '2023-02-20', '2024-05-24', 1, 0, 0, 1),
('Movie Recommendations', '2023-02-25', '2024-05-23', 1, 0, 0, 1),
('Fitness Goals', '2023-03-01', '2024-05-22', 1, 0, 0, 1),

-- Alex's threads
('Gaming Squad', '2023-03-05', '2024-05-21', 1, 0, 0, 1),
('Tech News', '2023-03-10', '2024-05-20', 1, 0, 0, 1),
('Travel Plans', '2023-03-15', '2024-05-19', 1, 0, 0, 1),

-- Lisa's threads
('Recipe Exchange', '2023-03-20', '2024-05-18', 1, 0, 0, 1),
('Parenting Tips', '2023-03-25', '2024-05-17', 1, 0, 0, 1),
('Home Improvement', '2023-04-01', '2024-05-16', 1, 0, 0, 1),

-- David's threads
('Car Enthusiasts', '2023-04-05', '2024-05-15', 1, 0, 0, 1),
('Music Sharing', '2023-04-10', '2024-05-14', 1, 0, 0, 1),
('Hiking Group', '2023-04-15', '2024-05-13', 1, 0, 0, 1),

-- Emma's threads
('Art Club', '2023-04-20', '2024-05-12', 1, 0, 0, 1),
('Poetry Corner', '2023-04-25', '2024-05-11', 1, 0, 0, 1),
('Photography Tips', '2023-05-01', '2024-05-10', 1, 0, 0, 1),

-- Michael's threads
('Mod Announcements', '2023-05-05', '2024-06-01', 1, 0, 0, 1),
('Community Guidelines', '2023-05-10', '2024-05-09', 1, 0, 0, 1),
('New Features Discussion', '2023-05-15', '2024-05-08', 1, 0, 0, 1),

-- Olivia's threads
('Fashion Trends', '2023-05-20', '2024-05-07', 1, 0, 0, 1),
('Beauty Tips', '2023-05-25', '2024-05-06', 1, 0, 0, 1),
('Mental Health Support', '2023-06-01', '2024-05-05', 1, 0, 0, 1);

-- Insert ThreadParticipants
-- For simplicity, let's make the creator a participant in their own threads
-- and add 2-3 more participants to each thread

-- John's threads participants
INSERT INTO ThreadParticipants (ThreadId, UserId, JoinedAt)
VALUES
-- Thread 1: Welcome to the Team (John, Jane, Mike, Sarah, Alex)
(1, 1, '2023-01-02'), -- John (creator)
(1, 2, '2023-01-02'), -- Jane
(1, 3, '2023-01-02'), -- Mike
(1, 4, '2023-01-02'), -- Sarah
(1, 5, '2023-01-02'), -- Alex

-- Thread 2: Project Alpha Discussion (John, Jane, David, Emma)
(2, 1, '2023-01-05'), -- John (creator)
(2, 2, '2023-01-05'), -- Jane
(2, 7, '2023-01-05'), -- David
(2, 8, '2023-01-05'), -- Emma

-- Thread 3: Admin Channel (John, Jane, Michael)
(3, 1, '2023-01-10'), -- John (creator)
(3, 2, '2023-01-10'), -- Jane
(3, 9, '2023-01-10'); -- Michael

-- Continue with similar patterns for all 30 threads...

-- Add sample messages to each thread (at least 10 per thread)
-- Here's an example for the first thread "Welcome to the Team"
INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
-- Thread 1: Welcome to the Team
(1, 1, 'Welcome everyone to our new team chat!', 'Welcome everyone to our new team chat!', 0, '2023-01-02 09:00:00'),
(1, 2, 'Thanks for setting this up, John!', 'Thanks for setting this up, John!', 0, '2023-01-02 09:05:00'),
(1, 3, 'Happy to be here! Looking forward to working with you all.', 'Happy to be here! Looking forward to working with you all.', 0, '2023-01-02 09:10:00'),
(1, 4, 'Hello everyone! Excited to collaborate on this project.', 'Hello everyone! Excited to collaborate on this project.', 0, '2023-01-02 09:15:00'),
(1, 5, 'This is going to be a damn good team!', 'This is going to be a **** good team!', 1, '2023-01-02 09:20:00'),
(1, 1, 'Let''s review our goals for the first quarter.', 'Let''s review our goals for the first quarter.', 0, '2023-01-02 09:25:00'),
(1, 2, 'I can prepare a presentation for our next meeting.', 'I can prepare a presentation for our next meeting.', 0, '2023-01-02 09:30:00'),
(1, 3, 'That sounds great Jane. When is the meeting scheduled?', 'That sounds great Jane. When is the meeting scheduled?', 0, '2023-01-02 09:35:00'),
(1, 1, 'Let''s aim for this Friday at 10 AM.', 'Let''s aim for this Friday at 10 AM.', 0, '2023-01-02 09:40:00'),
(1, 4, 'Works for me. I''ll prepare my section of the report.', 'Works for me. I''ll prepare my section of the report.', 0, '2023-01-02 09:45:00'),
(1, 5, 'Friday is perfect. I'll be ready with my updates.', 'Friday is perfect. I'll be ready with my updates.', 0, '2023-01-02 09:50:00'),
(1, 2, 'Great! I''ll send a calendar invite to everyone.', 'Great! I''ll send a calendar invite to everyone.', 0, '2023-01-02 09:55:00');

-- Add messages for Thread 2: Project Alpha Discussion
INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(2, 1, 'This thread is dedicated to Project Alpha discussions.', 'This thread is dedicated to Project Alpha discussions.', 0, '2023-01-05 10:00:00'),
(2, 2, 'I''ve uploaded the project specifications to the shared drive.', 'I''ve uploaded the project specifications to the shared drive.', 0, '2023-01-05 10:05:00'),
(2, 7, 'Thanks Jane, I''ll review them today.', 'Thanks Jane, I''ll review them today.', 0, '2023-01-05 10:10:00'),
(2, 8, 'Any specific sections I should focus on first?', 'Any specific sections I should focus on first?', 0, '2023-01-05 10:15:00'),
(2, 1, 'Emma, please review the user authentication requirements.', 'Emma, please review the user authentication requirements.', 0, '2023-01-05 10:20:00'),
(2, 2, 'And David, could you look at the database schema?', 'And David, could you look at the database schema?', 0, '2023-01-05 10:25:00'),
(2, 7, 'Will do. I might have some questions about the schema later.', 'Will do. I might have some questions about the schema later.', 0, '2023-01-05 10:30:00'),
(2, 8, 'I see some potential issues with the auth flow. I''ll document them.', 'I see some potential issues with the auth flow. I''ll document them.', 0, '2023-01-05 10:35:00'),
(2, 1, 'That would be helpful, Emma. Let''s discuss in our next meeting.', 'That would be helpful, Emma. Let''s discuss in our next meeting.', 0, '2023-01-05 10:40:00'),
(2, 7, 'This database design is shit. Who came up with this?', 'This database design is ****. Who came up with this?', 1, '2023-01-05 10:45:00'),
(2, 2, 'David, please be constructive. What specific issues do you see?', 'David, please be constructive. What specific issues do you see?', 0, '2023-01-05 10:50:00'),
(2, 7, 'Sorry about that. I see normalization issues in the user-role relationship.', 'Sorry about that. I see normalization issues in the user-role relationship.', 0, '2023-01-05 10:55:00');

-- Add messages for Thread 3: Admin Channel
INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(3, 1, 'This is a private channel for admins and moderators only.', 'This is a private channel for admins and moderators only.', 0, '2023-01-10 11:00:00'),
(3, 2, 'Got it. What do we need to discuss?', 'Got it. What do we need to discuss?', 0, '2023-01-10 11:05:00'),
(3, 9, 'I think we should review our moderation policies.', 'I think we should review our moderation policies.', 0, '2023-01-10 11:10:00'),
(3, 1, 'Agreed. I''ve noticed some inconsistencies in how we''re handling filtered words.', 'Agreed. I''ve noticed some inconsistencies in how we''re handling filtered words.', 0, '2023-01-10 11:15:00'),
(3, 2, 'Yeah, we should standardize the approach.', 'Yeah, we should standardize the approach.', 0, '2023-01-10 11:20:00'),
(3, 9, 'I suggest we categorize words by severity levels.', 'I suggest we categorize words by severity levels.', 0, '2023-01-10 11:25:00'),
(3, 1, 'That''s a good idea. Level 1 could be mild, Level 2 moderate, and Level 3 severe.', 'That''s a good idea. Level 1 could be mild, Level 2 moderate, and Level 3 severe.', 0, '2023-01-10 11:30:00'),
(3, 2, 'And we could have different actions based on the levels.', 'And we could have different actions based on the levels.', 0, '2023-01-10 11:35:00'),
(3, 9, 'Level 1: Warning, Level 2: Temporary mute, Level 3: Ban consideration.', 'Level 1: Warning, Level 2: Temporary mute, Level 3: Ban consideration.', 0, '2023-01-10 11:40:00'),
(3, 1, 'Perfect. Let''s document this and share with the mod team.', 'Perfect. Let''s document this and share with the mod team.', 0, '2023-01-10 11:45:00'),
(3, 2, 'I can draft up the documentation by tomorrow.', 'I can draft up the documentation by tomorrow.', 0, '2023-01-10 11:50:00'),
(3, 9, 'Thanks Jane. I''ll review it when it''s ready.', 'Thanks Jane. I''ll review it when it''s ready.', 0, '2023-01-10 11:55:00');

-- Add a few UserWarnings for demonstration
INSERT INTO UserWarnings (UserId, ThreadId, WarningMessage, CreatedAt)
VALUES
(5, 1, 'Warning for using inappropriate language. Please maintain professional communication.', '2023-01-02 09:21:00'),
(7, 2, 'Please be constructive in your criticism and avoid offensive language.', '2023-01-05 10:46:00'),
(3, 7, 'Your message contained inappropriate language. Please review our community guidelines.', '2023-02-06 14:15:00');

-- Add participants for Jane's threads
INSERT INTO ThreadParticipants (ThreadId, UserId, JoinedAt)
VALUES
-- Thread 4: UI/UX Team Chat (Jane, John, Emma, Olivia)
(4, 2, '2023-01-20'), -- Jane (creator)
(4, 1, '2023-01-20'), -- John
(4, 8, '2023-01-20'), -- Emma
(4, 10, '2023-01-20'), -- Olivia

-- Thread 5: Design Review (Jane, Emma, Alex, Lisa)
(5, 2, '2023-01-25'), -- Jane (creator)
(5, 8, '2023-01-25'), -- Emma
(5, 5, '2023-01-25'), -- Alex
(5, 6, '2023-01-25'), -- Lisa

-- Thread 6: Moderators Only (Jane, John, Michael)
(6, 2, '2023-02-01'), -- Jane (creator)
(6, 1, '2023-02-01'), -- John
(6, 9, '2023-02-01'); -- Michael

-- Add participants for Mike's threads
INSERT INTO ThreadParticipants (ThreadId, UserId, JoinedAt)
VALUES
-- Thread 7: Sports Discussion (Mike, Alex, David, John)
(7, 3, '2023-02-05'), -- Mike (creator)
(7, 5, '2023-02-05'), -- Alex
(7, 7, '2023-02-05'), -- David
(7, 1, '2023-02-05'), -- John

-- Thread 8: Weekend Plans (Mike, Sarah, Lisa, Olivia)
(8, 3, '2023-02-10'), -- Mike (creator)
(8, 4, '2023-02-10'), -- Sarah
(8, 6, '2023-02-10'), -- Lisa
(8, 10, '2023-02-10'), -- Olivia

-- Thread 9: Coding Help (Mike, Emma, David, Michael)
(9, 3, '2023-02-15'), -- Mike (creator)
(9, 8, '2023-02-15'), -- Emma
(9, 7, '2023-02-15'), -- David
(9, 9, '2023-02-15'); -- Michael

-- Add messages for Thread 4: UI/UX Team Chat
INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(4, 2, 'Welcome to the UI/UX Team Chat! This is where we''ll discuss design ideas and get feedback.', 'Welcome to the UI/UX Team Chat! This is where we''ll discuss design ideas and get feedback.', 0, '2023-01-20 13:00:00'),
(4, 1, 'Thanks for setting this up, Jane. We needed a dedicated space for design discussions.', 'Thanks for setting this up, Jane. We needed a dedicated space for design discussions.', 0, '2023-01-20 13:05:00'),
(4, 8, 'I''ve been working on some wireframes for the new features. Should I share them here?', 'I''ve been working on some wireframes for the new features. Should I share them here?', 0, '2023-01-20 13:10:00'),
(4, 2, 'Yes, please do! That would be great.', 'Yes, please do! That would be great.', 0, '2023-01-20 13:15:00'),
(4, 10, 'Looking forward to seeing them, Emma. I have some ideas for the color scheme too.', 'Looking forward to seeing them, Emma. I have some ideas for the color scheme too.', 0, '2023-01-20 13:20:00'),
(4, 8, 'Here are the initial mockups: [Attachment: wireframes_v1.png]', 'Here are the initial mockups: [Attachment: wireframes_v1.png]', 0, '2023-01-20 13:25:00'),
(4, 1, 'These look promising! But I think the navigation is a bit cluttered.', 'These look promising! But I think the navigation is a bit cluttered.', 0, '2023-01-20 13:30:00'),
(4, 10, 'I agree with John. Maybe we could simplify the top menu and move some options to a sidebar?', 'I agree with John. Maybe we could simplify the top menu and move some options to a sidebar?', 0, '2023-01-20 13:35:00'),
(4, 8, 'That makes sense. I''ll work on an alternative layout.', 'That makes sense. I''ll work on an alternative layout.', 0, '2023-01-20 13:40:00'),
(4, 2, 'Great discussion everyone! Emma, when do you think you can have the revised wireframes?', 'Great discussion everyone! Emma, when do you think you can have the revised wireframes?', 0, '2023-01-20 13:45:00'),
(4, 8, 'I should have them ready by tomorrow afternoon.', 'I should have them ready by tomorrow afternoon.', 0, '2023-01-20 13:50:00'),
(4, 10, 'This damn navigation has been a pain point for users for months. Glad we''re finally addressing it.', 'This **** navigation has been a pain point for users for months. Glad we''re finally addressing it.', 1, '2023-01-20 13:55:00');

-- Add messages for Thread 7: Sports Discussion
INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(7, 3, 'Hey everyone! Created this thread for sports talk. What teams are you all following?', 'Hey everyone! Created this thread for sports talk. What teams are you all following?', 0, '2023-02-05 15:00:00'),
(7, 5, 'Big Lakers fan here. Been following them since the Kobe era.', 'Big Lakers fan here. Been following them since the Kobe era.', 0, '2023-02-05 15:05:00'),
(7, 7, 'I''m more into football. Chiefs all the way!', 'I''m more into football. Chiefs all the way!', 0, '2023-02-05 15:10:00'),
(7, 1, 'I enjoy tennis more than team sports. Nadal is my favorite player.', 'I enjoy tennis more than team sports. Nadal is my favorite player.', 0, '2023-02-05 15:15:00'),
(7, 3, 'Nice variety! I''m a basketball and soccer fan myself.', 'Nice variety! I''m a basketball and soccer fan myself.', 0, '2023-02-05 15:20:00'),
(7, 5, 'Did anyone watch the game last night? That referee was blind as hell!', 'Did anyone watch the game last night? That referee was blind as ****!', 1, '2023-02-05 15:25:00'),
(7, 7, 'Yeah, that call in the fourth quarter was terrible.', 'Yeah, that call in the fourth quarter was terrible.', 0, '2023-02-05 15:30:00'),
(7, 3, 'The replay clearly showed it was out of bounds. I don''t know what he was thinking.', 'The replay clearly showed it was out of bounds. I don''t know what he was thinking.', 0, '2023-02-05 15:35:00'),
(7, 1, 'I missed it. Is there a highlight reel somewhere?', 'I missed it. Is there a highlight reel somewhere?', 0, '2023-02-05 15:40:00'),
(7, 5, 'I''ll send you a link, John. The whole fourth quarter was intense.', 'I''ll send you a link, John. The whole fourth quarter was intense.', 0, '2023-02-05 15:45:00'),
(7, 7, 'Anyone going to watch the championship next weekend?', 'Anyone going to watch the championship next weekend?', 0, '2023-02-05 15:50:00'),
(7, 3, 'Definitely! I''ve already planned a watch party at my place. You''re all invited!', 'Definitely! I''ve already planned a watch party at my place. You''re all invited!', 0, '2023-02-05 15:55:00');

-- Add messages for Thread 10: Book Club
INSERT INTO ThreadParticipants (ThreadId, UserId, JoinedAt)
VALUES
-- Thread 10: Book Club (Sarah, Jane, Emma, Olivia, Lisa)
(10, 4, '2023-02-20'), -- Sarah (creator)
(10, 2, '2023-02-20'), -- Jane
(10, 8, '2023-02-20'), -- Emma
(10, 10, '2023-02-20'), -- Olivia
(10, 6, '2023-02-20'); -- Lisa

INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(10, 4, 'Welcome to our virtual book club! I thought we could start with "The Midnight Library" by Matt Haig. Any thoughts?', 'Welcome to our virtual book club! I thought we could start with "The Midnight Library" by Matt Haig. Any thoughts?', 0, '2023-02-20 19:00:00'),
(10, 2, 'I''ve heard great things about that book! I''m in.', 'I''ve heard great things about that book! I''m in.', 0, '2023-02-20 19:05:00'),
(10, 8, 'I just finished it last month. It''s fantastic!', 'I just finished it last month. It''s fantastic!', 0, '2023-02-20 19:10:00'),
(10, 10, 'I haven''t read it yet, but I''m excited to start. What''s the timeline?', 'I haven''t read it yet, but I''m excited to start. What''s the timeline?', 0, '2023-02-20 19:15:00'),
(10, 4, 'How about we give everyone two weeks to read it? We can discuss on March 6th.', 'How about we give everyone two weeks to read it? We can discuss on March 6th.', 0, '2023-02-20 19:20:00'),
(10, 6, 'Two weeks works for me. I''ve been meaning to read more fiction.', 'Two weeks works for me. I''ve been meaning to read more fiction.', 0, '2023-02-20 19:25:00'),
(10, 2, 'Perfect! I just ordered the book online.', 'Perfect! I just ordered the book online.', 0, '2023-02-20 19:30:00'),
(10, 8, 'Since I''ve already read it, I''ll prepare some discussion questions if that helps.', 'Since I''ve already read it, I''ll prepare some discussion questions if that helps.', 0, '2023-02-20 19:35:00'),
(10, 4, 'That would be wonderful, Emma. Thanks!', 'That would be wonderful, Emma. Thanks!', 0, '2023-02-20 19:40:00'),
(10, 10, 'I''m so excited! I haven''t been in a book club since college.', 'I''m so excited! I haven''t been in a book club since college.', 0, '2023-02-20 19:45:00'),
(10, 6, 'Me neither. This is going to be fun!', 'Me neither. This is going to be fun!', 0, '2023-02-20 19:50:00'),
(10, 4, 'I''ll create a calendar invite for our discussion. Happy reading, everyone!', 'I''ll create a calendar invite for our discussion. Happy reading, everyone!', 0, '2023-02-20 19:55:00');

-- Add messages for Thread 15: Tech News 
INSERT INTO ThreadParticipants (ThreadId, UserId, JoinedAt)
VALUES
-- Thread 15: Tech News (Alex, Mike, David, John, Michael)
(15, 5, '2023-03-10'), -- Alex (creator)
(15, 3, '2023-03-10'), -- Mike
(15, 7, '2023-03-10'), -- David
(15, 1, '2023-03-10'), -- John
(15, 9, '2023-03-11'); -- Michael

INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(15, 5, 'Hey tech enthusiasts! I created this thread to share and discuss the latest in tech news.', 'Hey tech enthusiasts! I created this thread to share and discuss the latest in tech news.', 0, '2023-03-10 10:00:00'),
(15, 3, 'Great idea! Did you all see the news about the new AI model that can generate code?', 'Great idea! Did you all see the news about the new AI model that can generate code?', 0, '2023-03-10 10:05:00'),
(15, 7, 'Yeah, it''s impressive but also a bit concerning for us developers.', 'Yeah, it''s impressive but also a bit concerning for us developers.', 0, '2023-03-10 10:10:00'),
(15, 1, 'I think it''s more of a tool that will enhance productivity rather than replace developers.', 'I think it''s more of a tool that will enhance productivity rather than replace developers.', 0, '2023-03-10 10:15:00'),
(15, 5, 'Agreed. It''s like when calculators were invented - they didn''t replace mathematicians.', 'Agreed. It''s like when calculators were invented - they didn''t replace mathematicians.', 0, '2023-03-10 10:20:00'),
(15, 3, 'Has anyone tried it yet? The waitlist is huge.', 'Has anyone tried it yet? The waitlist is huge.', 0, '2023-03-10 10:25:00'),
(15, 7, 'I got access last week. It''s pretty damn impressive what it can do.', 'I got access last week. It''s pretty **** impressive what it can do.', 1, '2023-03-10 10:30:00'),
(15, 5, 'Really? Can you show us some examples?', 'Really? Can you show us some examples?', 0, '2023-03-10 10:35:00'),
(15, 7, 'Sure, I''ll share some screenshots later today.', 'Sure, I''ll share some screenshots later today.', 0, '2023-03-10 10:40:00'),
(15, 9, 'Hey everyone, just joined the thread. I''ve also been testing the new AI. It''s a game-changer.', 'Hey everyone, just joined the thread. I''ve also been testing the new AI. It''s a game-changer.', 0, '2023-03-11 09:00:00'),
(15, 1, 'Welcome Michael! Would love to hear your thoughts on how it compares to existing tools.', 'Welcome Michael! Would love to hear your thoughts on how it compares to existing tools.', 0, '2023-03-11 09:05:00'),
(15, 9, 'It''s far more intuitive and requires less prompt engineering than previous models.', 'It''s far more intuitive and requires less prompt engineering than previous models.', 0, '2023-03-11 09:10:00');

-- Add messages for Thread 21: Car Enthusiasts
INSERT INTO ThreadParticipants (ThreadId, UserId, JoinedAt)
VALUES
-- Thread 21: Car Enthusiasts (David, John, Alex, Mike)
(21, 7, '2023-04-05'), -- David (creator)
(21, 1, '2023-04-05'), -- John
(21, 5, '2023-04-05'), -- Alex
(21, 3, '2023-04-06'); -- Mike

INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(21, 7, 'Welcome fellow car enthusiasts! What are you all driving these days?', 'Welcome fellow car enthusiasts! What are you all driving these days?', 0, '2023-04-05 17:00:00'),
(21, 1, 'I have a Tesla Model 3. Electric all the way!', 'I have a Tesla Model 3. Electric all the way!', 0, '2023-04-05 17:05:00'),
(21, 5, 'Still loving my classic 1967 Mustang. Nothing beats the sound of that engine.', 'Still loving my classic 1967 Mustang. Nothing beats the sound of that engine.', 0, '2023-04-05 17:10:00'),
(21, 7, 'Nice choices! I''m driving a BMW M3. It''s a beast on the track.', 'Nice choices! I''m driving a BMW M3. It''s a beast on the track.', 0, '2023-04-05 17:15:00'),
(21, 1, 'How''s the maintenance on that BMW? I''ve heard they can be expensive to keep up.', 'How''s the maintenance on that BMW? I''ve heard they can be expensive to keep up.', 0, '2023-04-05 17:20:00'),
(21, 7, 'It''s not cheap, that''s for sure. But it''s worth every penny when you hit the gas.', 'It''s not cheap, that''s for sure. But it''s worth every penny when you hit the gas.', 0, '2023-04-05 17:25:00'),
(21, 5, 'Alex, how much work goes into maintaining that Mustang?', 'Alex, how much work goes into maintaining that Mustang?', 0, '2023-04-05 17:30:00'),
(21, 5, 'It''s a labor of love. I spend most weekends tinkering with something. The carburetor can be a real bitch to tune properly.', 'It''s a labor of love. I spend most weekends tinkering with something. The carburetor can be a real **** to tune properly.', 1, '2023-04-05 17:35:00'),
(21, 3, 'Hey everyone, just joined the thread. I drive a Subaru WRX. Perfect for the mountain roads around here.', 'Hey everyone, just joined the thread. I drive a Subaru WRX. Perfect for the mountain roads around here.', 0, '2023-04-06 09:00:00'),
(21, 7, 'Welcome Mike! Those WRXs are fantastic in the snow too.', 'Welcome Mike! Those WRXs are fantastic in the snow too.', 0, '2023-04-06 09:05:00'),
(21, 3, 'Absolutely. It handles like a dream in all conditions.', 'Absolutely. It handles like a dream in all conditions.', 0, '2023-04-06 09:10:00'),
(21, 1, 'Anyone planning to attend the car show next month? I heard it''s going to be huge.', 'Anyone planning to attend the car show next month? I heard it''s going to be huge.', 0, '2023-04-06 09:15:00');

-- Add more UserWarnings for the new conversations
INSERT INTO UserWarnings (UserId, ThreadId, WarningMessage, CreatedAt)
VALUES
(10, 4, 'Please avoid using inappropriate language when discussing professional matters.', '2023-01-20 14:00:00'),
(5, 7, 'Your message contained inappropriate language. Please be more mindful in sports discussions.', '2023-02-05 15:26:00'),
(7, 15, 'Warning for casual use of inappropriate language. Please remember this is a professional space.', '2023-03-10 10:31:00'),
(5, 21, 'Please avoid using offensive terms when describing mechanical challenges.', '2023-04-05 17:36:00');

-- Add participants and messages for Thread 25: Art Club
INSERT INTO ThreadParticipants (ThreadId, UserId, JoinedAt)
VALUES
-- Thread 25: Art Club (Emma, Sarah, Olivia, Lisa, Jane)
(25, 8, '2023-04-20'), -- Emma (creator)
(25, 4, '2023-04-20'), -- Sarah
(25, 10, '2023-04-20'), -- Olivia
(25, 6, '2023-04-21'), -- Lisa
(25, 2, '2023-04-22'); -- Jane

INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(25, 8, 'Welcome to the Art Club! This is a space to share our artwork, discuss techniques, and inspire each other.', 'Welcome to the Art Club! This is a space to share our artwork, discuss techniques, and inspire each other.', 0, '2023-04-20 14:00:00'),
(25, 4, 'Thanks for creating this, Emma! I''ve been getting into watercolors lately.', 'Thanks for creating this, Emma! I''ve been getting into watercolors lately.', 0, '2023-04-20 14:05:00'),
(25, 10, 'I love photography and digital art. Looking forward to sharing some of my work!', 'I love photography and digital art. Looking forward to sharing some of my work!', 0, '2023-04-20 14:10:00'),
(25, 8, 'That''s great! I''m primarily into oil painting, but I want to explore different mediums too.', 'That''s great! I''m primarily into oil painting, but I want to explore different mediums too.', 0, '2023-04-20 14:15:00'),
(25, 4, 'Here''s my latest watercolor: [Attachment: sunset_watercolor.jpg]', 'Here''s my latest watercolor: [Attachment: sunset_watercolor.jpg]', 0, '2023-04-20 14:20:00'),
(25, 8, 'Sarah, that''s beautiful! The colors are so vibrant.', 'Sarah, that''s beautiful! The colors are so vibrant.', 0, '2023-04-20 14:25:00'),
(25, 10, 'I love how you captured the reflection on the water. What paper are you using?', 'I love how you captured the reflection on the water. What paper are you using?', 0, '2023-04-20 14:30:00'),
(25, 4, 'Thank you both! I''m using Arches cold press, 140lb. Makes a huge difference.', 'Thank you both! I''m using Arches cold press, 140lb. Makes a huge difference.', 0, '2023-04-20 14:35:00'),
(25, 6, 'Hello artists! Just joined the group. I do a lot of ceramic work and some sketching.', 'Hello artists! Just joined the group. I do a lot of ceramic work and some sketching.', 0, '2023-04-21 10:00:00'),
(25, 8, 'Welcome Lisa! We''d love to see some of your ceramic pieces.', 'Welcome Lisa! We''d love to see some of your ceramic pieces.', 0, '2023-04-21 10:05:00'),
(25, 6, 'Here''s a set of mugs I finished last week: [Attachment: ceramic_mugs.jpg]', 'Here''s a set of mugs I finished last week: [Attachment: ceramic_mugs.jpg]', 0, '2023-04-21 10:10:00'),
(25, 2, 'Just joined! Those mugs are fantastic, Lisa. The glazing is so even. How the hell did you get that perfect finish?', 'Just joined! Those mugs are fantastic, Lisa. The glazing is so even. How the **** did you get that perfect finish?', 1, '2023-04-22 15:00:00');

-- Add participants and messages for Thread 27: Mod Announcements
INSERT INTO ThreadParticipants (ThreadId, UserId, JoinedAt)
VALUES
-- Thread 27: Mod Announcements (Michael, John, Jane, all users)
(27, 9, '2023-05-05'), -- Michael (creator)
(27, 1, '2023-05-05'), -- John
(27, 2, '2023-05-05'), -- Jane
(27, 3, '2023-05-05'), -- Mike
(27, 4, '2023-05-05'), -- Sarah
(27, 5, '2023-05-05'), -- Alex
(27, 6, '2023-05-05'), -- Lisa
(27, 7, '2023-05-05'), -- David
(27, 8, '2023-05-05'), -- Emma
(27, 10, '2023-05-05'); -- Olivia

INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(27, 9, 'IMPORTANT ANNOUNCEMENT: We''ve updated our community guidelines. Please take a moment to review them at the link below.', 'IMPORTANT ANNOUNCEMENT: We''ve updated our community guidelines. Please take a moment to review them at the link below.', 0, '2023-05-05 11:00:00'),
(27, 9, 'The main changes include stricter policies on inappropriate language and a new three-strike system for violations.', 'The main changes include stricter policies on inappropriate language and a new three-strike system for violations.', 0, '2023-05-05 11:05:00'),
(27, 1, 'Thanks for the update, Michael. These changes should help maintain a positive environment.', 'Thanks for the update, Michael. These changes should help maintain a positive environment.', 0, '2023-05-05 11:10:00'),
(27, 9, 'We''re also introducing a new reporting feature that makes it easier to flag problematic content.', 'We''re also introducing a new reporting feature that makes it easier to flag problematic content.', 0, '2023-05-05 11:15:00'),
(27, 2, 'The moderation team will be hosting a Q&A session next Friday to address any questions about these changes.', 'The moderation team will be hosting a Q&A session next Friday to address any questions about these changes.', 0, '2023-05-05 11:20:00'),
(27, 3, 'Will there be any changes to how private threads are moderated?', 'Will there be any changes to how private threads are moderated?', 0, '2023-05-05 11:25:00'),
(27, 9, 'Good question, Mike. Private threads will still be moderated for reported content, but we won''t be proactively monitoring them.', 'Good question, Mike. Private threads will still be moderated for reported content, but we won''t be proactively monitoring them.', 0, '2023-05-05 11:30:00'),
(27, 5, 'What about the filtered words list? Is that being expanded?', 'What about the filtered words list? Is that being expanded?', 0, '2023-05-05 11:35:00'),
(27, 2, 'Yes, we''ve added about 20 new terms to the filter at various severity levels.', 'Yes, we''ve added about 20 new terms to the filter at various severity levels.', 0, '2023-05-05 11:40:00'),
(27, 7, 'This is getting ridiculous. Soon we won''t be able to say anything without it being fucking censored.', 'This is getting ridiculous. Soon we won''t be able to say anything without it being ******* censored.', 1, '2023-05-05 11:45:00'),
(27, 9, '@david_brown Please note that this type of comment is exactly what the new guidelines address. You''ve received a warning.', '@david_brown Please note that this type of comment is exactly what the new guidelines address. You''ve received a warning.', 0, '2023-05-05 11:50:00'),
(27, 7, 'Sorry about that. I''ll be more mindful of my language.', 'Sorry about that. I''ll be more mindful of my language.', 0, '2023-05-05 11:55:00');

-- Add warning for Jane in Art Club thread and David in Mod Announcements
INSERT INTO UserWarnings (UserId, ThreadId, WarningMessage, CreatedAt)
VALUES
(2, 25, 'Please be mindful of casual profanity even when expressing enthusiasm or admiration.', '2023-04-22 15:01:00'),
(7, 27, 'Warning for using severe profanity in a thread meant for official announcements. This violates our community guidelines.', '2023-05-05 11:46:00');

-- Add participants and messages for Thread 30: Mental Health Support
INSERT INTO ThreadParticipants (ThreadId, UserId, JoinedAt)
VALUES
-- Thread 30: Mental Health Support (Olivia, Sarah, Emma, Jane, Lisa)
(30, 10, '2023-06-01'), -- Olivia (creator)
(30, 4, '2023-06-01'), -- Sarah
(30, 8, '2023-06-01'), -- Emma
(30, 2, '2023-06-02'), -- Jane
(30, 6, '2023-06-03'); -- Lisa

INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(30, 10, 'I created this thread as a safe space to discuss mental health, share resources, and support each other.', 'I created this thread as a safe space to discuss mental health, share resources, and support each other.', 0, '2023-06-01 20:00:00'),
(30, 10, 'Remember, this isn''t a replacement for professional help, but sometimes just talking helps.', 'Remember, this isn''t a replacement for professional help, but sometimes just talking helps.', 0, '2023-06-01 20:01:00'),
(30, 4, 'Thank you for creating this, Olivia. I think a lot of us could use this space.', 'Thank you for creating this, Olivia. I think a lot of us could use this space.', 0, '2023-06-01 20:05:00'),
(30, 8, 'This is wonderful. I''ve been practicing mindfulness meditation lately and it''s made a big difference for my anxiety.', 'This is wonderful. I''ve been practicing mindfulness meditation lately and it''s made a big difference for my anxiety.', 0, '2023-06-01 20:10:00'),
(30, 10, 'Emma, would you mind sharing some resources for getting started with meditation?', 'Emma, would you mind sharing some resources for getting started with meditation?', 0, '2023-06-01 20:15:00'),
(30, 8, 'Of course! I use the Headspace app, but there are also free YouTube videos by "Yoga with Adriene" that are great for beginners.', 'Of course! I use the Headspace app, but there are also free YouTube videos by "Yoga with Adriene" that are great for beginners.', 0, '2023-06-01 20:20:00'),
(30, 4, 'I''ve been struggling with burnout from work lately. Any tips for maintaining boundaries?', 'I''ve been struggling with burnout from work lately. Any tips for maintaining boundaries?', 0, '2023-06-01 20:25:00'),
(30, 10, 'That''s tough, Sarah. I''ve found that setting strict "no work email" hours has helped me a lot.', 'That''s tough, Sarah. I''ve found that setting strict "no work email" hours has helped me a lot.', 0, '2023-06-01 20:30:00'),
(30, 8, 'Also, schedule your breaks like you would meetings. They''re just as important!', 'Also, schedule your breaks like you would meetings. They''re just as important!', 0, '2023-06-01 20:35:00'),
(30, 2, 'Hi everyone, just joined. I wanted to share a great podcast I found called "The Happiness Lab" - it''s based on science and very insightful.', 'Hi everyone, just joined. I wanted to share a great podcast I found called "The Happiness Lab" - it''s based on science and very insightful.', 0, '2023-06-02 09:00:00'),
(30, 10, 'Thanks for the recommendation, Jane! I''ll check it out on my commute tomorrow.', 'Thanks for the recommendation, Jane! I''ll check it out on my commute tomorrow.', 0, '2023-06-02 09:05:00'),
(30, 6, 'Hello everyone. I just wanted to say that some days are just shit, and that''s okay too. We don''t always have to be positive.', 'Hello everyone. I just wanted to say that some days are just ****, and that''s okay too. We don''t always have to be positive.', 1, '2023-06-03 15:00:00'),
(30, 2, 'That''s a really important point, Lisa. Toxic positivity can be harmful. It''s okay to acknowledge when things are difficult.', 'That''s a really important point, Lisa. Toxic positivity can be harmful. It''s okay to acknowledge when things are difficult.', 0, '2023-06-03 15:05:00'),
(30, 4, 'Absolutely. Sometimes just sitting with the difficult emotions rather than trying to push them away is what we need.', 'Absolutely. Sometimes just sitting with the difficult emotions rather than trying to push them away is what we need.', 0, '2023-06-03 15:10:00'),
(30, 10, 'I think that''s what makes this space valuable - we can be authentic about our struggles.', 'I think that''s what makes this space valuable - we can be authentic about our struggles.', 0, '2023-06-03 15:15:00');

-- Add participants and messages for Thread 29: Beauty Tips
INSERT INTO ThreadParticipants (ThreadId, UserId, JoinedAt)
VALUES
-- Thread 29: Beauty Tips (Olivia, Jane, Sarah, Emma, Lisa)
(29, 10, '2023-05-25'), -- Olivia (creator)
(29, 2, '2023-05-25'), -- Jane
(29, 4, '2023-05-25'), -- Sarah
(29, 8, '2023-05-26'), -- Emma
(29, 6, '2023-05-26'); -- Lisa

INSERT INTO MessageHistory (ThreadId, UserId, OriginalMessage, ModeratedMessage, WasModified, CreatedAt)
VALUES
(29, 10, 'Welcome to the Beauty Tips thread! Let''s share our favorite products, routines, and advice.', 'Welcome to the Beauty Tips thread! Let''s share our favorite products, routines, and advice.', 0, '2023-05-25 16:00:00'),
(29, 2, 'I''ve been loving The Ordinary''s Niacinamide serum. Great for oily skin and affordable!', 'I''ve been loving The Ordinary''s Niacinamide serum. Great for oily skin and affordable!', 0, '2023-05-25 16:05:00'),
(29, 10, 'Yes! Their hyaluronic acid is amazing too for hydration.', 'Yes! Their hyaluronic acid is amazing too for hydration.', 0, '2023-05-25 16:10:00'),
(29, 4, 'Has anyone tried any good sunscreens that don''t leave a white cast? I''m struggling to find one I like.', 'Has anyone tried any good sunscreens that don''t leave a white cast? I''m struggling to find one I like.', 0, '2023-05-25 16:15:00'),
(29, 10, 'La Roche-Posay Anthelios is my go-to. It''s a bit pricey but worth it.', 'La Roche-Posay Anthelios is my go-to. It''s a bit pricey but worth it.', 0, '2023-05-25 16:20:00'),
(29, 2, 'I love Supergoop Unseen Sunscreen. It feels like a primer and works well under makeup.', 'I love Supergoop Unseen Sunscreen. It feels like a primer and works well under makeup.', 0, '2023-05-25 16:25:00'),
(29, 4, 'Thanks for the recommendations! I''ll check those out.', 'Thanks for the recommendations! I''ll check those out.', 0, '2023-05-25 16:30:00'),
(29, 8, 'Hello beauty lovers! I recently switched to a more minimalist skincare routine and my skin has never been better.', 'Hello beauty lovers! I recently switched to a more minimalist skincare routine and my skin has never been better.', 0, '2023-05-26 10:00:00'),
(29, 10, 'What does your routine include now, Emma?', 'What does your routine include now, Emma?', 0, '2023-05-26 10:05:00'),
(29, 8, 'Just cleanser, moisturizer, and sunscreen. I cut out all the extra serums and toners and my skin is less irritated.', 'Just cleanser, moisturizer, and sunscreen. I cut out all the extra serums and toners and my skin is less irritated.', 0, '2023-05-26 10:10:00'),
(29, 6, 'I''ve been trying to find a good cruelty-free mascara. The one I''m using now is a clumpy mess.', 'I''ve been trying to find a good cruelty-free mascara. The one I''m using now is a clumpy mess.', 0, '2023-05-26 10:15:00'),
(29, 2, 'Damn, that''s annoying. I recommend Ilia''s mascara - it''s clean, cruelty-free, and doesn''t clump.', '****, that''s annoying. I recommend Ilia''s mascara - it''s clean, cruelty-free, and doesn''t clump.', 1, '2023-05-26 10:20:00'),
(29, 6, 'Thanks, Jane! I''ll give that one a try.', 'Thanks, Jane! I''ll give that one a try.', 0, '2023-05-26 10:25:00'),
(29, 10, 'If anyone has questions about specific skin concerns, feel free to ask! I studied cosmetology before switching careers.', 'If anyone has questions about specific skin concerns, feel free to ask! I studied cosmetology before switching careers.', 0, '2023-05-26 10:30:00'),
(29, 4, 'That''s great to know, Olivia! I might take you up on that offer soon.', 'That''s great to know, Olivia! I might take you up on that offer soon.', 0, '2023-05-26 10:35:00');

-- Add warning for Lisa and Jane in the recent threads
INSERT INTO UserWarnings (UserId, ThreadId, WarningMessage, CreatedAt)
VALUES
(6, 30, 'Please be mindful of language even when expressing frustration. Try using alternative words.', '2023-06-03 15:01:00'),
(2, 29, 'Casual profanity detected. Please maintain a clean communication style in all discussions.', '2023-05-26 10:21:00');

-- End of initialization data 