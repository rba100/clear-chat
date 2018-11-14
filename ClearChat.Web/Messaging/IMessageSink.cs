using ClearChat.Core.Domain;

namespace ClearChat.Web.Messaging
{
    public interface IMessageSink
    {
        /// <summary>
        /// Publish a message.
        /// </summary>
        /// <param name="message"></param>
        void Publish(ChatMessage message);

        /// <summary>
        /// Publish a transient message that is displayed to user(s) but not persisted.
        /// </summary>
        void PublishSystemMessage(string message, string channelId, MessageScope messageScope);
    }

    public enum MessageScope { All, Caller }
}
