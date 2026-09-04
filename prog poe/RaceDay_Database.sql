/*
    RaceDay Database - Part 1
    SQL Server / SSMS
    Schema intentionally matches RaceDay_ERD.png exactly.

    Run this script on a clean SQL Server instance.
*/

IF DB_ID(N'RaceDay') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
END
GO

CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

CREATE TABLE Roles
(
    RoleID INT IDENTITY(1,1) NOT NULL,
    RoleName NVARCHAR(30) NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (RoleID),
    CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
);
GO

CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    RoleID INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Users PRIMARY KEY (UserID),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT FK_Users_Roles
        FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

CREATE TABLE Organisers
(
    OrganiserID INT NOT NULL,
    CONSTRAINT PK_Organisers PRIMARY KEY (OrganiserID),
    CONSTRAINT FK_Organisers_Users
        FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
);
GO

CREATE TABLE Participants
(
    ParticipantID INT NOT NULL,
    CONSTRAINT PK_Participants PRIMARY KEY (ParticipantID),
    CONSTRAINT FK_Participants_Users
        FOREIGN KEY (ParticipantID) REFERENCES Users(UserID)
);
GO

CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) NOT NULL,
    EventName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    EventDate DATE NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Distance DECIMAL(6,2) NOT NULL,
    EventType NVARCHAR(20) NOT NULL,
    OrganiserID INT NOT NULL,

    CONSTRAINT PK_Events PRIMARY KEY (EventID),
    CONSTRAINT CK_Events_Distance CHECK (Distance > 0),
    CONSTRAINT CK_Events_EventType CHECK (EventType IN (N'Run', N'Walk', N'Cycle')),
    CONSTRAINT FK_Events_Organisers
        FOREIGN KEY (OrganiserID) REFERENCES Organisers(OrganiserID)
);
GO

CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) NOT NULL,
    EventID INT NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    MinimumAge INT NULL,
    MaximumAge INT NULL,
    Distance DECIMAL(6,2) NULL,

    CONSTRAINT PK_Categories PRIMARY KEY (CategoryID),
    CONSTRAINT CK_Categories_MinimumAge
        CHECK (MinimumAge IS NULL OR MinimumAge >= 0),
    CONSTRAINT CK_Categories_MaximumAge
        CHECK (MaximumAge IS NULL OR MaximumAge >= 0),
    CONSTRAINT CK_Categories_AgeRange
        CHECK (
            MinimumAge IS NULL OR
            MaximumAge IS NULL OR
            MinimumAge <= MaximumAge
        ),
    CONSTRAINT CK_Categories_Distance
        CHECK (Distance IS NULL OR Distance > 0),
    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventID) REFERENCES Events(EventID)
);
GO

CREATE TABLE Enrolments
(
    EnrolmentID INT IDENTITY(1,1) NOT NULL,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL
        CONSTRAINT DF_Enrolments_EnrolmentDate DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Enrolments PRIMARY KEY (EnrolmentID),
    CONSTRAINT UQ_Enrolments_Participant_Event
        UNIQUE (ParticipantID, EventID),
    CONSTRAINT FK_Enrolments_Participants
        FOREIGN KEY (ParticipantID) REFERENCES Participants(ParticipantID),
    CONSTRAINT FK_Enrolments_Events
        FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT FK_Enrolments_Categories
        FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);
GO

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) NOT NULL,
    EnrolmentID INT NOT NULL,
    FinishTime TIME(0) NOT NULL,
    FinishingPosition INT NOT NULL,

    CONSTRAINT PK_Results PRIMARY KEY (ResultID),
    CONSTRAINT UQ_Results_Enrolment UNIQUE (EnrolmentID),
    CONSTRAINT CK_Results_FinishingPosition CHECK (FinishingPosition > 0),
    CONSTRAINT FK_Results_Enrolments
        FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
);
GO

/* Seed roles */
INSERT INTO Roles (RoleName)
VALUES (N'Organiser'),
       (N'Participant');
GO

/*
    PasswordHash values below are sample bcrypt-style hashes for planning/demo data.
    The real Part 2 application must hash passwords before storage and must never
    store the original password.
*/
INSERT INTO Users
    (FirstName, LastName, Email, PasswordHash, RoleID)
VALUES
    (N'Nomsa', N'Mokoena', N'nomsa@raceday.test',
     N'$2a$11$example.organiser.hash.nomsa', 1),
    (N'David', N'Naidoo', N'david@raceday.test',
     N'$2a$11$example.organiser.hash.david', 1),
    (N'Lebo', N'Dlamini', N'lebo@raceday.test',
     N'$2a$11$example.participant.hash.lebo', 2),
    (N'Thabo', N'Nkosi', N'thabo@raceday.test',
     N'$2a$11$example.participant.hash.thabo', 2);
GO

INSERT INTO Organisers (OrganiserID)
SELECT UserID FROM Users WHERE Email IN
    (N'nomsa@raceday.test', N'david@raceday.test');
GO

INSERT INTO Participants (ParticipantID)
SELECT UserID FROM Users WHERE Email IN
    (N'lebo@raceday.test', N'thabo@raceday.test');
GO

/* Three events */
INSERT INTO Events
    (EventName, Description, EventDate, Location, Distance, EventType, OrganiserID)
VALUES
    (N'Johannesburg City Run',
     N'Annual road-running event through central Johannesburg.',
     '2026-11-15', N'Johannesburg CBD', 21.10, N'Run', 1),
    (N'Soweto Community Walk',
     N'Community-focused walking event suitable for a wide range of participants.',
     '2026-12-06', N'Soweto', 10.00, N'Walk', 2),
    (N'Sandton Cycle Challenge',
     N'Road cycling challenge around Sandton and surrounding areas.',
     '2027-01-17', N'Sandton', 42.00, N'Cycle', 1);
GO

/* Categories for every event */
INSERT INTO Categories
    (EventID, CategoryName, MinimumAge, MaximumAge, Distance)
VALUES
    (1, N'Under 20', 0, 19, NULL),
    (1, N'Senior', 20, NULL, NULL),
    (1, N'21km Open', NULL, NULL, 21.10),
    (2, N'Under 20', 0, 19, NULL),
    (2, N'Senior', 20, NULL, NULL),
    (2, N'10km Open', NULL, NULL, 10.00),
    (3, N'Under 30', 18, 29, NULL),
    (3, N'30 Plus', 30, NULL, NULL),
    (3, N'42km Open', NULL, NULL, 42.00);
GO

/* Sample participant enrolments */
INSERT INTO Enrolments
    (ParticipantID, EventID, CategoryID)
VALUES
    (3, 1, 2),
    (4, 1, 1),
    (3, 2, 5),
    (4, 3, 8);
GO

/* Sample results */
INSERT INTO Results
    (EnrolmentID, FinishTime, FinishingPosition)
VALUES
    (1, '01:48:32', 14),
    (2, '02:05:17', 31);
GO

/* Verification queries */
SELECT * FROM Roles;
SELECT * FROM Users;
SELECT * FROM Organisers;
SELECT * FROM Participants;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM Enrolments;
SELECT * FROM Results;
GO
