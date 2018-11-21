using ClearChat.Core.Domain;

namespace ClearChat.Core
{
    /// <summary>
    /// Defines a message publisher.
    /// </summary>
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
        /// <remarks>
        /// Calling this method with MessageScope.Caller is not thread safe and must be called from
        /// the original worker thread handling an inbound request.
        /// </remarks>
        void PublishSystemMessage(string message, MessageScope messageScope);

        void ChangeChannel(string channel);
    }
}
