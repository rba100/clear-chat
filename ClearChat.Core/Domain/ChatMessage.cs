using System;

namespace ClearChat.Core.Domain
{
    public class ChatMessage
    {
        public ChatMessage(int id,
                           string userId,
                           string channelName, 
                           string message, 
                           int[] attachmentIds,
                           DateTime timeStampUtc)
        {
            Id = id;
            UserId = userId;
            ChannelName = channelName;
            Message = message;
            TimeStampUtc = timeStampUtc;
            AttachmentIds = attachmentIds;
        }

        public int Id { get; }
        public string UserId { get; }
        public string ChannelName { get; }
        public string Message { get; }
        public int[] AttachmentIds { get; }
        public DateTime TimeStampUtc { get; }
    }
}