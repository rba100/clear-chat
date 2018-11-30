using System;

namespace ClearChat.Core.Domain
{
    public class ChatMessage
    {
        public ChatMessage(int id,
                           string userId,
                           string channelName, 
                           string message,
                           DateTime timeStampUtc)
        {
            Id = id;
            UserId = userId;
            ChannelName = channelName;
            Message = message;
            TimeStampUtc = timeStampUtc;
        }

        public int Id { get; }
        public string UserId { get; }
        public string ChannelName { get; }
        public string Message { get; }
        public DateTime TimeStampUtc { get; }
    }
}