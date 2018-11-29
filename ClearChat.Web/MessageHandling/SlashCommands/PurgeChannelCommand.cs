using System;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    class PurgeChannelCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;

        public PurgeChannelCommand(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        public string CommandText => "purge";

        public void Handle(MessageContext context, string arguments)
        {
            var userId = context.User.UserId;
            var parts = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!parts.Any())
            {
                context.MessageHub.PublishSystemMessage("Error: correct usage is /purge channelName",
                                                        MessageScope.Caller);
                return;
            }

            var channelName = parts[0];

            var channels = m_MessageRepository.GetChannelMemberships(userId);
            if (!channels.Contains(channelName))
            {
                context.MessageHub.PublishSystemMessage("Error: you are not a member of that channel.",
                                                        MessageScope.Caller);
                return;
            }

            m_MessageRepository.ClearChannel(channelName);
            context.MessageHub.ForceInitHistory(channelName);
        }

        public string HelpText => "Destroy all messages in a channel with: /purge channelName";
    }
}