using System;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling
{
    class ChangeChannelCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;

        public ChangeChannelCommand(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        public string CommandText => "channel";

        public void Handle(MessageContext context, string arguments)
        {
            var parts = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!parts.Any())
            {
                context.MessageHub.PublishSystemMessage("Error: correct usage is /channel channelName channelPassword", 
                                                        MessageScope.Caller);
                return;
            }

            var channelName = parts[0];

            if (channelName.StartsWith("@"))
            {
                context.MessageHub.PublishSystemMessage("Error: channel names cannot start with '@'. This is a reserved character for direct messages.",
                                                        MessageScope.Caller);
                return;
            }

            var password = parts.Length > 1 ? parts[1] : string.Empty;
            var channelResult = m_MessageRepository.GetOrCreateChannel(channelName, password);
            switch (channelResult)
            {
                case SwitchChannelResult.Denied:
                    context.MessageHub.PublishSystemMessage("Error: wrong password channel",
                                                            MessageScope.Caller);
                    break;
                case SwitchChannelResult.Created:
                    context.MessageHub.ChangeChannel(channelName);
                    context.MessageHub.PublishSystemMessage($"You created channel '{channelName}'",
                                                            MessageScope.Caller);
                    break;
                case SwitchChannelResult.Accepted:
                    context.MessageHub.ChangeChannel(channelName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string HelpText => "Switch channel with: /channel channelName [password]";
    }
}