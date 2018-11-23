using System;

namespace ClearChat.Core.Domain
{
    public interface IChatMessageFactory
    {
        ChatMessage Create(string userId, string message, string channelName, DateTime timeStampUtc);
    }
}