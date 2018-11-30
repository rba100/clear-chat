using System;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Core
{
    public class ChatController : IMessageHub
    {
        private readonly IConnectionManager m_ConnectionManager;
        private readonly IMessageRepository m_MessageRepository;
        private readonly IChatContext m_ChatContext;
        private readonly IUserRepository m_UserRepository;

        public ChatController(IConnectionManager connectionManager,
                              IMessageRepository messageRepository,
                              IChatContext chatContext,
                              IUserRepository userRepository)
        {
            m_ConnectionManager = connectionManager;
            m_MessageRepository = messageRepository;
            m_ChatContext = chatContext;
            m_UserRepository = userRepository;
        }

        public void Publish(ChatMessage message)
        {
            m_ChatContext.SignalChannel(message.ChannelName, "newMessage", message);
        }

        public void PublishSystemMessage(string connectionId, string message)
        {
            m_ChatContext.SignalConnection(connectionId,
                                           "newMessage",
                                           new ChatMessage("System", "system", message, DateTime.UtcNow));
        }

        public void SendChannelHistory(string connectionId, string channelName)
        {
            var messages = m_MessageRepository.ChannelMessages(channelName);

            var colouredMessages = messages.Select(m => new ChatMessage(m.UserId, m.ChannelName, m.Message, m.TimeStampUtc));
            var task = m_ChatContext.SignalConnection(connectionId, "userDetails", messages.Select(m => m.UserId)
                                                     .Distinct()
                                                     .Select(m_UserRepository.GetUserDetails)
                                                     .ToArray());
            task.ContinueWith(_ =>
                m_ChatContext.SignalConnection(connectionId, "channelHistory", channelName, colouredMessages));
        }

        public void PublishUserDetails(string connectionId, IReadOnlyCollection<string> userIds)
        {
            var users = userIds.Select(m_UserRepository.GetUserDetails);
            m_ChatContext.SignalConnection(connectionId, "userDetails", users);
        }

        public void PublishUserDetails(IReadOnlyCollection<string> userIds)
        {
            var users = userIds.Select(m_UserRepository.GetUserDetails).ToArray();
            m_ChatContext.SignalAll("userDetails", users);
        }

        public void SendChannelList(string connectionId)
        {
            var userId = m_ConnectionManager.GetUserIdForConnection(connectionId);
            var channels = m_MessageRepository.GetChannelMembershipsForUser(userId);
            m_ChatContext.SignalConnection(connectionId, "channelMembership", channels);
        }

        public void UpdateChannelMembership(string connectionId)
        {
            var userId = m_ConnectionManager.GetUserIdForConnection(connectionId);
            var channels = m_MessageRepository.GetChannelMembershipsForUser(userId);
            foreach (var channel in channels)
            {
                m_ChatContext.AddToGroup(connectionId, channel);
            }
            SendChannelList(connectionId);
        }

        public void RemoveChannelMembership(string connectionId, string channelName)
        {
            m_ChatContext.RemoveFromGroup(connectionId, channelName);
        }
    }
}