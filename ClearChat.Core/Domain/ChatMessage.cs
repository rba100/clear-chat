using System;

namespace ClearChat.Core.Domain
{
    public class ChatMessage
    {
        public ChatMessage(int id,
                           string userName,
                           string channelName, 
                           string message, 
                           int[] attachmentIds,
                           DateTime timeStampUtc)
        {
            Id = id;
            UserName = userName;
            ChannelName = channelName;
            Message = message;
            TimeStampUtc = timeStampUtc;
            AttachmentIds = attachmentIds;
        }

        public int Id { get; }
        public string UserName { get; }
        public string ChannelName { get; }
        public string Message { get; }
        public int[] AttachmentIds { get; }
        public DateTime TimeStampUtc { get; }
    }
}