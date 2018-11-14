CREATE TABLE [dbo].[Messages]
(
    [id] [INT] IDENTITY(1, 1) NOT NULL,
    [userId] [VARCHAR](1000) NOT NULL,
    [channelId] [VARCHAR](1000) NOT NULL,
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
