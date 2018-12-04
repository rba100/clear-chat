using System.Collections.Generic;
using ClearChat.Core.Domain;

namespace ClearChat.Core
{
    /// <summary>
    /// Defines a message publisher.
    /// </summary>
    public interface IMessageHub
    {
        /// <summary>
        /// Publish a message.
        /// </summary>
        void Publish(ChatMessage message);

        /// <summary>
        /// Publish a transient message that is displayed to user(s) but not persisted.
        /// </summary>
        /// <remarks>
        /// Calling this method with MessageScope.Caller is not thread safe and must be called from
        /// the original worker thread handling an inbound request.
        /// </remarks>
        void PublishSystemMessage(string connectionId, string message);

        void UpdateChannelMembership(string connectionId);
        void RemoveChannelMembership(string connectionId, string channelName);
        void SendChannelHistory(string connectionId, string channelName);
        void SendChannelList(string connectionId);
        void PublishUserDetails(string connectionId, IReadOnlyCollection<string> userIds);
        void PublishUserDetails(IReadOnlyCollection<string> userIds);
        void PublishMessageDeleted(int messageId);
        void SendChannelHistory(string channelName);
    }
}
