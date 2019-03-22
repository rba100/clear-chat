DROP TABLE [dbo].[AutoResponses];
GO
DROP TABLE [dbo].[ChannelMembership];
GO
DROP TABLE [dbo].[Channels];
GO
DROP TABLE [dbo].MessageAttachments;
GO
DROP TABLE [dbo].[Messages];
GO
DROP TABLE [dbo].[UserRoles];
GO
DROP TABLE [dbo].[Users];
GO

CREATE TABLE [dbo].[Users]
(
    [id] INT IDENTITY(1, 1) PRIMARY KEY,
    [userName] [NVARCHAR](256) NOT NULL,
    [passwordHash] [VARBINARY](256) NOT NULL,
    [passwordSalt] [VARBINARY](256) NOT NULL,
    [hexColour] [VARCHAR](6) NULL
);

INSERT INTO [dbo].[Users] ( userName,
                            passwordHash,
                            passwordSalt )
VALUES ( 'ClearBot', 0x0, 0x0 );

DECLARE @clearBotUserId INT;
SET @clearBotUserId = SCOPE_IDENTITY();

CREATE TABLE [dbo].[UserRoles]
(
    [id] INT IDENTITY(1, 1) PRIMARY KEY,
    [userId] INT
        FOREIGN KEY REFERENCES dbo.Users ( id ) NOT NULL,
    [grantedBy] INT
        FOREIGN KEY REFERENCES dbo.Users ( id ) NULL,
    [roleType] INT NOT NULL
);

INSERT INTO [dbo].[UserRoles] ( userId,
                                grantedBy,
                                roleType )
VALUES ( @clearBotUserId, -- userId - int
         NULL,            -- grantedBy - int
         0                -- roleType - int
    );

CREATE TABLE [dbo].[Messages]
(
    [id] [INT] IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [userId] INT NOT NULL,
    [channelId] INT NOT NULL,
    [message] [VARBINARY](MAX) NOT NULL,
    [timeStampUtc] [DATETIME2](7) NOT NULL
);

CREATE TABLE [dbo].[Channels]
(
    [id] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [channelName] NVARCHAR(256) NOT NULL,
    [passwordHash] [VARBINARY](256) NOT NULL,
    [passwordSalt] [VARBINARY](256) NOT NULL
);

CREATE TABLE [dbo].[ChannelMembership]
(
    [userId] INT NOT NULL,
    [channelId] INT NOT NULL,
    PRIMARY KEY (
        userId,
        channelId )
);

CREATE TABLE [dbo].[AutoResponses]
(
    [id] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [channelId] INT NOT NULL,
    [authorUserId] INT NOT NULL,
    [substring] [VARCHAR](1000) NOT NULL,
    [response] [VARCHAR](1000) NOT NULL,
    FOREIGN KEY ( [authorUserId] ) REFERENCES [dbo].[Users] ( id ),
    FOREIGN KEY ( [channelId] ) REFERENCES [dbo].[Channels] ( [id] ),
);

CREATE TABLE [dbo].MessageAttachments
(
    id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    messageId INT NOT NULL,
    contentType NVARCHAR(50) NOT NULL,
    content VARBINARY(MAX) NOT NULL
);
ALTER TABLE dbo.MessageAttachments
ADD CONSTRAINT FK_MessageId
    FOREIGN KEY ( messageId )
    REFERENCES Messages ( id );