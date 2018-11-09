using System;

namespace ClearChat.Core.Domain
{
    public class ChatMessage
    {
        public ChatMessage(string userId,
                           string channelId, 
                           string message,
                           DateTime timeStampUtc)
        {
            UserId = userId;
            ChannelId = channelId;
            Message = message;
            TimeStampUtc = timeStampUtc;
        }

        public string UserId { get; }
        public string ChannelId { get; }
        public string Message { get; }
        public DateTime TimeStampUtc { get; }
    }
}