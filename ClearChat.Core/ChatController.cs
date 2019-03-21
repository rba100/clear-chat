using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;
using ClearChat.Core.Utility;

namespace ClearChat.Core
{
    public class ChatController : IMessageHub
    {
        private readonly IConnectionManager m_ConnectionManager;
        private readonly IMessageRepository m_MessageRepository;
        private readonly IChatContext m_ChatContext;
        private readonly IUserRepository m_UserRepository;
        private readonly IStringHasher m_StringHasher;

        public ChatController(IConnectionManager connectionManager,
                              IMessageRepository messageRepository,
                              IChatContext chatContext,
                              IUserRepository userRepository,
                              IStringHasher stringHasher)
        {
            m_ConnectionManager = connectionManager;
            m_MessageRepository = messageRepository;
            m_ChatContext = chatContext;
            m_UserRepository = userRepository;
            m_StringHasher = stringHasher;
        }

        public Task Publish(ChatMessage message)
        {
            return m_ChatContext.SignalChannel(message.ChannelName, "newMessage", message);
        }

        public void PublishSystemMessage(string connectionId, string message)
        {
            m_ChatContext.SignalConnection(connectionId,
                                           "newMessage",
                                           new ChatMessage(0, "System", "system", message, new int[0], DateTime.UtcNow));
        }

        public void SendChannelHistory(string channelName)
        {
            var messages = m_MessageRepository.ChannelMessages(channelName);
            m_ChatContext.SignalAll("channelHistory", channelName, messages);
        }

        public void SendChannelInformation(string connectionId, string channelName)
        {
            var usersOnline = m_ConnectionManager.GetUsers();
            var channel = m_MessageRepository.GetChannel(channelName);
            var memberships = m_MessageRepository.GetChannelMembershipsForChannel(channelName);

            var usersInChannelAndOnline = usersOnline.Select(m_UserRepository.GetUserDetails)
                                                     .Where(u => memberships.Contains(u.Id))
                                                     .Select(u => u.UserName)
                                                     .ToArray();

            m_ChatContext.SignalConnection(connectionId, "channelInformation", channel);
        }

        public void SendChannelHistory(string connectionId, string channelName)
        {
            var messages = m_MessageRepository.ChannelMessages(channelName);

            var task = m_ChatContext.SignalConnection(connectionId, "userDetails", messages.Select(m => m.UserName)
                                                     .Distinct()
                                                     .Select(m_UserRepository.GetUserDetails)
                                                     .ToArray());
            task.ContinueWith(_ =>
                m_ChatContext.SignalConnection(connectionId, "channelHistory", channelName, messages));
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

        public void PublishMessageDeleted(int messageId)
        {
            m_ChatContext.SignalAll("deleteMessage", messageId);
        }

        public void SendChannelList(string connectionId)
        {
            var userName = m_ConnectionManager.GetUserIdForConnection(connectionId);
            var user = m_UserRepository.GetUserDetails(userName);
            var channels = m_MessageRepository.GetChannelMembershipsForUser(user.Id);
            m_ChatContext.SignalConnection(connectionId, "channelMembership", channels);
        }

        public void UpdateChannelMembership(string connectionId)
        {
            var userName = m_ConnectionManager.GetUserIdForConnection(connectionId);
            var user = m_UserRepository.GetUserDetails(userName);
            var channels = m_MessageRepository.GetChannelMembershipsForUser(user.Id);
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