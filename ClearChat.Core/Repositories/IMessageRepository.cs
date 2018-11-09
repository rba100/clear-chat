using System.Collections.Generic;
using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public interface IMessageRepository
    {
        IReadOnlyCollection<ChatMessage> ChannelMessages(string channelId);
        void WriteMessage(ChatMessage message);
        void ClearChannel(string channelId);
    }
}