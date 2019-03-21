using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public class ChannelCachingMessageRepository : IMessageRepository
    {
        private readonly ConcurrentDictionary<int, List<string>> m_UserToChannelCache
            = new ConcurrentDictionary<int, List<string>>();

        private readonly ConcurrentDictionary<string, List<int>> m_ChannelToUserIdCache
            = new ConcurrentDictionary<string, List<int>>();

        private readonly IMessageRepository m_MessageRepository;
        private readonly IStringHasher m_StringHasher;

        public ChannelCachingMessageRepository(IMessageRepository messageRepository, 
                                        IStringHasher stringHasher)
        {
            m_MessageRepository = messageRepository;
            m_StringHasher = stringHasher;
        }

        public IReadOnlyCollection<ChatMessage> ChannelMessages(string channelName)
        {
            return m_MessageRepository.ChannelMessages(channelName);
        }

        public ChatMessage WriteMessage(int userId, string channelName, string message, DateTime timeStampUtc)
        {
            return m_MessageRepository.WriteMessage(userId, channelName, message, timeStampUtc);
        }

        public void ClearChannel(string channelName)
        {
            m_MessageRepository.ClearChannel(channelName);
        }

        public int AddAttachment(int messageId, string contentType, byte[] content)
        {
            return m_MessageRepository.AddAttachment(messageId, contentType, content);
        }

        public IReadOnlyCollection<MessageAttachment> GetAttachments(IReadOnlyCollection<int> messageIds)
        {
            return m_MessageRepository.GetAttachments(messageIds);
        }

        public MessageAttachment GetAttachment(int attachmentId)
        {
            return m_MessageRepository.GetAttachment(attachmentId);
        }

        public void DeleteAttachment(int messageAttachmentId)
        {
            m_MessageRepository.DeleteAttachment(messageAttachmentId);
        }

        public SwitchChannelResult GetOrCreateChannel(string channelName, string channelPassword)
        {
            return m_MessageRepository.GetOrCreateChannel(channelName, channelPassword);
        }

        public void AddChannelMembership(int userId, string channelName)
        {
            if (m_UserToChannelCache.ContainsKey(userId))
            {
                var list = m_UserToChannelCache[userId];
                if(!list.Contains(channelName)) list.Add(channelName);
            }
            if (m_ChannelToUserIdCache.ContainsKey(channelName))
            {
                var list = m_ChannelToUserIdCache[channelName];
                if (list.All(id => id != userId) )list.Add(userId);
            }
            m_MessageRepository.AddChannelMembership(userId, channelName);
        }

        public void RemoveChannelMembership(int userId, string channelName)
        {
            if (channelName == "default") return;
            if (m_ChannelToUserIdCache.ContainsKey(channelName))
            {
                m_ChannelToUserIdCache[channelName].RemoveAll(u => u == userId);
            }

            if (m_UserToChannelCache.ContainsKey(userId)) m_UserToChannelCache[userId].Remove(channelName);
            m_MessageRepository.RemoveChannelMembership(userId, channelName);
        }

        public Channel GetChannelInformation(string channelName)
        {
            return m_MessageRepository.GetChannelInformation(channelName);
        }

        public IReadOnlyCollection<string> GetChannelMembershipsForUser(int userId)
        {
            if (!m_UserToChannelCache.ContainsKey(userId))
            {
                m_UserToChannelCache[userId] = 
                    m_MessageRepository.GetChannelMembershipsForUser(userId).ToList();
            }
            return m_UserToChannelCache[userId].ToArray();
        }

        public IReadOnlyCollection<int> GetChannelMembershipsForChannel(string channelName)
        {
            if (!m_ChannelToUserIdCache.ContainsKey(channelName))
            {
                m_ChannelToUserIdCache[channelName] =
                    m_MessageRepository.GetChannelMembershipsForChannel(channelName).ToList();
            }
            return m_MessageRepository.GetChannelMembershipsForChannel(channelName);
        }

        public void DeleteMessage(int messageId)
        {
            m_MessageRepository.DeleteMessage(messageId);
        }

        public bool IsChannelPrivate(string channelName)
        {
            return m_MessageRepository.IsChannelPrivate(channelName);
        }

        public bool UserIsInChannel(User user, Channel channel)
        {
            if(m_UserToChannelCache.TryGetValue(user.Id, out var values))
            {
                if (values.Contains(channel.Name)) return true;
            }

            return m_MessageRepository.UserIsInChannel(user, channel);
        }
    }
}