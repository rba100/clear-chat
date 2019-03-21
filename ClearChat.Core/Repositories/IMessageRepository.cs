using System;
using System.Collections.Generic;
using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public interface IMessageRepository
    {
        IReadOnlyCollection<ChatMessage> ChannelMessages(string channelName);
        ChatMessage WriteMessage(int userId, string channelName, string message, DateTime timeStampUtc);
        void ClearChannel(string channelName);
        int AddAttachment(int messageId, string contentType, byte[] content);
        IReadOnlyCollection<MessageAttachment> GetAttachments(IReadOnlyCollection<int> messageIds);
        MessageAttachment GetAttachment(int attachmentId);
        void DeleteAttachment(int messageAttachmentId);
        SwitchChannelResult GetOrCreateChannel(string channelName, string channelPassword);
        void AddChannelMembership(int userId, string channelName);
        void RemoveChannelMembership(int userId, string channelName);
        Channel GetChannelInformation(string channelName);
        IReadOnlyCollection<string> GetChannelMembershipsForUser(int userId);
        IReadOnlyCollection<int> GetChannelMembershipsForChannel(string channelName);
        void DeleteMessage(int messageId);
        bool IsChannelPrivate(string channelName);
        bool UserIsInChannel(User user, Channel channel);
    }
}