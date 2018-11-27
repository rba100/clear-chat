-- TODO: Normalisation

CREATE TABLE [dbo].[Messages]
(
    [id] [INT] IDENTITY(1, 1) NOT NULL,
    [userId] [VARCHAR](1000) NOT NULL,
    [channelId] INT NOT NULL,
    [message] [VARBINARY](MAX) NOT NULL,
    [TimeStampUtc] [DATETIME2](7) NOT NULL
);

CREATE TABLE [dbo].[Users]
(
    [userIdHash] [VARBINARY](256) NOT NULL PRIMARY KEY,
    [passwordHash] [VARBINARY](256) NOT NULL,
    [passwordSalt] [VARBINARY](256) NOT NULL,
    [hexColour] [VARCHAR](6) NULL
);

CREATE TABLE [dbo].[Channels]
(
    [channelId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [channelNameHash] [VARBINARY](256) NOT NULL,
    [passwordHash] [VARBINARY](256) NOT NULL,
    [passwordSalt] [VARBINARY](256) NOT NULL
);

CREATE TABLE [dbo].[ChannelMembership]
(
    [id] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [userIdHash] [VARBINARY](256) NOT NULL,
    [channelName] [VARBINARY](MAX) NOT NULL
);