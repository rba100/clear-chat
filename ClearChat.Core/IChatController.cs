using System;
using ClearChat.Core.Domain;

namespace ClearChat.Core
{
    public class ChatController : IMessageHub
    {
        private readonly IConnectionManager m_ConnectionManager;
        private readonly IChatContext m_ChatContext;

        public void Publish(ChatMessage message)
        {
            m_ChatContext.SignalChannel(message.ChannelName, "newMessage", message);
        }

        public void PublishSystemMessage(string connectionId, string message)
        {
            m_ChatContext.SignalConnection(connectionId,
                                           "newMessage",
                                           new ChatMessage("System", "system", message, "000000", DateTime.UtcNow));
        }

        public void ForceInitHistory(string connectionId, string channelName)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateChannelMembership(string connectionId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveChannelMembership(string connectionId, string channelName)
        {
            throw new System.NotImplementedException();
        }
    }
}