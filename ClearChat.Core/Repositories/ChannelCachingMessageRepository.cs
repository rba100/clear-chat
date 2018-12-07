﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public class ChannelCachingMessageRepository : IMessageRepository
    {
        private readonly ConcurrentDictionary<string, List<string>> m_UserToChannelCache
            = new ConcurrentDictionary<string, List<string>>();

        private readonly ConcurrentDictionary<string, List<byte[]>> m_ChannelToUserHashCache
            = new ConcurrentDictionary<string, List<byte[]>>();

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

        public ChatMessage WriteMessage(string userId, string channelName, string message, DateTime timeStampUtc)
        {
            return m_MessageRepository.WriteMessage(userId, channelName, message, timeStampUtc);
        }

        public void ClearChannel(string channelName)
        {
            m_MessageRepository.ClearChannel(channelName);
        }

        public void AddAttachment(int messageId, ContentEncoding encoding, string contentType, byte[] content)
        {
            m_MessageRepository.AddAttachment(messageId, encoding, contentType, content);
        }

        public IReadOnlyCollection<MessageAttachment> GetAttachments(IReadOnlyCollection<int> messageIds)
        {
            return m_MessageRepository.GetAttachments(messageIds);
        }

        public void DeleteAttachment(int messageAttachmentId)
        {
            m_MessageRepository.DeleteAttachment(messageAttachmentId);
        }

        public SwitchChannelResult GetOrCreateChannel(string channelName, string channelPassword)
        {
            return m_MessageRepository.GetOrCreateChannel(channelName, channelPassword);
        }

        public void AddChannelMembership(string userId, string channelName)
        {
            if (m_UserToChannelCache.ContainsKey(userId))
            {
                var list = m_UserToChannelCache[userId];
                if(!list.Contains(channelName)) list.Add(channelName);
            }
            if (m_ChannelToUserHashCache.ContainsKey(channelName))
            {
                var userIdHash = m_StringHasher.Hash(userId);
                var list = m_ChannelToUserHashCache[channelName];
                if (!list.Any(bytes=>bytes.SequenceEqual(userIdHash))) list.Add(userIdHash);
            }
            m_MessageRepository.AddChannelMembership(userId, channelName);
        }

        public void RemoveChannelMembership(string userId, string channelName)
        {
            if (channelName == "default") return;
            var userIdHash = m_StringHasher.Hash(userId);
            if (m_ChannelToUserHashCache.ContainsKey(channelName))
            {
                m_ChannelToUserHashCache[channelName].RemoveAll(bytes=>bytes.SequenceEqual(userIdHash));
            }

            if (m_UserToChannelCache.ContainsKey(userId)) m_UserToChannelCache[userId].Remove(channelName);
            m_MessageRepository.RemoveChannelMembership(userId, channelName);
        }

        public IReadOnlyCollection<string> GetChannelMembershipsForUser(string userId)
        {
            if (!m_UserToChannelCache.ContainsKey(userId))
            {
                m_UserToChannelCache[userId] = 
                    m_MessageRepository.GetChannelMembershipsForUser(userId).ToList();
            }
            return m_UserToChannelCache[userId].ToArray();
        }

        public IReadOnlyCollection<byte[]> GetChannelMembershipsForChannel(string channelName)
        {
            if (!m_ChannelToUserHashCache.ContainsKey(channelName))
            {
                m_ChannelToUserHashCache[channelName] =
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
    }
}