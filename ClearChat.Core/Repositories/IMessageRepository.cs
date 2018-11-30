using System;
using System.Collections.Generic;
using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public interface IMessageRepository
    {
        IReadOnlyCollection<ChatMessage> ChannelMessages(string channelName);
        ChatMessage WriteMessage(string userId, string channelName, string message, DateTime timeStampUtc);
        void ClearChannel(string channelName);
        SwitchChannelResult GetOrCreateChannel(string channelName, string channelPassword);
        void AddChannelMembership(string userId, string channelName);
        void RemoveChannelMembership(string userId, string channelName);
        IReadOnlyCollection<string> GetChannelMembershipsForUser(string userId);

        /// <summary>
        /// Gets userId hashes for a channel.
        /// </summary>
        IReadOnlyCollection<byte[]> GetChannelMembershipsForChannel(string channelName);
        void DeleteMessage(int messageId);
    }
}