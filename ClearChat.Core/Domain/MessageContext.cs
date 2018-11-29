using System;

namespace ClearChat.Core.Domain
{
    /// <summary>
    /// Information about the current chat command being processed.
    /// </summary>
    public class MessageContext
    {
        public User User { get; }

        public string ChannelName { get; }

        public IMessageHub MessageHub { get; }

        public DateTime MessageTime { get; }

        public string Message { get; }

        public MessageContext(string message,
                           User user, 
                           string currentChannel, 
                           IMessageHub messageHub, 
                           DateTime messageTime)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            ChannelName = currentChannel ?? throw new ArgumentNullException(nameof(currentChannel));
            MessageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
            MessageTime = messageTime;
            Message = message;
        }
    }
}