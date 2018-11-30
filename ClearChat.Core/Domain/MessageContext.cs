﻿using System;

namespace ClearChat.Core.Domain
{
    /// <summary>
    /// Information about the current chat command being processed.
    /// </summary>
    public class MessageContext
    {
        public string UserId { get; }

        public string ConnectionId { get; }

        public string ChannelName { get; }

        public IMessageHub MessageHub { get; }

        public DateTime MessageTime { get; }

        public string Message { get; }

        public MessageContext(string message,
                              string userId,
                              string connectionId,
                              string currentChannel,
                              IMessageHub messageHub,
                              DateTime messageTime)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            ChannelName = currentChannel ?? throw new ArgumentNullException(nameof(currentChannel));
            MessageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
            MessageTime = messageTime;
            ConnectionId = connectionId;
            Message = message;
        }
    }
}