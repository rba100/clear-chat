using System;

namespace ClearChat.Core.Domain
{
    public class ChatMessage
    {
        public ChatMessage(string userId,
                           string channelName, 
                           string message,
                           string userIdColour,
                           DateTime timeStampUtc)
        {
            UserId = userId;
            ChannelName = channelName;
            Message = message;
            TimeStampUtc = timeStampUtc;
            UserIdColour = userIdColour;
        }

        public string UserId { get; }
        public string ChannelName { get; }
        public string Message { get; }
        public DateTime TimeStampUtc { get; }
        public string UserIdColour { get; }
    }
}