using System;

namespace ClearChat.Core.Domain
{
    /// <summary>
    /// Information about the current chat command being processed.
    /// </summary>
    public class ChatContext
    {
        public User User { get; }

        public string CurrentChannel { get; }

        public IMessageHub MessageHub { get; }

        public ChatContext(User user, 
                           string currentChannel, 
                           IMessageHub messageHub)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            CurrentChannel = currentChannel ?? throw new ArgumentNullException(nameof(currentChannel));
            MessageHub = messageHub ?? throw new ArgumentNullException(nameof(messageHub));
        }
    }
}