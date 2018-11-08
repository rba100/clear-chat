using System.Collections.Generic;
using ClearChat.Models;

namespace ClearChat.Repositories
{
    public interface IMessageRepository
    {
        IReadOnlyCollection<ChatMessage> ChannelMessages(string channelId);
        void WriteMessage(ChatMessage message);
        void ClearChannel(string channelId);
    }
}