using System;

namespace ClearChat.Core.Domain
{
    public class ChatMessage
    {
        public ChatMessage(string userId,
                           string channelName, 
                           string message,
                           DateTime timeStampUtc)
        {
            UserId = userId;
            ChannelName = channelName;
            Message = message;
            TimeStampUtc = timeStampUtc;
        }

        public string UserId { get; }
        public string ChannelName { get; }
        public string Message { get; }
        public DateTime TimeStampUtc { get; }
    }
}