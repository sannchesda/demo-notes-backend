-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'NotesAppDB')
BEGIN
    CREATE DATABASE NotesAppDB;
END
GO

-- Use the database
USE NotesAppDB;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Email nvarchar(255) NOT NULL UNIQUE,
        PasswordHash nvarchar(255) NOT NULL,
        FirstName nvarchar(100) NOT NULL,
        LastName nvarchar(100) NOT NULL,
        CreatedAt datetime2(7) NOT NULL
    );
END
GO

-- Drop existing Notes table if it exists (to add UserId column)
IF EXISTS (SELECT * FROM sysobjects WHERE name='Notes' AND xtype='U')
BEGIN
    DROP TABLE Notes;
END
GO

-- Create Notes table with UserId
CREATE TABLE Notes (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Title nvarchar(255) NOT NULL,
    Content nvarchar(max) NULL,
    CreatedAt datetime2(7) NOT NULL,
    UpdatedAt datetime2(7) NOT NULL,
    UserId int NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
GO

-- Insert a demo user
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, CreatedAt) VALUES
('demo@example.com', '$2a$11$E1gNYGHBGTi8/VcNu8K9Guc0G8RKoOtVl7jQG3T9lZl.YOx1/KjfK', 'Demo', 'User', GETUTCDATE());
-- Password is: "password123"
GO

-- Get the demo user ID
DECLARE @DemoUserId int = (SELECT Id FROM Users WHERE Email = 'demo@example.com');

-- Insert sample notes for the demo user
INSERT INTO Notes (Title, Content, CreatedAt, UpdatedAt, UserId) VALUES
('Welcome to Your Notes App', 'Welcome to your personal notes app! This is a sample note to get you started. You can create, edit, and delete notes as needed.', GETUTCDATE(), GETUTCDATE(), @DemoUserId),
('Shopping List', 'Milk, Bread, Eggs, Butter, Coffee, Bananas, Yogurt', GETUTCDATE(), GETUTCDATE(), @DemoUserId),
('Meeting Notes', 'Project kickoff meeting scheduled for next Monday at 2 PM. Discuss project timeline, deliverables, and team assignments.', GETUTCDATE(), GETUTCDATE(), @DemoUserId);
GO
