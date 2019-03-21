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
DROP TABLE [dbo].[Users]; 
GO

CREATE TABLE [dbo].[Users]
(
    [id] INT IDENTITY(1,1) PRIMARY KEY,
    [userName] [NVARCHAR](256) NOT NULL,
    [passwordHash] [VARBINARY](256) NOT NULL,
    [passwordSalt] [VARBINARY](256) NOT NULL,
    [hexColour] [VARCHAR](6) NULL
);

INSERT INTO  [dbo].[Users] (userName, passwordHash, passwordSalt) VALUES ('ClearBot',0x0,0x0);

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
	PRIMARY KEY(userId, channelId)
);

CREATE TABLE [dbo].[AutoResponses]
(
    [id] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	[channelId] INT NOT NULL,
	[authorUserId] INT NOT NULL,
    [substring] [VARCHAR](1000) NOT NULL,
    [response] [VARCHAR](1000) NOT NULL,
    FOREIGN KEY ([authorUserId]) REFERENCES [dbo].[Users](id),
    FOREIGN KEY ([channelId]) REFERENCES [dbo].[Channels]([id]),
);

CREATE TABLE [dbo].MessageAttachments
(
	id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	messageId INT NOT NULL,
	contentType NVARCHAR(50) NOT NULL,
	content VARBINARY(max) NOT NULL
)
ALTER TABLE dbo.MessageAttachments ADD CONSTRAINT FK_MessageId FOREIGN KEY(MessageId) REFERENCES Messages(id)