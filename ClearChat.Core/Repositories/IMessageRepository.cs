using System.Collections.Generic;
using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public interface IMessageRepository
    {
        IReadOnlyCollection<ChatMessage> ChannelMessages(string channelName);
        void WriteMessage(ChatMessage message);
        void ClearChannel(string channelName);
        SwitchChannelResult GetOrCreateChannel(string channelName, string channelPassword);
    }
}